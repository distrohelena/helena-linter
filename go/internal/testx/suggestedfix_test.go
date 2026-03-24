package testx

import (
	"fmt"
	"go/token"
	"testing"

	"golang.org/x/tools/go/analysis"
)

type fakeTB struct {
	fatalMsg string
}

func (f *fakeTB) Helper() {}

func (f *fakeTB) Fatalf(format string, args ...any) {
	f.fatalMsg = fmt.Sprintf(format, args...)
	panic(f.fatalMsg)
}

func expectFatal(t *testing.T, want string, fn func()) {
	t.Helper()

	defer func() {
		r := recover()
		if r == nil {
			t.Fatalf("expected panic with %q", want)
		}
		got, ok := r.(string)
		if !ok {
			t.Fatalf("panic value = %#v, want string %q", r, want)
		}
		if got != want {
			t.Fatalf("panic value = %q, want %q", got, want)
		}
	}()

	fn()
}

func TestRequireSuggestedFix(t *testing.T) {
	t.Run("returns matching fix", func(t *testing.T) {
		diagnostic := analysis.Diagnostic{
			SuggestedFixes: []analysis.SuggestedFix{
				{Message: "other"},
				{Message: "insert blank line"},
			},
		}

		got := RequireSuggestedFix(t, diagnostic, "insert blank line")
		if got.Message != "insert blank line" {
			t.Fatalf("RequireSuggestedFix() = %q, want %q", got.Message, "insert blank line")
		}
	})

	t.Run("returns all matching fixes", func(t *testing.T) {
		fixes := SuggestedFixesByMessage(analysis.Diagnostic{
			SuggestedFixes: []analysis.SuggestedFix{
				{Message: "duplicate"},
				{Message: "other"},
				{Message: "duplicate"},
			},
		}, "duplicate")

		if len(fixes) != 2 {
			t.Fatalf("SuggestedFixesByMessage() returned %d fixes, want 2", len(fixes))
		}
		if fixes[0].Message != "duplicate" || fixes[1].Message != "duplicate" {
			t.Fatalf("SuggestedFixesByMessage() = %#v, want two duplicate fixes", fixes)
		}
	})

	t.Run("reports missing fix", func(t *testing.T) {
		fake := &fakeTB{}
		want := `did not find suggested fix with message "missing fix"`
		expectFatal(t, want, func() {
			RequireSuggestedFix(fake, analysis.Diagnostic{}, "missing fix")
		})
	})

	t.Run("fails on duplicate matches", func(t *testing.T) {
		fake := &fakeTB{}
		want := `found multiple suggested fixes with message "duplicate"`
		expectFatal(t, want, func() {
			RequireSuggestedFix(fake, analysis.Diagnostic{
				SuggestedFixes: []analysis.SuggestedFix{
					{Message: "duplicate"},
					{Message: "duplicate"},
				},
			}, "duplicate")
		})
	})

	t.Run("returns edits intact", func(t *testing.T) {
		fix := RequireSuggestedFix(t, analysis.Diagnostic{
			SuggestedFixes: []analysis.SuggestedFix{
				{
					Message: "rewrite",
					TextEdits: []analysis.TextEdit{
						{
							Pos: token.Pos(1),
							End: token.Pos(2),
						},
					},
				},
			},
		}, "rewrite")

		if len(fix.TextEdits) != 1 {
			t.Fatalf("RequireSuggestedFix() returned %d edits, want 1", len(fix.TextEdits))
		}
	})
}
