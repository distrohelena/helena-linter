package dev.distrohelena.linter.checkstyle.helpers;

import com.puppycrawl.tools.checkstyle.api.DetailAST;
import com.puppycrawl.tools.checkstyle.api.FileContents;

/**
 * Analyzes line spacing between sibling Checkstyle AST statements.
 */
public final class BlankLineAnalyzer {

    /**
     * Prevents direct instantiation.
     */
    private BlankLineAnalyzer() {
    }

    /**
     * Determines whether two sibling statements are separated by at least one blank line.
     *
     * @param previousStatement the earlier sibling statement.
     * @param nextStatement the following sibling statement.
     * @param fileContents the parsed file contents used to inspect line blanks.
     * @return {@code true} when a blank line exists between the statements; otherwise {@code false}.
     */
    public static boolean hasBlankLineBetween(
            DetailAST previousStatement,
            DetailAST nextStatement,
            FileContents fileContents) {
        if (previousStatement == null || nextStatement == null || fileContents == null) {
            return false;
        } else if (!StatementAstNavigator.areSiblings(previousStatement, nextStatement)) {
            return false;
        }

        int previousEndLine = getEndLine(previousStatement);
        int nextStartLine = nextStatement.getLineNo();

        if (nextStartLine - previousEndLine <= 1) {
            return false;
        }

        for (int line = previousEndLine + 1; line < nextStartLine; line++) {
            if (fileContents.lineIsBlank(line)) {
                return true;
            }
        }

        return false;
    }

    /**
     * Determines whether the supplied statement is preceded by a blank line.
     *
     * @param statement the statement to inspect.
     * @param fileContents the parsed file contents used to inspect line blanks.
     * @return {@code true} when a blank line exists before the statement; otherwise {@code false}.
     */
    public static boolean hasBlankLineBefore(DetailAST statement, FileContents fileContents) {
        if (statement == null) {
            return false;
        }

        return hasBlankLineBetween(StatementAstNavigator.getPreviousSibling(statement), statement, fileContents);
    }

    /**
     * Determines whether the supplied statement is followed by a blank line.
     *
     * @param statement the statement to inspect.
     * @param fileContents the parsed file contents used to inspect line blanks.
     * @return {@code true} when a blank line exists after the statement; otherwise {@code false}.
     */
    public static boolean hasBlankLineAfter(DetailAST statement, FileContents fileContents) {
        if (statement == null) {
            return false;
        }

        return hasBlankLineBetween(statement, StatementAstNavigator.getNextSibling(statement), fileContents);
    }

    /**
     * Computes the last line occupied by the supplied AST subtree.
     *
     * @param node the AST node whose end line should be resolved.
     * @return the deepest line number reachable from the node.
     */
    private static int getEndLine(DetailAST node) {
        int endLine = node.getLineNo();

        for (DetailAST child = node.getFirstChild(); child != null; child = child.getNextSibling()) {
            int childEndLine = getEndLine(child);

            if (childEndLine > endLine) {
                endLine = childEndLine;
            }
        }

        return endLine;
    }
}
