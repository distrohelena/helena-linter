package exprx

import (
	"go/ast"
	"go/parser"
	"go/token"
	"testing"
)

func mustExpr(t *testing.T, src string) ast.Expr {
	t.Helper()

	expr, err := parser.ParseExpr(src)
	if err != nil {
		t.Fatalf("ParseExpr(%q) error = %v", src, err)
	}
	return expr
}

func mustCallExpr(t *testing.T, src string) ast.Expr {
	t.Helper()

	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "input.go", src, 0)
	if err != nil {
		t.Fatalf("ParseFile() error = %v", err)
	}

	var expr ast.Expr
	ast.Inspect(file, func(n ast.Node) bool {
		if expr != nil {
			return false
		}
		if e, ok := n.(*ast.CallExpr); ok {
			expr = e
			return false
		}
		return true
	})
	if expr == nil {
		t.Fatal("did not find call expression")
	}
	return expr
}

func TestComplementary(t *testing.T) {
	tests := []struct {
		name  string
		left  string
		right string
		ident IdentEqualFunc
		want  bool
	}{
		{name: "equals nil vs not equals nil", left: "value == nil", right: "value != nil", ident: func(left, right *ast.Ident) bool { return left.Name == right.Name }, want: true},
		{name: "nil comparison is symmetric", left: "nil != value", right: "value == nil", ident: func(left, right *ast.Ident) bool { return left.Name == right.Name }, want: true},
		{name: "negation vs positive", left: "!flag", right: "flag", ident: func(left, right *ast.Ident) bool { return left.Name == right.Name }, want: true},
		{name: "parenthesized negation", left: "!(flag)", right: "flag", ident: func(left, right *ast.Ident) bool { return left.Name == right.Name }, want: true},
		{name: "negated equality with swapped operands", left: "!(a == b)", right: "b != a", ident: func(left, right *ast.Ident) bool { return left.Name == right.Name }, want: true},
		{name: "generic call with multiple type arguments", left: "!pred[T, U](x)", right: "pred[T, U](x)", ident: func(left, right *ast.Ident) bool { return left.Name == right.Name }, want: true},
		{name: "ellipsis changes call shape", left: "pred(xs...)", right: "pred(xs)", ident: func(left, right *ast.Ident) bool { return left.Name == right.Name }, want: false},
		{name: "different operands", left: "value == nil", right: "other != nil", ident: func(left, right *ast.Ident) bool { return left.Name == right.Name }, want: false},
		{name: "different negation target", left: "!flag", right: "other", ident: func(left, right *ast.Ident) bool { return left.Name == right.Name }, want: false},
		{name: "same polarity", left: "flag", right: "flag", ident: func(left, right *ast.Ident) bool { return left.Name == right.Name }, want: false},
		{name: "nil comparator is conservative", left: "!flag", right: "flag", ident: nil, want: false},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := Complementary(mustExpr(t, tt.left), mustExpr(t, tt.right), tt.ident); got != tt.want {
				t.Fatalf("Complementary(%q, %q) = %v, want %v", tt.left, tt.right, got, tt.want)
			}
		})
	}
}

func TestComplementaryUsesInjectedIdentifierIdentity(t *testing.T) {
	identEqual := func(left, right *ast.Ident) bool {
		if left.Name == right.Name {
			return true
		}
		return (left.Name == "foo" && right.Name == "bar") || (left.Name == "bar" && right.Name == "foo")
	}

	if got := Complementary(mustExpr(t, "foo == nil"), mustExpr(t, "bar != nil"), identEqual); !got {
		t.Fatal("Complementary() = false, want true when identifiers are treated as equivalent by callback")
	}
}

func TestComplementarySelectorNamesStaySpellingBased(t *testing.T) {
	identEqual := func(left, right *ast.Ident) bool {
		return true
	}

	if got := Complementary(mustExpr(t, "obj.foo == nil"), mustExpr(t, "obj.bar != nil"), identEqual); got {
		t.Fatal("Complementary() = true, want false when selector names differ")
	}
}

func TestComplementaryNilComparatorDoesNotPanicOnIdentifierComparison(t *testing.T) {
	if got := Complementary(mustExpr(t, "!flag"), mustExpr(t, "flag"), nil); got {
		t.Fatal("Complementary() = true, want false with nil comparator")
	}
}

func TestComplementaryVariadicCallsFromDifferentLocations(t *testing.T) {
	left := mustExpr(t, `!pred(xs...)`)
	right := mustCallExpr(t, `package p

func f() {
	pred(xs...)
}`)

	identEqual := func(left, right *ast.Ident) bool {
		return left.Name == right.Name
	}

	if got := Complementary(left, right, identEqual); !got {
		t.Fatal("Complementary() = false, want true for identical variadic calls from different locations")
	}
}
