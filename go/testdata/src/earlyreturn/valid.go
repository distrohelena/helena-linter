package earlyreturn

func validWrappedHappyPathWithInterveningComment(err error) error {
	if err == nil {
		touch()
		return nil
	}
	// keep this near the fallback return
	return wrapErr(err)
}

func validIfElseWithScopedInit() error {
	if value := nextValue(); value != nil {
		touchValue(value)
	} else {
		return valueErr()
	}
	return nil
}

func validIfElseWithBoundaryComment(flag bool) error {
	if flag {
		touch()
	} /* keep this */ else {
		return newErr()
	}
	return nil
}

func nextValue() *int {
	value := 1
	return &value
}

func touchValue(*int) {}

func valueErr() error { return nil }
