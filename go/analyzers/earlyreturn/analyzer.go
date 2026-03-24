// Package earlyreturn implements the early-return rule.
package earlyreturn

import (
	"go/ast"
	"go/parser"
	"go/token"
	"os"
	"strings"

	"github.com/distrohelena/helena-linter/go/internal/astx"
	"github.com/distrohelena/helena-linter/go/internal/diag"
	"github.com/distrohelena/helena-linter/go/internal/exprx"
	"github.com/distrohelena/helena-linter/go/internal/fixx"
	"github.com/distrohelena/helena-linter/go/internal/flow"
	"golang.org/x/tools/go/analysis"
)

// Analyzer reports conservative guard-clause rewrites for wrapped happy paths
// and simple if/else branches.
var Analyzer = &analysis.Analyzer{
	Name: "earlyreturn",
	Doc:  "reports conservative control-flow shapes that should become guard clauses",
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
				if !ok || stmt == nil || stmt.Init != nil || stmt.Body == nil {
					return true
				}

				if edit, ok := wrappedHappyPathEdit(pass.Fset, source, file.Comments, item, stmt); ok {
					report(pass, stmt, edit)
					return true
				}

				if edit, ok := ifElseGuardEdit(pass.Fset, source, file.Comments, stmt); ok {
					report(pass, stmt, edit)
				}

				return true
			})

			return true
		})
	}

	return nil, nil
}

func report(pass *analysis.Pass, stmt *ast.IfStmt, edit analysis.TextEdit) {
	pass.Report(analysis.Diagnostic{
		Pos:     stmt.Pos(),
		Message: diag.Message(diag.RuleEarlyReturn, "rewrite this if as a guard clause with an early return"),
		SuggestedFixes: []analysis.SuggestedFix{
			{
				Message:   "Rewrite as guard clause",
				TextEdits: []analysis.TextEdit{edit},
			},
		},
	})
}

func wrappedHappyPathEdit(
	fset *token.FileSet,
	source []byte,
	comments []*ast.CommentGroup,
	item astx.StatementInBlock,
	stmt *ast.IfStmt,
) (analysis.TextEdit, bool) {
	if stmt.Else != nil || len(stmt.Body.List) == 0 || hasScopeChangingStatement(stmt.Body.List) {
		return analysis.TextEdit{}, false
	}

	next, ok := astx.NextStatement(item.Block, item.Statement)
	if !ok || hasInterveningComment(comments, stmt.End(), next.Pos()) {
		return analysis.TextEdit{}, false
	}

	fallback, ok := next.(*ast.ReturnStmt)
	if !ok {
		return analysis.TextEdit{}, false
	}

	last, ok := stmt.Body.List[len(stmt.Body.List)-1].(*ast.ReturnStmt)
	if !ok || last == nil {
		return analysis.TextEdit{}, false
	}

	file := fset.File(stmt.Pos())
	if file == nil {
		return analysis.TextEdit{}, false
	}

	newline := detectNewline(source)
	outerIndent := lineIndent(source, file.Offset(stmt.Pos()))
	indentUnit := detectIndentUnit(source, file, stmt.Body, outerIndent)

	guardCond, ok := invertedConditionText(source, file, stmt.Cond)
	if !ok {
		return analysis.TextEdit{}, false
	}

	fallbackText, ok := nodeText(source, file, fallback)
	if !ok {
		return analysis.TextEdit{}, false
	}

	happyText, ok := blockContent(source, file, stmt.Body)
	if !ok {
		return analysis.TextEdit{}, false
	}

	replacement := renderGuardClause(
		newline,
		outerIndent,
		indentUnit,
		guardCond,
		fallbackText,
		happyText,
	)

	return fixx.Replace(stmt.Pos(), fallback.End(), replacement), true
}

