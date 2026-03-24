package redundantelseif

func invalidNilComparison() {
	var value any
	if value == nil {
		println("nil")
	} else if value != nil { // want `redundant-else-if: replace this else if with else`
		println("not nil")
	}
}

func invalidFlagComparison() {
	flag := false
	if !flag {
		println("false")
	} else if flag { // want `redundant-else-if: replace this else if with else`
		println("true")
	}
}
