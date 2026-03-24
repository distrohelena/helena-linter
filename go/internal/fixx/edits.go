package fixx

import (
	"go/token"

	"golang.org/x/tools/go/analysis"
)

// Replace returns a single text edit that replaces the token range with text.
func Replace(pos, end token.Pos, text string) analysis.TextEdit {
	return analysis.TextEdit{Pos: pos, End: end, NewText: []byte(text)}
}

// InsertAt returns a zero-width text edit that inserts text at pos.
func InsertAt(pos token.Pos, text string) analysis.TextEdit {
	return Replace(pos, pos, text)
}

// Delete returns a text edit that removes the token range.
func Delete(pos, end token.Pos) analysis.TextEdit {
	return Replace(pos, end, "")
}
