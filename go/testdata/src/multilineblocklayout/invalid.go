package multilineblocklayout

func invalidIfBlock() {
	if ready { run() } // want `multiline-block-layout: break this non-empty block across multiple lines`
}

func invalidForBlock() {
	for i := 0; i < 3; i++ { run(i) } // want `multiline-block-layout: break this non-empty block across multiple lines`
}

func invalidSwitchBlock() {
	switch n := 1; n { case 1: run(n) } // want `multiline-block-layout: break this non-empty block across multiple lines`
}
