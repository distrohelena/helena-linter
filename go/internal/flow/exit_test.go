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

func TestDefinitelyExitsControlFlow(t *testing.T) {
	tests := []struct {
		name string
		stmt ast.Stmt
		want bool
	}{
		{name: "return exits", stmt: mustStmt(t, `package p

func f() {
	return
}`), want: true},
		{name: "block ending in return exits", stmt: mustStmt(t, `package p

func f() {
	{
		return
	}
}`), want: true},
		{name: "if with both branches exiting exits", stmt: mustStmt(t, `package p

func f() {
	if cond {
		return
	} else {
		return
	}
}`), want: true},
		{name: "labeled return exits", stmt: mustStmt(t, `package p

func f() {
	done:
		return
}`), want: true},
		{name: "if with one non-exiting branch does not exit", stmt: mustStmt(t, `package p

func f() {
	if cond {
		return
	} else {
		cleanup()
	}
}`), want: false},
		{name: "non-exit statement", stmt: mustStmt(t, `package p

func f() {
	defer cleanup()
}`), want: false},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := DefinitelyExitsControlFlow(tt.stmt); got != tt.want {
				t.Fatalf("DefinitelyExitsControlFlow() = %v, want %v", got, tt.want)
			}
		})
	}
}

func TestIsHelenaExitStatement(t *testing.T) {
	tests := []struct {
		name string
		stmt ast.Stmt
		want bool
	}{
		{name: "return", stmt: mustStmt(t, `package p

func f() {
	return
}`), want: true},
		{name: "break", stmt: mustStmt(t, `package p

func f() {
	break
}`), want: true},
		{name: "continue", stmt: mustStmt(t, `package p

func f() {
	continue
}`), want: true},
		{name: "goto", stmt: mustStmt(t, `package p

func f() {
	goto done
done:
}`), want: true},
		{name: "labeled return", stmt: mustStmt(t, `package p

func f() {
	done:
		return
}`), want: true},
		{name: "synthetic label with no body", stmt: &ast.LabeledStmt{Label: ast.NewIdent("done")}, want: false},
		{name: "non-exit", stmt: mustStmt(t, `package p

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

func TestDefinitelyExitsControlFlowNilAndSyntheticCases(t *testing.T) {
	t.Run("nil if body is conservative false", func(t *testing.T) {
		if got := DefinitelyExitsControlFlow(&ast.IfStmt{}); got {
			t.Fatal("DefinitelyExitsControlFlow(&ast.IfStmt{}) = true, want false")
		}
	})

	t.Run("synthetic labeled nil body is conservative false", func(t *testing.T) {
		if got := DefinitelyExitsControlFlow(&ast.LabeledStmt{Label: ast.NewIdent("done")}); got {
			t.Fatal("DefinitelyExitsControlFlow(&ast.LabeledStmt{Name: ...}) = true, want false")
		}
	})
}
