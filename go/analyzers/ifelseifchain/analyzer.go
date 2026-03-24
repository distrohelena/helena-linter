// Package ifelseifchain implements the if-else-if-chain rule.
package ifelseifchain

import (
	"go/ast"
	"go/token"
	"os"

	"github.com/distrohelena/helena-linter/go/internal/astx"
	"github.com/distrohelena/helena-linter/go/internal/diag"
	"github.com/distrohelena/helena-linter/go/internal/fixx"
	"github.com/distrohelena/helena-linter/go/internal/flow"
	"golang.org/x/tools/go/analysis"
)

// Analyzer reports sibling if statements that should be joined into else if chains.
var Analyzer = &analysis.Analyzer{
	Name: "ifelseifchain",
	Doc:  "reports sibling if statements that should be joined into else if chains",
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
			block, ok := n.(*ast.BlockStmt)
			if !ok {
				return true
			}

			astx.WalkBlockStatements(block, func(item astx.StatementInBlock) bool {
				stmt, ok := item.Statement.(*ast.IfStmt)
				if !ok || stmt == nil || stmt.Else != nil || stmt.Body == nil {
					return true
				}

				if !flow.DefinitelyExitsControlFlow(stmt.Body) {
					return true
				}

				next, ok := astx.NextStatement(item.Block, item.Statement)
				if !ok {
					return true
				}

				nextIf, ok := next.(*ast.IfStmt)
				if !ok || nextIf == nil {
					return true
				}

				edit, ok := ifElseIfChainEdit(pass.Fset, source, file.Comments, stmt, nextIf)
				if !ok {
					return true
				}

				pass.Report(analysis.Diagnostic{
					Pos:     nextIf.Pos(),
					Message: diag.Message(diag.RuleIfElseIfChain, "join this if to the previous branch with else if"),
					SuggestedFixes: []analysis.SuggestedFix{
						{
							Message:   "Join with previous if",
							TextEdits: []analysis.TextEdit{edit},
						},
					},
				})
				return true
			})

			return true
		})
	}

	return nil, nil
}

func ifElseIfChainEdit(fset *token.FileSet, source []byte, comments []*ast.CommentGroup, previous, next *ast.IfStmt) (analysis.TextEdit, bool) {
	if fset == nil || previous == nil || next == nil || previous.Body == nil {
		return analysis.TextEdit{}, false
	}

	file := fset.File(previous.Pos())
	if file == nil {
		return analysis.TextEdit{}, false
	}

	start := file.Offset(previous.Body.Rbrace)
	if start < 0 || start >= len(source) {
		return analysis.TextEdit{}, false
	}
	start++

	end := file.Offset(next.Pos())
	if end < 0 || end > len(source) || start > end {
		return analysis.TextEdit{}, false
	}

	if hasInterveningComment(comments, previous, next) {
		return fixx.InsertAt(file.Pos(start), " else"), true
	}

	return fixx.Replace(file.Pos(start), file.Pos(end), " else "), true
}

func hasInterveningComment(comments []*ast.CommentGroup, left, right ast.Node) bool {
	if left == nil || right == nil {
		return false
	}

	leftEnd := left.End()
	rightPos := right.Pos()
	for _, group := range comments {
		if group == nil {
			continue
		}
		if group.Pos() > leftEnd && group.End() < rightPos {
			return true
		}
	}
	return false
}
