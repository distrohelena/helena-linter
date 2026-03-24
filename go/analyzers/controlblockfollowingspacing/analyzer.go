// Package controlblockfollowingspacing implements the control-block-following-spacing rule.
package controlblockfollowingspacing

import (
	"go/ast"

	"github.com/distrohelena/helena-linter/go/internal/astx"
	"github.com/distrohelena/helena-linter/go/internal/diag"
	"github.com/distrohelena/helena-linter/go/internal/fixx"
	"github.com/distrohelena/helena-linter/go/internal/textx"
	"golang.org/x/tools/go/analysis"
)

// Analyzer reports missing blank lines after completed control blocks before the next sibling statement.
var Analyzer = &analysis.Analyzer{
	Name: "controlblockfollowingspacing",
	Doc:  "reports missing blank lines after completed control blocks before the next sibling statement",
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
				if !isSupportedControlBlockStatement(item.Statement) {
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
					Message: diag.MissingBlankLineAfter(diag.RuleControlBlockFollowingSpacing),
					SuggestedFixes: []analysis.SuggestedFix{
						{
							Message:   "Add blank line after control block",
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

func isSupportedControlBlockStatement(stmt ast.Stmt) bool {
	switch stmt.(type) {
	case *ast.ForStmt, *ast.SwitchStmt, *ast.TypeSwitchStmt, *ast.SelectStmt:
		return true
	default:
		return false
	}
}