func ifElseGuardEdit(
	fset *token.FileSet,
	source []byte,
	comments []*ast.CommentGroup,
	stmt *ast.IfStmt,
) (analysis.TextEdit, bool) {
	elseBlock, ok := stmt.Else.(*ast.BlockStmt)
	if !ok || elseBlock == nil || len(stmt.Body.List) == 0 || len(elseBlock.List) == 0 {
		return analysis.TextEdit{}, false
	}

	if hasInterveningComment(comments, stmt.Body.Rbrace, elseBlock.Lbrace) {
		return analysis.TextEdit{}, false
	}

	thenExits := flow.DefinitelyExitsControlFlow(stmt.Body)
	elseExits := flow.DefinitelyExitsControlFlow(elseBlock)
	if !thenExits && !elseExits {
		return analysis.TextEdit{}, false
	}

	file := fset.File(stmt.Pos())
	if file == nil {
		return analysis.TextEdit{}, false
	}

	newline := detectNewline(source)
	outerIndent := lineIndent(source, file.Offset(stmt.Pos()))
	indentUnit := detectIndentUnit(source, file, stmt.Body, outerIndent)

	thenText, ok := blockContent(source, file, stmt.Body)
	if !ok {
		return analysis.TextEdit{}, false
	}

	elseText, ok := blockContent(source, file, elseBlock)
	if !ok {
		return analysis.TextEdit{}, false
	}

	var (
		guardCond   string
		guardBody   string
		successBody string
	)

	switch {
	case !thenExits && elseExits:
		if hasScopeChangingStatement(stmt.Body.List) {
			return analysis.TextEdit{}, false
		}
		guardCond, ok = invertedConditionText(source, file, stmt.Cond)
		if !ok {
			return analysis.TextEdit{}, false
		}
		guardBody = elseText
		successBody = thenText
	case thenExits && !elseExits:
		if hasScopeChangingStatement(elseBlock.List) {
			return analysis.TextEdit{}, false
		}
		guardCond, ok = exprText(source, file, stmt.Cond)
		if !ok {
			return analysis.TextEdit{}, false
		}
		guardBody = thenText
		successBody = elseText
	default:
		guardCond, ok = invertedConditionText(source, file, stmt.Cond)
		if !ok {
			return analysis.TextEdit{}, false
		}
		guardBody = elseText
		successBody = thenText
	}

	replacement := renderGuardClause(
		newline,
		outerIndent,
		indentUnit,
		guardCond,
		guardBody,
		successBody,
	)

	return fixx.Replace(stmt.Pos(), stmt.End(), replacement), true
}

func renderGuardClause(newline, outerIndent, indentUnit, cond, guardBody, successBody string) string {
	var builder strings.Builder
	builder.WriteString("if ")
	builder.WriteString(cond)
	builder.WriteString(" {")
	builder.WriteString(newline)
	builder.WriteString(indentLines(guardBody, outerIndent+indentUnit, newline))
	builder.WriteString(newline)
	builder.WriteString(outerIndent)
	builder.WriteString("}")
	builder.WriteString(newline)
	builder.WriteString(indentLines(successBody, outerIndent, newline))
	return builder.String()
}

func hasScopeChangingStatement(list []ast.Stmt) bool {
	for _, stmt := range list {
		switch s := stmt.(type) {
		case *ast.DeclStmt:
			return true
		case *ast.AssignStmt:
			if s.Tok == token.DEFINE {
				return true
			}
		}
	}
	return false
}

func hasInterveningComment(comments []*ast.CommentGroup, leftEnd, rightStart token.Pos) bool {
	for _, group := range comments {
		if group == nil {
			continue
		}
		if group.Pos() > leftEnd && group.End() < rightStart {
			return true
		}
	}
	return false
}

func invertedConditionText(source []byte, file *token.File, expr ast.Expr) (string, bool) {
	raw, ok := exprText(source, file, expr)
	if !ok {
		return "", false
	}

	inverted := ""
	switch e := stripParens(expr).(type) {
	case *ast.UnaryExpr:
		if e.Op != token.NOT {
			break
		}
		inverted, ok = exprText(source, file, e.X)
		if !ok {
			return "", false
		}
	case *ast.BinaryExpr:
		left, ok := exprText(source, file, e.X)
		if !ok {
			return "", false
		}
		right, ok := exprText(source, file, e.Y)
		if !ok {
			return "", false
		}
		switch e.Op {
		case token.EQL:
			inverted = left + " != " + right
		case token.NEQ:
			inverted = left + " == " + right
		}
	case *ast.Ident, *ast.SelectorExpr, *ast.IndexExpr, *ast.IndexListExpr, *ast.CallExpr:
		inverted = "!" + raw
	}

	if inverted == "" {
		inverted = "!(" + raw + ")"
	}

	parsed, err := parser.ParseExpr(inverted)
	if err != nil {
		return "", false
	}
	if !exprx.Complementary(expr, parsed, func(left, right *ast.Ident) bool {
		return left.Name == right.Name
	}) {
		return "", false
	}
	return inverted, true
}

