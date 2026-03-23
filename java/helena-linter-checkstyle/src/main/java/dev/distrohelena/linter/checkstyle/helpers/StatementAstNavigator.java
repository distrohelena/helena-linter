package dev.distrohelena.linter.checkstyle.helpers;

import com.puppycrawl.tools.checkstyle.api.DetailAST;

/**
 * Provides small navigation helpers for walking across Checkstyle sibling AST nodes.
 */
public final class StatementAstNavigator {

    /**
     * Prevents direct instantiation.
     */
    private StatementAstNavigator() {
    }

    /**
     * Gets the immediate previous sibling for the supplied AST node.
     *
     * @param node the node whose previous sibling should be resolved.
     * @return the previous sibling node, or {@code null} when the node has none.
     */
    public static DetailAST getPreviousSibling(DetailAST node) {
        if (node == null) {
            return null;
        }

        return node.getPreviousSibling();
    }

    /**
     * Gets the immediate next sibling for the supplied AST node.
     *
     * @param node the node whose next sibling should be resolved.
     * @return the next sibling node, or {@code null} when the node has none.
     */
    public static DetailAST getNextSibling(DetailAST node) {
        if (node == null) {
            return null;
        }

        return node.getNextSibling();
    }

    /**
     * Determines whether two nodes share the same parent node.
     *
     * @param first the first node to compare.
     * @param second the second node to compare.
     * @return {@code true} when both nodes belong to the same sibling list; otherwise {@code false}.
     */
    public static boolean areSiblings(DetailAST first, DetailAST second) {
        if (first == null || second == null) {
            return false;
        } else if (first.getParent() == null) {
            return false;
        } else if (second.getParent() == null) {
            return false;
        }

        return first.getParent() == second.getParent();
    }
}
