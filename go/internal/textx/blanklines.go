package textx

import (
	"go/ast"
	"go/token"
	"sort"
)

type lineSpan struct {
	start int
	end   int
}

// HasBlankLineBetween reports whether there is at least one blank line between
// two adjacent AST nodes, accounting for intervening comment groups.
func HasBlankLineBetween(fset *token.FileSet, comments []*ast.CommentGroup, left, right ast.Node) bool {
	if fset == nil || left == nil || right == nil {
		return false
	}

	leftSpan, ok := nodeLineSpan(fset, left)
	if !ok {
		return false
	}
	rightSpan, ok := nodeLineSpan(fset, right)
	if !ok {
		return false
	}

	spans := []lineSpan{leftSpan}
	for _, group := range comments {
		if group == nil {
			continue
		}

		commentSpan, ok := commentGroupLineSpan(fset, group)
		if !ok {
			continue
		}
		if commentSpan.end <= leftSpan.end || commentSpan.start >= rightSpan.start {
			continue
		}
		spans = append(spans, commentSpan)
	}
	spans = append(spans, rightSpan)

	sort.Slice(spans, func(i, j int) bool {
		if spans[i].start == spans[j].start {
			return spans[i].end < spans[j].end
		}
		return spans[i].start < spans[j].start
	})

	for i := 1; i < len(spans); i++ {
		if spans[i].start-spans[i-1].end > 1 {
			return true
		}
	}
	return false
}

func nodeLineSpan(fset *token.FileSet, node ast.Node) (lineSpan, bool) {
	start := fset.Position(node.Pos()).Line
	end := fset.Position(node.End()).Line
	if start == 0 || end == 0 {
		return lineSpan{}, false
	}
	return lineSpan{start: start, end: end}, true
}

func commentGroupLineSpan(fset *token.FileSet, group *ast.CommentGroup) (lineSpan, bool) {
	start := fset.Position(group.Pos()).Line
	end := fset.Position(group.End()).Line
	if start == 0 || end == 0 {
		return lineSpan{}, false
	}
	return lineSpan{start: start, end: end}, true
}
