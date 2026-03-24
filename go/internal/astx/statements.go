package astx

import (
	"go/ast"
	"go/token"
)

// StatementInBlock carries a statement together with its enclosing block and
// index within that block.
type StatementInBlock struct {
	Block     *ast.BlockStmt
	Index     int
	Statement ast.Stmt
}

func statementIndex(block *ast.BlockStmt, stmt ast.Stmt) int {
	if block == nil || stmt == nil {
		return -1
	}
	for i, candidate := range block.List {
		if candidate == stmt {
			return i
		}
	}
	return -1
}

// PreviousStatement returns the previous sibling statement in the same block.
func PreviousStatement(block *ast.BlockStmt, stmt ast.Stmt) (ast.Stmt, bool) {
	i := statementIndex(block, stmt)
	if i <= 0 {
		return nil, false
	}
	return block.List[i-1], true
}

// NextStatement returns the next sibling statement in the same block.
func NextStatement(block *ast.BlockStmt, stmt ast.Stmt) (ast.Stmt, bool) {
	i := statementIndex(block, stmt)
	if i < 0 || i >= len(block.List)-1 {
		return nil, false
	}
	return block.List[i+1], true
}

// WalkBlockStatements calls fn for each statement in block along with its
// enclosing block and index. Iteration stops when fn returns false.
func WalkBlockStatements(block *ast.BlockStmt, fn func(StatementInBlock) bool) {
	if block == nil || fn == nil {
		return
	}
	for i, stmt := range block.List {
		if !fn(StatementInBlock{Block: block, Index: i, Statement: stmt}) {
			return
		}
	}
}

// IsHelenaExitSpacingStatement reports whether stmt is one of the statement
// forms that Helena spacing rules treat as an exit-style boundary.
func IsHelenaExitSpacingStatement(stmt ast.Stmt) bool {
	switch s := stmt.(type) {
	case *ast.LabeledStmt:
		return IsHelenaExitSpacingStatement(s.Stmt)
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
