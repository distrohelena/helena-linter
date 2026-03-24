package ifelseifchain

func validNestedIf(first, second bool) {
	if first {
		if second {
			return
		}
	}
	if second {
		println("second")
	}
}

func validElseIfChain(first, second bool) {
	if first {
		return
	} else if second {
		println("second")
	}
}
