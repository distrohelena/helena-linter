package declarationspacing

func compute() int { return 1 }

func computeTwo() int { return 2 }

func render(int) {}

func validDeclarationSpacing() {
	var result = compute()

	render(result)
}

func validDeclarationSpacingWithGroup() {
	var first = compute()
	var second = compute()

	render(second)
	_ = first
}
