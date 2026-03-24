package redundantelseif

func validDifferentCondition() {
	var value any
	other := true
	if value == nil {
		println("nil")
	} else if other {
		println("other")
	}
}

func validSamePolarity() {
	flag := false
	if !flag {
		println("false")
	} else if !flag {
		println("still false")
	}
}
