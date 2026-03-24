package declarationleadingspacing

func logStart() {}

func compute() int { return 1 }

func validLeadingSpacing() {
	logStart()

	var result = compute()
	_ = result
}

func validLeadingSpacingWithGroup() {
	logStart()

	var first = compute()
	var second = compute()
	_ = first
	_ = second
}
