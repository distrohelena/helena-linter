package diag

import "testing"

func TestRuleIDValidate(t *testing.T) {
	t.Run("accepts canonical ids", func(t *testing.T) {
		if err := RuleExitSpacing.Validate(); err != nil {
			t.Fatalf("RuleExitSpacing.Validate() returned error: %v", err)
		}
	})

	t.Run("rejects empty ids", func(t *testing.T) {
		if err := RuleID("").Validate(); err == nil {
			t.Fatal("RuleID(\"\").Validate() returned nil error")
		}
	})

	t.Run("rejects malformed ids", func(t *testing.T) {
		for _, rule := range []RuleID{"Bad-Rule", "bad rule", " bad", "bad_underscore"} {
			if err := rule.Validate(); err == nil {
				t.Fatalf("%q.Validate() returned nil error", rule)
			}
		}
	})
}

func TestMessagePanicsOnInvalidRuleID(t *testing.T) {
	defer func() {
		r := recover()
		err, ok := r.(error)
		if !ok {
			t.Fatalf("panic value = %#v, want error", r)
		}
		want := `rule id "bad rule" contains invalid character ' '`
		if err.Error() != want {
			t.Fatalf("panic value = %q, want %q", err.Error(), want)
		}
	}()

	_ = Message(RuleID("bad rule"), "message")
}
