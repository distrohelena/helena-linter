package diag

import (
	"fmt"
	"strings"
	"unicode"
)

type RuleID string

const (
	RuleControlBlockFollowingSpacing RuleID = "control-block-following-spacing"
	RuleDeclarationLeadingSpacing    RuleID = "declaration-leading-spacing"
	RuleDeclarationSpacing           RuleID = "declaration-spacing"
	RuleEarlyReturn                  RuleID = "early-return"
	RuleExitSpacing                  RuleID = "exit-spacing"
	RuleIfElseIfChain                RuleID = "if-else-if-chain"
	RuleIfFollowingSpacing           RuleID = "if-following-spacing"
	RuleIfLeadingSpacing             RuleID = "if-leading-spacing"
	RuleMultilineBlockLayout         RuleID = "multiline-block-layout"
	RuleRedundantElseIf              RuleID = "redundant-else-if"
)

func (r RuleID) String() string {
	return string(r)
}

// Validate reports whether the rule identifier is well formed.
func (r RuleID) Validate() error {
	raw := string(r)
	if raw == "" {
		return fmt.Errorf("rule id cannot be empty")
	}
	if strings.TrimSpace(raw) != raw {
		return fmt.Errorf("rule id %q cannot contain leading or trailing whitespace", raw)
	}
	for _, ch := range raw {
		if unicode.IsUpper(ch) {
			return fmt.Errorf("rule id %q must be lowercase", raw)
		}
		if !(unicode.IsLower(ch) || unicode.IsDigit(ch) || ch == '-') {
			return fmt.Errorf("rule id %q contains invalid character %q", raw, ch)
		}
	}
	return nil
}

func mustRuleID(rule RuleID) RuleID {
	if err := rule.Validate(); err != nil {
		panic(err)
	}
	return rule
}

// Message formats a rule-scoped diagnostic message with a validated Helena rule name.
func Message(rule RuleID, format string, args ...any) string {
	mustRuleID(rule)
	if format == "" {
		return rule.String()
	}
	return fmt.Sprintf("%s: %s", rule, fmt.Sprintf(format, args...))
}

// MissingBlankLineBefore reports a blank-line omission before a statement.
func MissingBlankLineBefore(rule RuleID) string {
	return Message(rule, "add a blank line before this statement")
}

// MissingBlankLineAfter reports a blank-line omission after a statement.
func MissingBlankLineAfter(rule RuleID) string {
	return Message(rule, "add a blank line after this statement")
}
