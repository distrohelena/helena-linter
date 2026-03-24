// Package exitspacing implements the exit-spacing rule.
package exitspacing

import (
	"go/ast"

	"github.com/distrohelena/helena-linter/go/internal/astx"
	"github.com/distrohelena/helena-linter/go/internal/diag"
	"github.com/distrohelena/helena-linter/go/internal/fixx"
	"github.com/distrohelena/helena-linter/go/internal/flow"
	"github.com/distrohelena/helena-linter/go/internal/textx"
	"golang.org/x/tools/go/analysis"
)

// Analyzer reports missing blank lines before exit statements.
var Analyzer = &analysis.Analyzer{
	Name: "exitspacing",
	Doc:  "reports missing blank lines before exit statements",
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
				if !flow.IsLocalExitStatement(item.Statement) {
					return true
				}

				previous, ok := astx.PreviousStatement(item.Block, item.Statement)
				if !ok || textx.HasBlankLineBetween(pass.Fset, file.Comments, previous, item.Statement) {
					return true
				}

				insertionPos := textx.BlankLineInsertionPos(pass.Fset, file.Comments, previous, item.Statement)
				if insertionPos == 0 {
					return true
				}

				pass.Report(analysis.Diagnostic{
					Pos:     item.Statement.Pos(),
					Message: diag.MissingBlankLineBefore(diag.RuleExitSpacing),
					SuggestedFixes: []analysis.SuggestedFix{
						{
							Message:   "Add blank line before exit statement",
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
