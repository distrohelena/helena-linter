package controlblockfollowingspacing

func invalidControlBlockFollowingSpacingFor() {
	for i := 0; i < 1; i++ { // want `control-block-following-spacing: add a blank line after this statement`
		println()
	}
	println()
}

func invalidControlBlockFollowingSpacingSwitch() {
	switch 1 { // want `control-block-following-spacing: add a blank line after this statement`
	case 1:
		println()
	}
	println()
}

func invalidControlBlockFollowingSpacingTypeSwitch() {
	var value any
	switch value.(type) { // want `control-block-following-spacing: add a blank line after this statement`
	default:
		println()
	}
	println()
}

func invalidControlBlockFollowingSpacingSelect() {
	select { // want `control-block-following-spacing: add a blank line after this statement`
	default:
		println()
	}
	println()
}
