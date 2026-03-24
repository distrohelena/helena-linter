// Package iffollowingspacing implements the if-following-spacing rule.
package iffollowingspacing

import (
	"go/ast"

	"github.com/distrohelena/helena-linter/go/internal/astx"
	"github.com/distrohelena/helena-linter/go/internal/diag"
	"github.com/distrohelena/helena-linter/go/internal/fixx"
	"github.com/distrohelena/helena-linter/go/internal/textx"
	"golang.org/x/tools/go/analysis"
)

// Analyzer reports missing blank lines after completed if statements before the next sibling statement.
var Analyzer = &analysis.Analyzer{
	Name: "iffollowingspacing",
	Doc:  "reports missing blank lines after completed if statements before the next sibling statement",
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

				next, ok := astx.NextStatement(item.Block, item.Statement)
				if !ok || textx.HasBlankLineBetween(pass.Fset, file.Comments, item.Statement, next) {
					return true
				}

				insertionPos := textx.BlankLineInsertionPos(pass.Fset, file.Comments, item.Statement, next)
				if insertionPos == 0 {
					return true
				}

				pass.Report(analysis.Diagnostic{
					Pos:     item.Statement.Pos(),
					Message: diag.MissingBlankLineAfter(diag.RuleIfFollowingSpacing),
					SuggestedFixes: []analysis.SuggestedFix{
						{
							Message:   "Add blank line after if statement",
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
