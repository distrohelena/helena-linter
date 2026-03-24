package declarationleadingspacing

func invalidLeadingSpacing() {
	logStart()
	var result = compute() // want `declaration-leading-spacing: add a blank line before this statement`
	_ = result
}

func invalidLeadingSpacingWithComment() {
	logStart()
	// this comment should stay attached to the declaration
	var another = compute() // want `declaration-leading-spacing: add a blank line before this statement`
	_ = another
}
