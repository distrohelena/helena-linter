package ifleadingspacing

func invalidIfLeadingSpacing() {
	println()
	if true { // want `if-leading-spacing: add a blank line before this statement`
		println()
	}
}

func invalidIfLeadingSpacingWithElse() {
	println()
	if true { // want `if-leading-spacing: add a blank line before this statement`
		println()
	} else {
		println()
	}
}
