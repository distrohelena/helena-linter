package testx

import "golang.org/x/tools/go/analysis"

type fatallyFailable interface {
	Helper()
	Fatalf(format string, args ...any)
}

// SuggestedFixesByMessage returns every fix within one diagnostic whose message matches wantMessage.
func SuggestedFixesByMessage(diagnostic analysis.Diagnostic, wantMessage string) []analysis.SuggestedFix {
	var fixes []analysis.SuggestedFix
	for _, fix := range diagnostic.SuggestedFixes {
		if fix.Message == wantMessage {
			fixes = append(fixes, fix)
		}
	}
	return fixes
}

// RequireSuggestedFix returns the unique fix with the requested message from one diagnostic and
// fails the test if the fix is missing or duplicated.
func RequireSuggestedFix(t fatallyFailable, diagnostic analysis.Diagnostic, wantMessage string) analysis.SuggestedFix {
	t.Helper()

	fixes := SuggestedFixesByMessage(diagnostic, wantMessage)

	switch len(fixes) {
	case 0:
		t.Fatalf("did not find suggested fix with message %q", wantMessage)
	case 1:
		return fixes[0]
	default:
		t.Fatalf("found multiple suggested fixes with message %q", wantMessage)
	}

	return analysis.SuggestedFix{}
}
