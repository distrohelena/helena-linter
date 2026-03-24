package textx

import (
	"go/ast"
	"go/parser"
	"go/token"
	"testing"

	"github.com/distrohelena/helena-linter/go/internal/astx"
)

func parseBlock(t *testing.T, src string) (*token.FileSet, *ast.BlockStmt) {
	t.Helper()

	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "input.go", src, parser.ParseComments)
	if err != nil {
		t.Fatalf("ParseFile() error = %v", err)
	}

	fn, ok := file.Decls[0].(*ast.FuncDecl)
	if !ok {
		t.Fatalf("first declaration is %T, want *ast.FuncDecl", file.Decls[0])
	}
	return fset, fn.Body
}

func TestSiblingStatements(t *testing.T) {
	_, block := parseBlock(t, `package p

func f() {
	first()
	middle()
	last()
}`)

	first := block.List[0]
	middle := block.List[1]
	last := block.List[2]

	prev, ok := astx.PreviousStatement(block, middle)
	if !ok {
		t.Fatal("PreviousStatement() returned false for middle statement")
	}
	if prev != first {
		t.Fatalf("PreviousStatement() = %T, want first statement", prev)
	}

	next, ok := astx.NextStatement(block, middle)
	if !ok {
		t.Fatal("NextStatement() returned false for middle statement")
	}
	if next != last {
		t.Fatalf("NextStatement() = %T, want last statement", next)
	}

	if _, ok := astx.PreviousStatement(block, first); ok {
		t.Fatal("PreviousStatement() returned true for first statement")
	}
	if _, ok := astx.NextStatement(block, last); ok {
		t.Fatal("NextStatement() returned true for last statement")
	}
}

func TestHasBlankLineBetween(t *testing.T) {
	t.Run("detects adjacent statements without a blank line", func(t *testing.T) {
		fset, block := parseBlock(t, `package p

func f() {
	first()
	second()
}`)

		if HasBlankLineBetween(fset, block.List[0], block.List[1]) {
			t.Fatal("HasBlankLineBetween() = true, want false")
		}
	})

	t.Run("detects a blank line between statements", func(t *testing.T) {
		fset, block := parseBlock(t, `package p

func f() {
	first()

	second()
}`)

		if !HasBlankLineBetween(fset, block.List[0], block.List[1]) {
			t.Fatal("HasBlankLineBetween() = false, want true")
		}
	})
}
