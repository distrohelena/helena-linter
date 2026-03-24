package bundle

import (
	"github.com/distrohelena/helena-linter/go/analyzers/controlblockfollowingspacing"
	"github.com/distrohelena/helena-linter/go/analyzers/declarationleadingspacing"
	"github.com/distrohelena/helena-linter/go/analyzers/declarationspacing"
	"github.com/distrohelena/helena-linter/go/analyzers/earlyreturn"
	"github.com/distrohelena/helena-linter/go/analyzers/exitspacing"
	"github.com/distrohelena/helena-linter/go/analyzers/ifelseifchain"
	"github.com/distrohelena/helena-linter/go/analyzers/iffollowingspacing"
	"github.com/distrohelena/helena-linter/go/analyzers/ifleadingspacing"
	"github.com/distrohelena/helena-linter/go/analyzers/multilineblocklayout"
	"github.com/distrohelena/helena-linter/go/analyzers/redundantelseif"
	"golang.org/x/tools/go/analysis"
)

// Recommended returns the Helena Go analyzer bundle.
func Recommended() []*analysis.Analyzer {
	return []*analysis.Analyzer{
		controlblockfollowingspacing.Analyzer,
		declarationleadingspacing.Analyzer,
		declarationspacing.Analyzer,
		earlyreturn.Analyzer,
		exitspacing.Analyzer,
		ifelseifchain.Analyzer,
		iffollowingspacing.Analyzer,
		ifleadingspacing.Analyzer,
		multilineblocklayout.Analyzer,
		redundantelseif.Analyzer,
	}
}

