package flow

import (
	"go/ast"
	"go/token"
)

// Helena flow helpers live here because the Go rules need a conservative,
// context-free "definitely exits" helper for future control-flow rules.
//
// DefinitelyExitsControlFlow reports whether stmt definitely stops execution of
// the surrounding local control flow without needing broader statement context.
func DefinitelyExitsControlFlow(stmt ast.Stmt) bool {
	switch s := stmt.(type) {
	case *ast.BlockStmt:
		return blockDefinitelyExits(s)
	case *ast.IfStmt:
		return ifDefinitelyExits(s)
	case *ast.LabeledStmt:
		return DefinitelyExitsControlFlow(s.Stmt)
	case *ast.ReturnStmt:
		return true
	}
	return false
}

// IsHelenaExitStatement reports whether stmt is one of the Helena exit-style
// statements that spacing rules care about.
func IsHelenaExitStatement(stmt ast.Stmt) bool {
	switch s := stmt.(type) {
	case *ast.LabeledStmt:
		return IsHelenaExitStatement(s.Stmt)
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

func blockDefinitelyExits(block *ast.BlockStmt) bool {
	if block == nil || len(block.List) == 0 {
		return false
	}
	return DefinitelyExitsControlFlow(block.List[len(block.List)-1])
}

func ifDefinitelyExits(stmt *ast.IfStmt) bool {
	if stmt == nil {
		return false
	}
	if stmt.Body == nil {
		return false
	}
	if !statementListDefinitelyExits(stmt.Body.List) {
		return false
	}
	if stmt.Else == nil {
		return false
	}
	return elseDefinitelyExits(stmt.Else)
}

func statementListDefinitelyExits(list []ast.Stmt) bool {
	if len(list) == 0 {
		return false
	}
	return DefinitelyExitsControlFlow(list[len(list)-1])
}

func elseDefinitelyExits(stmt ast.Stmt) bool {
	switch s := stmt.(type) {
	case *ast.BlockStmt:
		return blockDefinitelyExits(s)
	case *ast.IfStmt:
		return ifDefinitelyExits(s)
	case *ast.LabeledStmt:
		return elseDefinitelyExits(s.Stmt)
	}
	return false
}
