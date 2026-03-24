package bundle

import "golang.org/x/tools/go/analysis"

// Recommended returns the default Helena analyzer set for Go consumers.
func Recommended() []*analysis.Analyzer {
	return nil
}
