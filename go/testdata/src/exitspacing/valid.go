package exitspacing

func validExitSpacingReturn() int {
	println()

	return 1
}

func validExitSpacingBreak() {
	for {
		println()

		break
	}
}

func validExitSpacingContinue() {
	for {
		println()

		continue
	}
}

func validExitSpacingGoto() {
	println()

	goto done
done:
}
