package ifelseifchain

func invalidSiblingIf(first, second bool) {
	if first {
		return
	}
	if second { // want `if-else-if-chain: join this if to the previous branch with else if`
		println("second")
	}
}

func invalidSiblingIfWithComment(first, second bool) {
	if first {
		return
	}
	/* keep this */
	if second { // want `if-else-if-chain: join this if to the previous branch with else if`
		println("second")
	}
}
