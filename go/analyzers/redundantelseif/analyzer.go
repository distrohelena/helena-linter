// Package redundantelseif implements the redundant-else-if rule.
package redundantelseif

import (
	"bytes"
	"go/ast"
	"go/token"
	"go/types"
	"os"
	"unicode"

	"github.com/distrohelena/helena-linter/go/internal/diag"
	"github.com/distrohelena/helena-linter/go/internal/exprx"
	"github.com/distrohelena/helena-linter/go/internal/fixx"
	"golang.org/x/tools/go/analysis"
)

// Analyzer reports else-if branches whose condition is the exact complement of
// the parent if condition and therefore redundant.
var Analyzer = &analysis.Analyzer{
	Name: "redundantelseif",
	Doc:  "reports redundant else-if branches whose conditions exactly complement the parent if",
	Run:  run,
}

func run(pass *analysis.Pass) (any, error) {
	for _, file := range pass.Files {
		file := file

		tokenFile := pass.Fset.File(file.Pos())
		if tokenFile == nil {
			continue
		}

		source, err := os.ReadFile(tokenFile.Name())
		if err != nil {
			continue
		}

		ast.Inspect(file, func(n ast.Node) bool {
			stmt, ok := n.(*ast.IfStmt)
			if !ok || stmt == nil {
				return true
			}

			elseIf, ok := stmt.Else.(*ast.IfStmt)
			if !ok || elseIf == nil || elseIf.Init != nil || elseIf.Else != nil {
				return true
			}

			if !exprx.Complementary(stmt.Cond, elseIf.Cond, identEqual(pass.TypesInfo)) {
				return true
			}

			edit, ok := redundantElseIfEdit(pass.Fset, source, elseIf)
			if !ok {
				return true
			}

			pass.Report(analysis.Diagnostic{
				Pos:     elseIf.Pos(),
				Message: diag.Message(diag.RuleRedundantElseIf, "replace this else if with else"),
				SuggestedFixes: []analysis.SuggestedFix{
					{
						Message:   "Replace with else",
						TextEdits: []analysis.TextEdit{edit},
					},
				},
			})
			return true
		})
	}

	return nil, nil
}

func identEqual(info *types.Info) exprx.IdentEqualFunc {
	return func(left, right *ast.Ident) bool {
		if left == nil || right == nil {
			return false
		}

		if info != nil {
			leftObj := info.ObjectOf(left)
			rightObj := info.ObjectOf(right)
			if leftObj != nil && rightObj != nil {
				return leftObj == rightObj
			}
		}

		return left.Name == right.Name
	}
}

func redundantElseIfEdit(fset *token.FileSet, source []byte, ifStmt *ast.IfStmt) (analysis.TextEdit, bool) {
	if fset == nil || ifStmt == nil || ifStmt.Body == nil {
		return analysis.TextEdit{}, false
	}

	file := fset.File(ifStmt.Pos())
	if file == nil {
		return analysis.TextEdit{}, false
	}

	start := file.Offset(ifStmt.Pos())
	condStart := file.Offset(ifStmt.Cond.Pos())
	condEnd := file.Offset(ifStmt.Cond.End())
	bodyStart := file.Offset(ifStmt.Body.Lbrace)
	if start < 0 || condStart < 0 || condEnd < 0 || bodyStart < 0 {
		return analysis.TextEdit{}, false
	}
	if start > len(source) || condStart > len(source) || condEnd > len(source) || bodyStart > len(source) {
		return analysis.TextEdit{}, false
	}
	if start+len("if") > condStart || condStart > condEnd || condEnd > bodyStart {
		return analysis.TextEdit{}, false
	}

	replaceStart := start
	for replaceStart > 0 {
		switch source[replaceStart-1] {
		case ' ', '\t':
			replaceStart--
		default:
			goto startAdjusted
		}
	}

startAdjusted:
	prefix := bytes.TrimLeftFunc(source[start+len("if"):condStart], unicode.IsSpace)
	suffix := bytes.TrimLeftFunc(source[condEnd:bodyStart], unicode.IsSpace)

	replacement := make([]byte, 0, bodyStart-start)
	switch {
	case len(prefix) == 0 && len(suffix) == 0:
		replacement = append(replacement, ' ')
	case len(prefix) == 0:
		replacement = append(replacement, ' ')
		replacement = append(replacement, suffix...)
	case len(suffix) == 0:
		replacement = append(replacement, prefix...)
	default:
		replacement = append(replacement, prefix...)
		if !bytes.HasSuffix(prefix, []byte(" ")) {
			replacement = append(replacement, ' ')
		}
		replacement = append(replacement, suffix...)
	}

	return fixx.Replace(file.Pos(replaceStart), ifStmt.Body.Lbrace, string(replacement)), true
}
