package multilineblocklayout

var ready = true

func run(args ...int) {}

func validEmptyIfBlock() {
	if ready {
	}
}

func validEmptyForBlock() {
	for {
	}
}

func validEmptySwitchBlock() {
	switch n := 1; n {
	}
}

func validCommentOnlyIfBlock() {
	if ready { /* keep */
	}
}

func validCommentOnlySwitchBlock() {
	switch ready { /* keep */
	}
}
