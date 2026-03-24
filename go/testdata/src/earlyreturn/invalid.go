package earlyreturn

func invalidWrappedHappyPath(err error) error {
	if err == nil { // want `early-return: rewrite this if as a guard clause with an early return`
		touch()
		return nil
	}
	return wrapErr(err)
}

func invalidIfElse(flag bool) error {
	if flag { // want `early-return: rewrite this if as a guard clause with an early return`
		touch()
	} else {
		return newErr()
	}
	return nil
}

func touch() {}

func wrapErr(err error) error { return err }

func newErr() error { return nil }
