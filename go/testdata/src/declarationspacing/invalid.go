package declarationspacing

func invalidDeclarationSpacing() {
	var result = compute() // want `declaration-spacing: add a blank line after this statement`
	render(result)
}

func invalidDeclarationSpacingWithGroup() {
	var first = compute()
	var second = computeTwo() // want `declaration-spacing: add a blank line after this statement`
	render(second)
	_ = first
}