func stripParens(expr ast.Expr) ast.Expr {
	for {
		paren, ok := expr.(*ast.ParenExpr)
		if !ok {
			return expr
		}
		expr = paren.X
	}
}

func exprText(source []byte, file *token.File, expr ast.Expr) (string, bool) {
	return nodeText(source, file, expr)
}

func nodeText(source []byte, file *token.File, node ast.Node) (string, bool) {
	if file == nil || node == nil {
		return "", false
	}
	start := file.Offset(node.Pos())
	end := file.Offset(node.End())
	if start < 0 || end < start || end > len(source) {
		return "", false
	}
	return string(source[start:end]), true
}

func blockContent(source []byte, file *token.File, block *ast.BlockStmt) (string, bool) {
	if file == nil || block == nil {
		return "", false
	}
	start := file.Offset(block.Lbrace) + 1
	end := file.Offset(block.Rbrace)
	if start < 0 || end < start || end > len(source) {
		return "", false
	}

	text := string(source[start:end])
	newline := detectNewline(source)
	lines := strings.Split(text, newline)

	for len(lines) > 0 && strings.TrimSpace(lines[0]) == "" {
		lines = lines[1:]
	}
	for len(lines) > 0 && strings.TrimSpace(lines[len(lines)-1]) == "" {
		lines = lines[:len(lines)-1]
	}
	if len(lines) == 0 {
		return "", false
	}

	prefix := commonIndent(lines)
	for i, line := range lines {
		if strings.TrimSpace(line) == "" {
			lines[i] = ""
			continue
		}
		lines[i] = strings.TrimPrefix(line, prefix)
	}

	return strings.Join(lines, newline), true
}

func commonIndent(lines []string) string {
	var prefix string
	first := true
	for _, line := range lines {
		if strings.TrimSpace(line) == "" {
			continue
		}
		indent := leadingIndent(line)
		if first {
			prefix = indent
			first = false
			continue
		}
		for !strings.HasPrefix(indent, prefix) {
			if prefix == "" {
				return ""
			}
			prefix = prefix[:len(prefix)-1]
		}
	}
	return prefix
}

func leadingIndent(line string) string {
	i := 0
	for i < len(line) {
		if line[i] != ' ' && line[i] != '\t' {
			break
		}
		i++
	}
	return line[:i]
}

func indentLines(text, indent, newline string) string {
	lines := strings.Split(text, newline)
	for i, line := range lines {
		if line == "" {
			lines[i] = indent
			continue
		}
		lines[i] = indent + line
	}
	return strings.Join(lines, newline)
}

func lineIndent(source []byte, offset int) string {
	if offset <= 0 {
		return ""
	}
	lineStart := offset
	for lineStart > 0 && source[lineStart-1] != '\n' {
		lineStart--
	}
	end := lineStart
	for end < len(source) && (source[end] == ' ' || source[end] == '\t') {
		end++
	}
	return string(source[lineStart:end])
}

func detectIndentUnit(source []byte, file *token.File, block *ast.BlockStmt, outerIndent string) string {
	if file == nil || block == nil || len(block.List) == 0 {
		return "\t"
	}
	firstOffset := file.Offset(block.List[0].Pos())
	if firstOffset < 0 || firstOffset > len(source) {
		return "\t"
	}
	innerIndent := lineIndent(source, firstOffset)
	if strings.HasPrefix(innerIndent, outerIndent) && len(innerIndent) > len(outerIndent) {
		return innerIndent[len(outerIndent):]
	}
	return "\t"
}

func detectNewline(source []byte) string {
	if strings.Contains(string(source), "\r\n") {
		return "\r\n"
	}
	return "\n"
}
