package iffollowingspacing

func invalidIfFollowingSpacing() {
	if true { // want `if-following-spacing: add a blank line after this statement`
		println()
	}
	println()
}

func invalidIfFollowingSpacingChain() {
	if true { // want `if-following-spacing: add a blank line after this statement`
		println()
	} else if false {
		println()
	} else {
		println()
	}
	println()
}
