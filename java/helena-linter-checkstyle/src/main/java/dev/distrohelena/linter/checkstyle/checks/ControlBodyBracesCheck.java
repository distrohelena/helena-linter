package dev.distrohelena.linter.checkstyle.checks;

import com.puppycrawl.tools.checkstyle.api.AbstractCheck;
import com.puppycrawl.tools.checkstyle.api.DetailAST;
import com.puppycrawl.tools.checkstyle.api.TokenTypes;
import dev.distrohelena.linter.checkstyle.diagnostics.HelenaMessageIds;
import dev.distrohelena.linter.checkstyle.helpers.StatementAstNavigator;

/**
 * Requires executable control bodies to use braces, while allowing {@code else if} chaining syntax.
 */
public final class ControlBodyBracesCheck extends AbstractCheck {

    /**
     * Creates the check that enforces braced control bodies.
     */
    public ControlBodyBracesCheck() {
    }

    /**
     * Returns the token types that can contain a body governed by this rule.
     *
     * @return the token types the check should visit.
     */
    @Override
    public int[] getDefaultTokens() {
        return new int[] {
                TokenTypes.LITERAL_IF,
                TokenTypes.LITERAL_FOR,
                TokenTypes.LITERAL_WHILE,
                TokenTypes.LITERAL_DO,
                TokenTypes.LITERAL_TRY,
                TokenTypes.LITERAL_SYNCHRONIZED
        };
    }

    /**
     * Returns the token types that can contain a body governed by this rule.
     *
     * @return the token types the check should accept.
     */
    @Override
    public int[] getAcceptableTokens() {
        return getDefaultTokens();
    }

    /**
     * Returns the token types that must be processed by this rule.
     *
     * @return an empty token set because the check only relies on visit callbacks.
     */
    @Override
    public int[] getRequiredTokens() {
        return new int[0];
    }

    /**
     * Inspects the current control statement and reports any body that is not wrapped in braces.
     *
     * @param ast the control statement being visited.
     */
    @Override
    public void visitToken(DetailAST ast) {
        int tokenType = ast.getType();

        if (tokenType == TokenTypes.LITERAL_IF) {
            checkIfStatement(ast);
        } else if (tokenType == TokenTypes.LITERAL_TRY) {
            checkTryStatement(ast);
        } else {
            checkSimpleControlStatement(ast);
        }
    }

    /**
     * Validates the body and optional {@code else} branch of an {@code if} statement.
     *
     * @param ifStatement the {@code if} statement to inspect.
     */
    private void checkIfStatement(DetailAST ifStatement) {
        DetailAST thenBranch = findThenBranch(ifStatement);

        if (thenBranch != null && thenBranch.getType() != TokenTypes.SLIST) {
            log(ifStatement, HelenaMessageIds.CONTROL_BODY_BRACES);
        }

        DetailAST elseNode = ifStatement.findFirstToken(TokenTypes.LITERAL_ELSE);
        DetailAST elseBranch = findElseBranch(elseNode);

        if (elseBranch != null
                && elseBranch.getType() != TokenTypes.SLIST
                && elseBranch.getType() != TokenTypes.LITERAL_IF) {
            log(elseNode, HelenaMessageIds.CONTROL_BODY_BRACES);
        }
    }

    /**
     * Validates the body of a {@code try} statement.
     *
     * @param tryStatement the {@code try} statement to inspect.
     */
    private void checkTryStatement(DetailAST tryStatement) {
        DetailAST body = findTryBody(tryStatement);

        if (body != null && body.getType() != TokenTypes.SLIST) {
            log(tryStatement, HelenaMessageIds.CONTROL_BODY_BRACES);
        }
    }

    /**
     * Validates the body of a statement that exposes a single executable branch.
     *
     * @param controlStatement the control statement to inspect.
     */
    private void checkSimpleControlStatement(DetailAST controlStatement) {
        DetailAST body = findSimpleBody(controlStatement);

        if (body != null && body.getType() != TokenTypes.SLIST) {
            log(controlStatement, HelenaMessageIds.CONTROL_BODY_BRACES);
        }
    }

    /**
     * Locates the then-branch that follows the condition portion of an {@code if} statement.
     *
     * @param ifStatement the {@code if} statement to inspect.
     * @return the branch that executes when the condition is true.
     */
    private DetailAST findThenBranch(DetailAST ifStatement) {
        DetailAST rightParen = ifStatement.findFirstToken(TokenTypes.RPAREN);

        if (rightParen == null) {
            return null;
        }

        return StatementAstNavigator.getNextSibling(rightParen);
    }

    /**
     * Locates the optional {@code else} branch of an {@code if} statement.
     *
     * @param ifStatement the {@code if} statement to inspect.
     * @return the branch that executes when the condition is false, or {@code null} when no else exists.
     */
    private DetailAST findElseBranch(DetailAST elseNode) {
        if (elseNode == null) {
            return null;
        }

        DetailAST directChild = elseNode.getFirstChild();

        if (directChild != null) {
            return directChild;
        }

        return StatementAstNavigator.getNextSibling(elseNode);
    }

    /**
     * Locates the body of a {@code try} statement.
     *
     * @param tryStatement the {@code try} statement to inspect.
     * @return the direct block node that represents the executable body, or {@code null} when absent.
     */
    private DetailAST findTryBody(DetailAST tryStatement) {
        for (DetailAST child = tryStatement.getFirstChild(); child != null; child = child.getNextSibling()) {
            if (child.getType() == TokenTypes.SLIST) {
                return child;
            }
        }

        return null;
    }

    /**
     * Locates the body of a control statement that uses a single executable branch.
     *
     * @param controlStatement the control statement to inspect.
     * @return the immediate body statement, or {@code null} when one cannot be resolved.
     */
    private DetailAST findSimpleBody(DetailAST controlStatement) {
        if (controlStatement.getType() == TokenTypes.LITERAL_DO) {
            return findDoBody(controlStatement);
        }

        DetailAST rightParen = controlStatement.findFirstToken(TokenTypes.RPAREN);

        if (rightParen == null) {
            return null;
        }

        return StatementAstNavigator.getNextSibling(rightParen);
    }

    /**
     * Locates the statement body of a {@code do} loop before the trailing {@code while} clause.
     *
     * @param doStatement the {@code do} statement to inspect.
     * @return the first executable body node, or {@code null} when none is present.
     */
    private DetailAST findDoBody(DetailAST doStatement) {
        for (DetailAST child = doStatement.getFirstChild(); child != null; child = child.getNextSibling()) {
            int tokenType = child.getType();

            if (tokenType == TokenTypes.SLIST
                    || tokenType == TokenTypes.LITERAL_IF
                    || tokenType == TokenTypes.LITERAL_FOR
                    || tokenType == TokenTypes.LITERAL_WHILE
                    || tokenType == TokenTypes.LITERAL_TRY
                    || tokenType == TokenTypes.LITERAL_SWITCH
                    || tokenType == TokenTypes.LITERAL_SYNCHRONIZED
                    || tokenType == TokenTypes.EXPR
                    || tokenType == TokenTypes.LITERAL_RETURN
                    || tokenType == TokenTypes.LITERAL_THROW
                    || tokenType == TokenTypes.LITERAL_BREAK
                    || tokenType == TokenTypes.LITERAL_CONTINUE
                    || tokenType == TokenTypes.VARIABLE_DEF
                    || tokenType == TokenTypes.EMPTY_STAT) {
                return child;
            }
        }

        return null;
    }
}
