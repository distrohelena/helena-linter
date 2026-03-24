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

func TestIsHelenaExitSpacingStatement(t *testing.T) {
	tests := []struct {
		name string
		stmt ast.Stmt
		want bool
	}{
		{name: "return", stmt: &ast.ReturnStmt{}, want: true},
		{name: "break", stmt: &ast.BranchStmt{Tok: token.BREAK}, want: true},
		{name: "continue", stmt: &ast.BranchStmt{Tok: token.CONTINUE}, want: true},
		{name: "goto", stmt: &ast.BranchStmt{Tok: token.GOTO}, want: true},
		{name: "labeled return", stmt: &ast.LabeledStmt{Stmt: &ast.ReturnStmt{}}, want: true},
		{name: "labeled break", stmt: &ast.LabeledStmt{Stmt: &ast.BranchStmt{Tok: token.BREAK}}, want: true},
		{name: "non-exit", stmt: &ast.ExprStmt{}, want: false},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := IsHelenaExitSpacingStatement(tt.stmt); got != tt.want {
				t.Fatalf("IsHelenaExitSpacingStatement() = %v, want %v", got, tt.want)
			}
		})
	}
}
