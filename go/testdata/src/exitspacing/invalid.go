package exitspacing

func invalidExitSpacingReturn() int {
	println()
	return 1 // want `exit-spacing: add a blank line before this statement`
}

func invalidExitSpacingBreak() {
	for {
		println()
		break // want `exit-spacing: add a blank line before this statement`
	}
}

func invalidExitSpacingContinue() {
	for {
		println()
		continue // want `exit-spacing: add a blank line before this statement`
	}
}

func invalidExitSpacingGoto() {
	println()
	goto done // want `exit-spacing: add a blank line before this statement`
done:
}
