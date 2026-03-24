// Package declarationleadingspacing implements the declaration-leading-spacing rule.
package declarationleadingspacing

import (
	"go/ast"

	"github.com/distrohelena/helena-linter/go/internal/astx"
	"github.com/distrohelena/helena-linter/go/internal/diag"
	"github.com/distrohelena/helena-linter/go/internal/fixx"
	"github.com/distrohelena/helena-linter/go/internal/textx"
	"golang.org/x/tools/go/analysis"
)

// Analyzer reports missing blank lines before declarations that follow non-declarations.
var Analyzer = &analysis.Analyzer{
	Name: "declarationleadingspacing",
	Doc:  "reports missing blank lines before declarations that follow non-declarations",
	Run:  run,
}

func run(pass *analysis.Pass) (any, error) {
	for _, file := range pass.Files {
		file := file
		ast.Inspect(file, func(n ast.Node) bool {
			block, ok := n.(*ast.BlockStmt)
			if !ok {
				return true
			}

			astx.WalkBlockStatements(block, func(item astx.StatementInBlock) bool {
				if !isDeclaration(item.Statement) {
					return true
				}

				previous, ok := astx.PreviousStatement(item.Block, item.Statement)
				if !ok || isDeclaration(previous) || textx.HasBlankLineBetween(pass.Fset, file.Comments, previous, item.Statement) {
					return true
				}

				insertionPos := textx.BlankLineInsertionPos(pass.Fset, file.Comments, previous, item.Statement)
				if insertionPos == 0 {
					return true
				}

				pass.Report(analysis.Diagnostic{
					Pos:     item.Statement.Pos(),
					Message: diag.MissingBlankLineBefore(diag.RuleDeclarationLeadingSpacing),
					SuggestedFixes: []analysis.SuggestedFix{
						{
							Message:   "Add blank line before declaration",
							TextEdits: []analysis.TextEdit{fixx.InsertAt(insertionPos, "\n")},
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

func isDeclaration(stmt ast.Stmt) bool {
	_, ok := stmt.(*ast.DeclStmt)
	return ok
}
