package controlblockfollowingspacing

func validControlBlockFollowingSpacingFor() {
	for i := 0; i < 1; i++ {
		println()
	}

	println()
}

func validControlBlockFollowingSpacingSwitch() {
	switch 1 {
	case 1:
		println()
	}

	println()
}

func validControlBlockFollowingSpacingTypeSwitch() {
	var value any
	switch value.(type) {
	default:
		println()
	}

	println()
}

func validControlBlockFollowingSpacingSelect() {
	select {
	default:
		println()
	}

	println()
}
