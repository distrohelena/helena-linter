package dev.distrohelena.linter.checkstyle.helpers;

import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;

import com.puppycrawl.tools.checkstyle.JavaParser;
import com.puppycrawl.tools.checkstyle.api.CheckstyleException;
import com.puppycrawl.tools.checkstyle.api.DetailAST;
import com.puppycrawl.tools.checkstyle.api.FileContents;
import com.puppycrawl.tools.checkstyle.api.FileText;
import com.puppycrawl.tools.checkstyle.api.TokenTypes;
import java.io.File;
import java.util.Arrays;
import org.junit.jupiter.api.Test;

/**
 * Verifies the complementary-condition comparisons used by the Java Checkstyle checks.
 */
class ConditionComparisonAnalyzerTests {

    /**
     * Confirms that equality and inequality checks against {@code null} are treated as complements.
     *
     * @throws Exception when the source cannot be parsed for the test fixture.
     */
    @Test
    void shouldRecognizeComplementaryNullComparisons() throws Exception {
        DetailAST leftCondition = parseFirstIfCondition("""
                class Test {
                    void test(Object value) {
                        if (value == null) {
                        }
                    }
                }
                """);
        DetailAST rightCondition = parseFirstIfCondition("""
                class Test {
                    void test(Object value) {
                        if (value != null) {
                        }
                    }
                }
                """);

        assertTrue(ConditionComparisonAnalyzer.isComplementaryCondition(leftCondition, rightCondition));
        assertTrue(ConditionComparisonAnalyzer.isComplementaryCondition(rightCondition, leftCondition));
    }

    /**
     * Confirms that a boolean value and its logical negation are treated as complements.
     *
     * @throws Exception when the source cannot be parsed for the test fixture.
     */
    @Test
    void shouldRecognizeBooleanNegationComparisons() throws Exception {
        DetailAST negatedCondition = parseFirstIfCondition("""
                class Test {
                    void test(boolean flag) {
                        if (!flag) {
                        }
                    }
                }
                """);
        DetailAST positiveCondition = parseFirstIfCondition("""
                class Test {
                    void test(boolean flag) {
                        if (flag) {
                        }
                    }
                }
                """);

        assertTrue(ConditionComparisonAnalyzer.isComplementaryCondition(negatedCondition, positiveCondition));
        assertTrue(ConditionComparisonAnalyzer.isComplementaryCondition(positiveCondition, negatedCondition));
    }

    /**
     * Confirms that unrelated comparison expressions are not treated as complements.
     *
     * @throws Exception when the source cannot be parsed for the test fixture.
     */
    @Test
    void shouldRejectNonComplementaryConditions() throws Exception {
        DetailAST firstCondition = parseFirstIfCondition("""
                class Test {
                    void test(Object value, Object other) {
                        if (value == null) {
                        }
                    }
                }
                """);
        DetailAST secondCondition = parseFirstIfCondition("""
                class Test {
                    void test(Object value, Object other) {
                        if (other == null) {
                        }
                    }
                }
                """);

        assertFalse(ConditionComparisonAnalyzer.isComplementaryCondition(firstCondition, secondCondition));
    }

    /**
     * Parses the supplied Java source and returns the first {@code if} condition expression.
     *
     * @param source the Java source used for the test fixture.
     * @return the condition node beneath the first {@code if} statement in the file.
     * @throws CheckstyleException when the source cannot be parsed into a Checkstyle AST.
     */
    private static DetailAST parseFirstIfCondition(String source) throws CheckstyleException {
        DetailAST ifStatement = findFirstToken(parseCompilationUnit(source), TokenTypes.LITERAL_IF);
        DetailAST expression = ifStatement.findFirstToken(TokenTypes.EXPR);
        return expression.getFirstChild();
    }

    /**
     * Parses the supplied Java source into a Checkstyle AST.
     *
     * @param source the Java source used for the test fixture.
     * @return the parsed compilation unit AST.
     * @throws CheckstyleException when the source cannot be parsed.
     */
    private static DetailAST parseCompilationUnit(String source) throws CheckstyleException {
        FileText fileText = new FileText(new File("Test.java"), Arrays.asList(source.split("\\R", -1)));
        FileContents fileContents = new FileContents(fileText);
        return JavaParser.appendHiddenCommentNodes(JavaParser.parse(fileContents));
    }

    /**
     * Finds the first node with the requested token type beneath the supplied AST root.
     *
     * @param root the AST root to search.
     * @param type the token type to locate.
     * @return the first matching node.
     */
    private static DetailAST findFirstToken(DetailAST root, int type) {
        if (root.getType() == type) {
            return root;
        }

        for (DetailAST child = root.getFirstChild(); child != null; child = child.getNextSibling()) {
            DetailAST match = findFirstToken(child, type);

            if (match != null) {
                return match;
            }
        }

        return null;
    }
}
