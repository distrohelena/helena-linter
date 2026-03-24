// Package multilineblocklayout implements the multiline-block-layout rule.
package multilineblocklayout

import (
	"bytes"
	"go/ast"
	"go/format"
	"go/token"
	"os"
	"strings"

	"github.com/distrohelena/helena-linter/go/internal/diag"
	"github.com/distrohelena/helena-linter/go/internal/fixx"
	"golang.org/x/tools/go/analysis"
)

// Analyzer reports non-empty single-line if, for, and switch blocks.
var Analyzer = &analysis.Analyzer{
	Name: "multilineblocklayout",
	Doc:  "reports non-empty single-line if, for, and switch blocks",
	Run:  run,
}

func run(pass *analysis.Pass) (any, error) {
	for _, file := range pass.Files {
		file := file
		ast.Inspect(file, func(n ast.Node) bool {
			switch stmt := n.(type) {
			case *ast.IfStmt:
				reportBlock(pass, stmt.Body)
			case *ast.ForStmt:
				reportBlock(pass, stmt.Body)
			case *ast.SwitchStmt:
				reportBlock(pass, stmt.Body)
			}
			return true
		})
	}

	return nil, nil
}

func reportBlock(pass *analysis.Pass, block *ast.BlockStmt) {
	if pass == nil || pass.Fset == nil || block == nil || len(block.List) == 0 {
		return
	}

	if !isSingleLineBlock(pass.Fset, block) {
		return
	}

	edit, ok := multilineBlockEdit(pass.Fset, block)
	if !ok {
		return
	}

	pass.Report(analysis.Diagnostic{
		Pos:     block.Lbrace,
		Message: diag.Message(diag.RuleMultilineBlockLayout, "break this non-empty block across multiple lines"),
		SuggestedFixes: []analysis.SuggestedFix{
			{
				Message:   "Use multiline block layout",
				TextEdits: []analysis.TextEdit{edit},
			},
		},
	})
}

func isSingleLineBlock(fset *token.FileSet, block *ast.BlockStmt) bool {
	if fset == nil || block == nil {
		return false
	}

	start := fset.Position(block.Lbrace).Line
	end := fset.Position(block.Rbrace).Line
	return start != 0 && start == end
}

func multilineBlockEdit(fset *token.FileSet, block *ast.BlockStmt) (analysis.TextEdit, bool) {
	file := fset.File(block.Lbrace)
	if file == nil {
		return analysis.TextEdit{}, false
	}

	source, err := os.ReadFile(file.Name())
	if err != nil {
		return analysis.TextEdit{}, false
	}

	var rendered bytes.Buffer
	if err := format.Node(&rendered, fset, block); err != nil {
		return analysis.TextEdit{}, false
	}

	blockText := rendered.String()
	blockText = strings.ReplaceAll(blockText, "\n", detectNewline(source))
	blockText = indentFollowingLines(blockText, leadingWhitespace(source, file.Offset(block.Lbrace)))

	start := block.Lbrace
	end := file.Pos(file.Offset(block.Rbrace) + 1)
	return fixx.Replace(start, end, blockText), true
}

func leadingWhitespace(source []byte, offset int) string {
	if offset <= 0 {
		return ""
	}

	lineStart := bytes.LastIndexByte(source[:offset], '\n') + 1
	end := lineStart
	for end < len(source) {
		switch source[end] {
		case ' ', '\t':
			end++
		default:
			return string(source[lineStart:end])
		}
	}
	return string(source[lineStart:end])
}

func indentFollowingLines(text, indent string) string {
	if indent == "" {
		return text
	}

	lines := strings.Split(text, "\n")
	for i := 1; i < len(lines); i++ {
		lines[i] = indent + lines[i]
	}
	return strings.Join(lines, "\n")
}

func detectNewline(source []byte) string {
	if bytes.Contains(source, []byte("\r\n")) {
		return "\r\n"
	}
	return "\n"
}
