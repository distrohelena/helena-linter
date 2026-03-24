package testx

import (
	"go/token"
	"testing"

	"golang.org/x/tools/go/analysis"
)

type fakeTB struct {
	fatalMsg string
}

func (f *fakeTB) Helper() {}

func (f *fakeTB) Fatalf(format string, args ...any) {
	f.fatalMsg = format
}

func TestRequireSuggestedFix(t *testing.T) {
	t.Run("returns matching fix", func(t *testing.T) {
		diagnostics := []analysis.Diagnostic{
			{
				SuggestedFixes: []analysis.SuggestedFix{
					{Message: "other"},
					{Message: "insert blank line"},
				},
			},
		}

		got := RequireSuggestedFix(t, diagnostics, "insert blank line")
		if got.Message != "insert blank line" {
			t.Fatalf("RequireSuggestedFix() = %q, want %q", got.Message, "insert blank line")
		}
	})

	t.Run("reports missing fix", func(t *testing.T) {
		fake := &fakeTB{}
		got := RequireSuggestedFix(fake, []analysis.Diagnostic{{}}, "missing fix")
		if got.Message != "" || len(got.TextEdits) != 0 {
			t.Fatalf("RequireSuggestedFix() = %#v, want zero value", got)
		}
		want := "did not find suggested fix with message %q"
		if fake.fatalMsg != want {
			t.Fatalf("Fatalf format = %q, want %q", fake.fatalMsg, want)
		}
	})

	t.Run("returns edits intact", func(t *testing.T) {
		fix := RequireSuggestedFix(t, []analysis.Diagnostic{
			{
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
			},
		}, "rewrite")

		if len(fix.TextEdits) != 1 {
			t.Fatalf("RequireSuggestedFix() returned %d edits, want 1", len(fix.TextEdits))
		}
	})
}
