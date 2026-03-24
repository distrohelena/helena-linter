package testx

import "golang.org/x/tools/go/analysis"

type fatallyFailable interface {
	Helper()
	Fatalf(format string, args ...any)
}

// RequireSuggestedFix returns the unique fix with the requested message and fails the test if the
// fix is missing or duplicated.
func RequireSuggestedFix(t fatallyFailable, diagnostics []analysis.Diagnostic, wantMessage string) analysis.SuggestedFix {
	t.Helper()

	var (
		matched     bool
		matchingFix analysis.SuggestedFix
	)

	for _, diagnostic := range diagnostics {
		for _, fix := range diagnostic.SuggestedFixes {
			if fix.Message == wantMessage {
				if matched {
					t.Fatalf("found multiple suggested fixes with message %q", wantMessage)
				}
				matched = true
				matchingFix = fix
			}
		}
	}

	if !matched {
		t.Fatalf("did not find suggested fix with message %q", wantMessage)
	}

	return matchingFix
}
