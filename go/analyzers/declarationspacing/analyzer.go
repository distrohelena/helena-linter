// Package declarationspacing implements the declaration-spacing rule.
package declarationspacing

import (
	"go/ast"

	"github.com/distrohelena/helena-linter/go/internal/astx"
	"github.com/distrohelena/helena-linter/go/internal/diag"
	"github.com/distrohelena/helena-linter/go/internal/fixx"
	"github.com/distrohelena/helena-linter/go/internal/textx"
	"golang.org/x/tools/go/analysis"
)

// Analyzer reports missing blank lines after declarations before non-declarations.
var Analyzer = &analysis.Analyzer{
	Name: "declarationspacing",
	Doc:  "reports missing blank lines after declarations before non-declarations",
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

				next, ok := astx.NextStatement(item.Block, item.Statement)
				if !ok || isDeclaration(next) || textx.HasBlankLineBetween(pass.Fset, file.Comments, item.Statement, next) {
					return true
				}

				insertionPos := textx.BlankLineInsertionPos(pass.Fset, file.Comments, item.Statement, next)
				if insertionPos == 0 {
					return true
				}

				pass.Report(analysis.Diagnostic{
					Pos:     item.Statement.Pos(),
					Message: diag.MissingBlankLineAfter(diag.RuleDeclarationSpacing),
					SuggestedFixes: []analysis.SuggestedFix{
						{
							Message:   "Add blank line after declaration",
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
