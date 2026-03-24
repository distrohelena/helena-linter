package diag

import "testing"

func TestMessageHelpers(t *testing.T) {
	t.Run("message", func(t *testing.T) {
		got := Message(RuleExitSpacing, "add a blank line before this statement")
		want := "exit-spacing: add a blank line before this statement"
		if got != want {
			t.Fatalf("Message() = %q, want %q", got, want)
		}
	})

	t.Run("before", func(t *testing.T) {
		got := MissingBlankLineBefore(RuleDeclarationSpacing)
		want := "declaration-spacing: add a blank line before this statement"
		if got != want {
			t.Fatalf("MissingBlankLineBefore() = %q, want %q", got, want)
		}
	})

	t.Run("after", func(t *testing.T) {
		got := MissingBlankLineAfter(RuleIfLeadingSpacing)
		want := "if-leading-spacing: add a blank line after this statement"
		if got != want {
			t.Fatalf("MissingBlankLineAfter() = %q, want %q", got, want)
		}
	})
}
