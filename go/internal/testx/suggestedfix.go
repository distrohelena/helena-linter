package testx

import "golang.org/x/tools/go/analysis"

type fatallyFailable interface {
	Helper()
	Fatalf(format string, args ...any)
}

// RequireSuggestedFix returns the first fix with the requested message and fails the test if it
// is not present.
func RequireSuggestedFix(t fatallyFailable, diagnostics []analysis.Diagnostic, wantMessage string) analysis.SuggestedFix {
	t.Helper()

	for _, diagnostic := range diagnostics {
		for _, fix := range diagnostic.SuggestedFixes {
			if fix.Message == wantMessage {
				return fix
			}
		}
	}

	t.Fatalf("did not find suggested fix with message %q", wantMessage)
	return analysis.SuggestedFix{}
}
