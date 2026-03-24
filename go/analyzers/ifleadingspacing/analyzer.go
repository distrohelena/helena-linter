// Package ifleadingspacing implements the if-leading-spacing rule.
package ifleadingspacing

import (
	"go/ast"

	"github.com/distrohelena/helena-linter/go/internal/astx"
	"github.com/distrohelena/helena-linter/go/internal/diag"
	"github.com/distrohelena/helena-linter/go/internal/fixx"
	"github.com/distrohelena/helena-linter/go/internal/textx"
	"golang.org/x/tools/go/analysis"
)

// Analyzer reports missing blank lines before if statements that follow another sibling statement.
var Analyzer = &analysis.Analyzer{
	Name: "ifleadingspacing",
	Doc:  "reports missing blank lines before if statements that follow another sibling statement",
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
				if _, ok := item.Statement.(*ast.IfStmt); !ok {
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
					Message: diag.MissingBlankLineBefore(diag.RuleIfLeadingSpacing),
					SuggestedFixes: []analysis.SuggestedFix{
						{
							Message:   "Add blank line before if statement",
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
