package flow

import (
	"go/ast"
	"go/token"
)

// IsHelenaExitStatement reports whether stmt is a Helena exit-style statement.
// The current Helena rules treat return, break, continue, and goto as exits.
func IsHelenaExitStatement(stmt ast.Stmt) bool {
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
