package declarationleadingspacing

import (
	"go/ast"
	"go/parser"
	"go/token"
	"path/filepath"
	"testing"

	"golang.org/x/tools/go/analysis"
	"golang.org/x/tools/go/analysis/analysistest"
)

func TestAnalyzer(t *testing.T) {
	analysistest.Run(t, mustAbs(t, filepath.Join("..", "..", "testdata")), Analyzer, "declarationleadingspacing")
}

func TestSuggestedFixPreservesComments(t *testing.T) {
	const src = `package p

func logStart() {}

func compute() int { return 1 }

func f() {
	logStart()
	// keep this comment attached to the declaration
	var result = compute()
	_ = result
}
`
	const want = `package p

func logStart() {}

func compute() int { return 1 }

func f() {
	logStart()

	// keep this comment attached to the declaration
	var result = compute()
	_ = result
}
`

	got := applySingleFix(t, src)
	if got != want {
		t.Fatalf("fixed source mismatch\n--- got ---\n%s\n--- want ---\n%s", got, want)
	}
}

func mustAbs(t *testing.T, path string) string {
	t.Helper()

	abs, err := filepath.Abs(path)
	if err != nil {
		t.Fatalf("filepath.Abs(%q) error = %v", path, err)
	}
	return abs
}

func applySingleFix(t *testing.T, src string) string {
	t.Helper()

	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "input.go", src, parser.ParseComments)
	if err != nil {
		t.Fatalf("ParseFile() error = %v", err)
	}

	var diagnostics []analysis.Diagnostic
	pass := &analysis.Pass{
		Fset:  fset,
		Files: []*ast.File{file},
		Report: func(d analysis.Diagnostic) {
			diagnostics = append(diagnostics, d)
		},
	}
	if _, err := Analyzer.Run(pass); err != nil {
		t.Fatalf("Analyzer.Run() error = %v", err)
	}
	if len(diagnostics) != 1 {
		t.Fatalf("Analyzer.Run() reported %d diagnostics, want 1", len(diagnostics))
	}
	if len(diagnostics[0].SuggestedFixes) != 1 {
		t.Fatalf("diagnostic reported %d suggested fixes, want 1", len(diagnostics[0].SuggestedFixes))
	}
	fix := diagnostics[0].SuggestedFixes[0]
	if len(fix.TextEdits) != 1 {
		t.Fatalf("suggested fix contained %d edits, want 1", len(fix.TextEdits))
	}

	edit := fix.TextEdits[0]
	start := fset.Position(edit.Pos).Offset
	end := fset.Position(edit.End).Offset
	return src[:start] + string(edit.NewText) + src[end:]
}
