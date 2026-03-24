package exprx

import (
	"go/ast"
	"go/parser"
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

func TestComplementary(t *testing.T) {
	tests := []struct {
		name  string
		left  string
		right string
		want  bool
	}{
		{name: "equals nil vs not equals nil", left: "value == nil", right: "value != nil", want: true},
		{name: "nil comparison is symmetric", left: "nil != value", right: "value == nil", want: true},
		{name: "negation vs positive", left: "!flag", right: "flag", want: true},
		{name: "parenthesized negation", left: "!(flag)", right: "flag", want: true},
		{name: "different operands", left: "value == nil", right: "other != nil", want: false},
		{name: "different negation target", left: "!flag", right: "other", want: false},
		{name: "same polarity", left: "flag", right: "flag", want: false},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := Complementary(mustExpr(t, tt.left), mustExpr(t, tt.right)); got != tt.want {
				t.Fatalf("Complementary(%q, %q) = %v, want %v", tt.left, tt.right, got, tt.want)
			}
		})
	}
}
