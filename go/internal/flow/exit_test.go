package flow

import (
	"go/ast"
	"go/parser"
	"go/token"
	"testing"
)

func mustStmt(t *testing.T, src string) ast.Stmt {
	t.Helper()

	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "input.go", src, 0)
	if err != nil {
		t.Fatalf("ParseFile() error = %v", err)
	}

	fn, ok := file.Decls[0].(*ast.FuncDecl)
	if !ok {
		t.Fatalf("first declaration is %T, want *ast.FuncDecl", file.Decls[0])
	}
	return fn.Body.List[0]
}

func TestIsHelenaExitStatement(t *testing.T) {
	tests := []struct {
		name string
		stmt ast.Stmt
		want bool
	}{
		{name: "return exits", stmt: mustStmt(t, `package p

func f() {
	return
}`), want: true},
		{name: "break exits", stmt: mustStmt(t, `package p

func f() {
	break
}`), want: true},
		{name: "continue exits", stmt: mustStmt(t, `package p

func f() {
	continue
}`), want: true},
		{name: "goto exits", stmt: mustStmt(t, `package p

func f() {
	goto done
done:
}`), want: true},
		{name: "non-exit statement", stmt: mustStmt(t, `package p

func f() {
	defer cleanup()
}`), want: false},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := IsHelenaExitStatement(tt.stmt); got != tt.want {
				t.Fatalf("IsHelenaExitStatement() = %v, want %v", got, tt.want)
			}
		})
	}
}
