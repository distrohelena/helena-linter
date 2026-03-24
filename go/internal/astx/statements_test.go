package astx

import (
	"go/ast"
	"go/parser"
	"go/token"
	"testing"
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

	prev, ok := PreviousStatement(block, middle)
	if !ok {
		t.Fatal("PreviousStatement() returned false for middle statement")
	}
	if prev != first {
		t.Fatalf("PreviousStatement() = %T, want first statement", prev)
	}

	next, ok := NextStatement(block, middle)
	if !ok {
		t.Fatal("NextStatement() returned false for middle statement")
	}
	if next != last {
		t.Fatalf("NextStatement() = %T, want last statement", next)
	}

	if _, ok := PreviousStatement(block, first); ok {
		t.Fatal("PreviousStatement() returned true for first statement")
	}
	if _, ok := NextStatement(block, last); ok {
		t.Fatal("NextStatement() returned true for last statement")
	}
}

func TestWalkBlockStatements(t *testing.T) {
	_, block := parseBlock(t, `package p

func f() {
	first()
	second()
	third()
}`)

	var seen []StatementInBlock
	WalkBlockStatements(block, func(item StatementInBlock) bool {
		seen = append(seen, item)
		return true
	})

	if len(seen) != 3 {
		t.Fatalf("WalkBlockStatements() visited %d statements, want 3", len(seen))
	}
	for i, item := range seen {
		if item.Block != block {
			t.Fatalf("item %d block = %p, want %p", i, item.Block, block)
		}
		if item.Index != i {
			t.Fatalf("item %d index = %d, want %d", i, item.Index, i)
		}
		if item.Statement != block.List[i] {
			t.Fatalf("item %d statement mismatch", i)
		}
	}
}
