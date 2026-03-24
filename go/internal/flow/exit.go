package flow

import (
	"go/ast"
	"go/token"
)

// ExactLocalExit reports whether stmt is one of the local exit statements that
// Helena rules care about.
func ExactLocalExit(stmt ast.Stmt) bool {
	switch s := stmt.(type) {
	case *ast.ReturnStmt:
		return true
	case *ast.BranchStmt:
		switch s.Tok {
		case token.BREAK, token.CONTINUE, token.GOTO:
			return true
		}
	}
	return false
}
