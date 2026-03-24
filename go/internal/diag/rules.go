package diag

import "fmt"

const (
	RuleControlBlockFollowingSpacing = "control-block-following-spacing"
	RuleDeclarationLeadingSpacing    = "declaration-leading-spacing"
	RuleDeclarationSpacing           = "declaration-spacing"
	RuleEarlyReturn                  = "early-return"
	RuleExitSpacing                  = "exit-spacing"
	RuleIfElseIfChain                = "if-else-if-chain"
	RuleIfFollowingSpacing           = "if-following-spacing"
	RuleIfLeadingSpacing             = "if-leading-spacing"
	RuleMultilineBlockLayout         = "multiline-block-layout"
	RuleRedundantElseIf              = "redundant-else-if"
)

// Message formats a rule-scoped diagnostic message with a stable Helena rule name.
func Message(rule, format string, args ...any) string {
	prefix := rule
	if format == "" {
		return prefix
	}
	return fmt.Sprintf("%s: %s", prefix, fmt.Sprintf(format, args...))
}

// MissingBlankLineBefore reports a blank-line omission before a statement.
func MissingBlankLineBefore(rule string) string {
	return Message(rule, "add a blank line before this statement")
}

// MissingBlankLineAfter reports a blank-line omission after a statement.
func MissingBlankLineAfter(rule string) string {
	return Message(rule, "add a blank line after this statement")
}
