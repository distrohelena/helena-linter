package textx

import (
	"go/ast"
	"go/token"
)

// HasBlankLineBetween reports whether there is at least one blank line between
// two adjacent AST nodes based on their token positions.
func HasBlankLineBetween(fset *token.FileSet, left, right ast.Node) bool {
	if fset == nil || left == nil || right == nil {
		return false
	}

	leftEnd := fset.Position(left.End()).Line
	rightStart := fset.Position(right.Pos()).Line
	if leftEnd == 0 || rightStart == 0 {
		return false
	}
	return rightStart-leftEnd > 1
}
