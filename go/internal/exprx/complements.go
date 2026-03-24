package exprx

import (
	"go/ast"
	"go/token"
)

// Complementary reports whether left and right are exact logical complements
// for the small set of shapes the Helena Go analyzers rely on.
func Complementary(left, right ast.Expr) bool {
	left = stripParens(left)
	right = stripParens(right)

	if left == nil || right == nil {
		return false
	}

	if isNegationOf(left, right) || isNegationOf(right, left) {
		return true
	}

	lbin, lok := left.(*ast.BinaryExpr)
	rbin, rok := right.(*ast.BinaryExpr)
	if !lok || !rok {
		return false
	}

	if !isEqualityOrInequality(lbin.Op) || !isEqualityOrInequality(rbin.Op) {
		return false
	}
	if lbin.Op == rbin.Op {
		return false
	}

	return sameExpr(lbin.X, rbin.X) && sameExpr(lbin.Y, rbin.Y) ||
		sameExpr(lbin.X, rbin.Y) && sameExpr(lbin.Y, rbin.X)
}

func stripParens(expr ast.Expr) ast.Expr {
	for {
		paren, ok := expr.(*ast.ParenExpr)
		if !ok {
			return expr
		}
		expr = paren.X
	}
}

func isNegationOf(a, b ast.Expr) bool {
	unary, ok := a.(*ast.UnaryExpr)
	if !ok || unary.Op != token.NOT {
		return false
	}
	return sameExpr(unary.X, b)
}

func isEqualityOrInequality(op token.Token) bool {
	return op == token.EQL || op == token.NEQ
}

func sameExpr(left, right ast.Expr) bool {
	left = stripParens(left)
	right = stripParens(right)

	switch l := left.(type) {
	case nil:
		return right == nil
	case *ast.Ident:
		r, ok := right.(*ast.Ident)
		return ok && l.Name == r.Name
	case *ast.BasicLit:
		r, ok := right.(*ast.BasicLit)
		return ok && l.Kind == r.Kind && l.Value == r.Value
	case *ast.SelectorExpr:
		r, ok := right.(*ast.SelectorExpr)
		return ok && sameExpr(l.X, r.X) && sameExpr(l.Sel, r.Sel)
	case *ast.UnaryExpr:
		r, ok := right.(*ast.UnaryExpr)
		return ok && l.Op == r.Op && sameExpr(l.X, r.X)
	case *ast.BinaryExpr:
		r, ok := right.(*ast.BinaryExpr)
		return ok && l.Op == r.Op && sameExpr(l.X, r.X) && sameExpr(l.Y, r.Y)
	case *ast.CallExpr:
		r, ok := right.(*ast.CallExpr)
		if !ok || !sameExpr(l.Fun, r.Fun) || len(l.Args) != len(r.Args) {
			return false
		}
		for i := range l.Args {
			if !sameExpr(l.Args[i], r.Args[i]) {
				return false
			}
		}
		return true
	case *ast.IndexExpr:
		r, ok := right.(*ast.IndexExpr)
		return ok && sameExpr(l.X, r.X) && sameExpr(l.Index, r.Index)
	case *ast.ParenExpr:
		return sameExpr(l.X, right)
	default:
		return false
	}
}
