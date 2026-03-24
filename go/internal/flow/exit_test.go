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
		{name: "bare break is not context-free exit", stmt: mustStmt(t, `package p

func f() {
	break
}`), want: false},
		{name: "bare continue is not context-free exit", stmt: mustStmt(t, `package p

func f() {
	continue
}`), want: false},
		{name: "bare goto is not context-free exit", stmt: mustStmt(t, `package p

func f() {
	goto done
done:
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

func TestIsHelenaExitSpacingStatement(t *testing.T) {
	tests := []struct {
		name string
		stmt ast.Stmt
		want bool
	}{
		{name: "return is spacing exit", stmt: mustStmt(t, `package p

func f() {
	return
}`), want: true},
		{name: "labeled return is spacing exit", stmt: mustStmt(t, `package p

func f() {
	done:
		return
}`), want: true},
		{name: "labeled break is spacing exit", stmt: mustStmt(t, `package p

func f() {
	done:
		break
}`), want: true},
		{name: "break is spacing exit", stmt: mustStmt(t, `package p

func f() {
	break
}`), want: true},
		{name: "continue is spacing exit", stmt: mustStmt(t, `package p

func f() {
	continue
}`), want: true},
		{name: "goto is spacing exit", stmt: mustStmt(t, `package p

func f() {
	goto done
done:
}`), want: true},
		{name: "nested if is not a spacing exit", stmt: mustStmt(t, `package p

func f() {
	if cond {
		return
	}
}`), want: false},
		{name: "plain statement is not a spacing exit", stmt: mustStmt(t, `package p

func f() {
	defer cleanup()
}`), want: false},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := IsHelenaExitSpacingStatement(tt.stmt); got != tt.want {
				t.Fatalf("IsHelenaExitSpacingStatement() = %v, want %v", got, tt.want)
			}
		})
	}
}
