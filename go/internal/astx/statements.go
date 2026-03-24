package astx

import "go/ast"

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
