package textx

import (
	"go/ast"
	"go/parser"
	"go/token"
	"testing"

	"github.com/distrohelena/helena-linter/go/internal/astx"
)

func parseFile(t *testing.T, src string) (*token.FileSet, *ast.File) {
	t.Helper()

	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "input.go", src, parser.ParseComments)
	if err != nil {
		t.Fatalf("ParseFile() error = %v", err)
	}
	return fset, file
}

func parseBlock(t *testing.T, src string) (*token.FileSet, *ast.BlockStmt, *ast.File) {
	t.Helper()

	fset, file := parseFile(t, src)
	fn, ok := file.Decls[0].(*ast.FuncDecl)
	if !ok {
		t.Fatalf("first declaration is %T, want *ast.FuncDecl", file.Decls[0])
	}
	return fset, fn.Body, file
}

func TestSiblingStatements(t *testing.T) {
	_, block, _ := parseBlock(t, `package p

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
	t.Run("no blank line, no comments", func(t *testing.T) {
		fset, block, file := parseBlock(t, `package p

func f() {
	first()
	second()
}`)

		if HasBlankLineBetween(fset, file.Comments, block.List[0], block.List[1]) {
			t.Fatal("HasBlankLineBetween() = true, want false")
		}
	})

	t.Run("blank line with no comments", func(t *testing.T) {
		fset, block, file := parseBlock(t, `package p

func f() {
	first()

	second()
}`)

		if !HasBlankLineBetween(fset, file.Comments, block.List[0], block.List[1]) {
			t.Fatal("HasBlankLineBetween() = false, want true")
		}
	})

	t.Run("comment between statements with no blank line", func(t *testing.T) {
		fset, block, file := parseBlock(t, `package p

func f() {
	first()
	// comment
	second()
}`)

		if HasBlankLineBetween(fset, file.Comments, block.List[0], block.List[1]) {
			t.Fatal("HasBlankLineBetween() = true, want false")
		}
	})

	t.Run("multi-line block comment between statements", func(t *testing.T) {
		fset, block, file := parseBlock(t, `package p

func f() {
	first()
	/*
		comment line 1
		comment line 2
	*/
	second()
}`)

		if HasBlankLineBetween(fset, file.Comments, block.List[0], block.List[1]) {
			t.Fatal("HasBlankLineBetween() = true, want false")
		}
	})

	t.Run("blank line before or after intervening comment group", func(t *testing.T) {
		tests := []struct {
			name string
			src  string
		}{
			{
				name: "blank line before comment",
				src: `package p

func f() {
	first()

	// comment
	second()
}`,
			},
			{
				name: "blank line after comment",
				src: `package p

func f() {
	first()
	// comment

	second()
}`,
			},
			{
				name: "blank line before multiline block comment",
				src: `package p

func f() {
	first()

	/*
		comment line 1
		comment line 2
	*/
	second()
}`,
			},
			{
				name: "blank line after multiline block comment",
				src: `package p

func f() {
	first()
	/*
		comment line 1
		comment line 2
	*/

	second()
}`,
			},
		}

		for _, tt := range tests {
			t.Run(tt.name, func(t *testing.T) {
				fset, block, file := parseBlock(t, tt.src)
				if !HasBlankLineBetween(fset, file.Comments, block.List[0], block.List[1]) {
					t.Fatal("HasBlankLineBetween() = false, want true")
				}
			})
		}
	})
}
