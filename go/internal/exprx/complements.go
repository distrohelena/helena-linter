package exprx

import (
	"go/ast"
	"go/token"
)

// IdentEqualFunc reports whether two identifiers should be treated as the same
// symbol for the purposes of complement detection.
type IdentEqualFunc func(left, right *ast.Ident) bool

// Complementary reports whether left and right are exact logical complements
// for the small set of shapes the Helena Go analyzers rely on.
//
// Callers should provide an identifier comparison function that matches their
// scope model. When nil, identifier comparisons are treated as unequal.
func Complementary(left, right ast.Expr, identEqual IdentEqualFunc) bool {
	left = stripParens(left)
	right = stripParens(right)

	if left == nil || right == nil {
		return false
	}

	if isNegationOf(left, right, identEqual) || isNegationOf(right, left, identEqual) {
		return true
	}

	if isNegatedEqualityComplement(left, right, identEqual) || isNegatedEqualityComplement(right, left, identEqual) {
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

	if sameExpr(lbin.X, rbin.X, identEqual) && sameExpr(lbin.Y, rbin.Y, identEqual) {
		return true
	}
	if sameExpr(lbin.X, rbin.Y, identEqual) && sameExpr(lbin.Y, rbin.X, identEqual) {
		return true
	}

	return false
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

func isNegationOf(a, b ast.Expr, identEqual IdentEqualFunc) bool {
	unary, ok := a.(*ast.UnaryExpr)
	if !ok || unary.Op != token.NOT {
		return false
	}
	return sameExpr(unary.X, b, identEqual)
}

func isNegatedEqualityComplement(left, right ast.Expr, identEqual IdentEqualFunc) bool {
	unary, ok := left.(*ast.UnaryExpr)
	if !ok || unary.Op != token.NOT {
		return false
	}
	lbin, ok := stripParens(unary.X).(*ast.BinaryExpr)
	if !ok || !isEqualityOrInequality(lbin.Op) {
		return false
	}
	rbin, ok := stripParens(right).(*ast.BinaryExpr)
	if !ok || !isEqualityOrInequality(rbin.Op) || lbin.Op == rbin.Op {
		return false
	}

	return sameExpr(lbin.X, rbin.X, identEqual) && sameExpr(lbin.Y, rbin.Y, identEqual) ||
		sameExpr(lbin.X, rbin.Y, identEqual) && sameExpr(lbin.Y, rbin.X, identEqual)
}

func isEqualityOrInequality(op token.Token) bool {
	return op == token.EQL || op == token.NEQ
}

func sameExpr(left, right ast.Expr, identEqual IdentEqualFunc) bool {
	left = stripParens(left)
	right = stripParens(right)

	switch l := left.(type) {
	case nil:
		return right == nil
	case *ast.Ident:
		if identEqual == nil {
			return false
		}
		r, ok := right.(*ast.Ident)
		return ok && identEqual(l, r)
	case *ast.BasicLit:
		r, ok := right.(*ast.BasicLit)
		return ok && l.Kind == r.Kind && l.Value == r.Value
	case *ast.SelectorExpr:
		r, ok := right.(*ast.SelectorExpr)
		return ok && sameExpr(l.X, r.X, identEqual) && sameSelectorIdent(l.Sel, r.Sel)
	case *ast.UnaryExpr:
		r, ok := right.(*ast.UnaryExpr)
		return ok && l.Op == r.Op && sameExpr(l.X, r.X, identEqual)
	case *ast.BinaryExpr:
		r, ok := right.(*ast.BinaryExpr)
		return ok && l.Op == r.Op && sameExpr(l.X, r.X, identEqual) && sameExpr(l.Y, r.Y, identEqual)
	case *ast.CallExpr:
		r, ok := right.(*ast.CallExpr)
		if !ok || !sameExpr(l.Fun, r.Fun, identEqual) || len(l.Args) != len(r.Args) || (l.Ellipsis != token.NoPos) != (r.Ellipsis != token.NoPos) {
			return false
		}
		for i := range l.Args {
			if !sameExpr(l.Args[i], r.Args[i], identEqual) {
				return false
			}
		}
		return true
	case *ast.IndexExpr:
		r, ok := right.(*ast.IndexExpr)
		return ok && sameExpr(l.X, r.X, identEqual) && sameExpr(l.Index, r.Index, identEqual)
	case *ast.IndexListExpr:
		r, ok := right.(*ast.IndexListExpr)
		if !ok || !sameExpr(l.X, r.X, identEqual) || len(l.Indices) != len(r.Indices) {
			return false
		}
		for i := range l.Indices {
			if !sameExpr(l.Indices[i], r.Indices[i], identEqual) {
				return false
			}
		}
		return true
	case *ast.ParenExpr:
		return sameExpr(l.X, right, identEqual)
	default:
		return false
	}
}

func sameSelectorIdent(left, right ast.Expr) bool {
	l, ok := left.(*ast.Ident)
	if !ok {
		return false
	}
	r, ok := right.(*ast.Ident)
	return ok && l.Name == r.Name
}
