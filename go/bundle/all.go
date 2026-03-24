package bundle

import "golang.org/x/tools/go/analysis"

// Recommended is the bundle hook for the Helena Go analyzers.
//
// The scaffold intentionally returns no analyzers yet. Task 2 will populate the
// bundle once the individual analyzers exist.
func Recommended() []*analysis.Analyzer {
	return []*analysis.Analyzer{}
}
