package dev.distrohelena.linter.checkstyle.helpers;

import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertTrue;

import com.puppycrawl.tools.checkstyle.JavaParser;
import com.puppycrawl.tools.checkstyle.api.CheckstyleException;
import com.puppycrawl.tools.checkstyle.api.DetailAST;
import com.puppycrawl.tools.checkstyle.api.FileContents;
import com.puppycrawl.tools.checkstyle.api.FileText;
import com.puppycrawl.tools.checkstyle.api.TokenTypes;
import java.io.File;
import java.lang.reflect.Method;
import java.util.Arrays;
import java.util.stream.Stream;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.Arguments;
import org.junit.jupiter.params.provider.MethodSource;
import org.junit.jupiter.api.Test;

/**
 * Verifies the exit-analysis helper used by the Java Checkstyle checks.
 */
class ControlFlowExitAnalyzerTests {

    /**
     * Confirms that statements which unconditionally exit the current flow are detected.
     *
     * @param source the Java source that contains the exit statement under test.
     * @param tokenType the token type that should be treated as an exit.
     * @throws Exception when the source cannot be parsed for the test fixture.
     */
    @ParameterizedTest
    @MethodSource("exitStatementSources")
    void shouldDetectExitStatements(String source, int tokenType) throws Exception {
        DetailAST statement = findFirstToken(parseCompilationUnit(source), tokenType);

        assertTrue(ControlFlowExitAnalyzer.doesStatementDefinitelyExit(statement));
    }

    /**
     * Confirms that an {@code if} statement without a guaranteed exit path remains non-exiting.
     *
     * @throws Exception when the source cannot be parsed for the test fixture.
     */
    @Test
    void shouldRejectNonExitingBranches() throws Exception {
        DetailAST ifStatement = findFirstToken(parseCompilationUnit("""
                class Test {
                    void test(boolean flag) {
                        if (flag) {
                            return;
                        }
                    }
                }
                """), TokenTypes.LITERAL_IF);

        assertFalse(ControlFlowExitAnalyzer.doesStatementDefinitelyExit(ifStatement));
    }

    /**
     * Confirms that a nested block with a conditional exit still counts as fallthrough when later
     * statements remain reachable.
     *
     * @throws Exception when the source cannot be parsed for the test fixture.
     */
    @Test
    void shouldRejectNestedBlockThatCanFallThroughAfterConditionalExit() throws Exception {
        DetailAST methodBody = findFirstToken(parseCompilationUnit("""
                class Test {
                    void test(boolean flag) {
                        {
                            if (flag) {
                                return;
                            }
                        }

                        work();
                    }
                }
                """), TokenTypes.METHOD_DEF).findFirstToken(TokenTypes.SLIST);

        assertFalse(ControlFlowExitAnalyzer.doesStatementDefinitelyExit(methodBody));
    }

    /**
     * Confirms that a normal expression statement branch remains visible to the resolver.
     *
     * @throws Exception when the source cannot be parsed for the test fixture or the private helper cannot be invoked.
     */
    @Test
    void shouldResolveExpressionStatementBranchWithoutSkippingIt() throws Exception {
        DetailAST ifStatement = findFirstToken(parseCompilationUnit("""
                class Test {
                    void test(boolean flag) {
                        if (flag) foo();
                        else return;
                    }
                }
                """), TokenTypes.LITERAL_IF);

        DetailAST thenBranch = invokeFindIfBranch(ifStatement);

        assertNotNull(thenBranch);
        assertEquals(TokenTypes.EXPR, thenBranch.getType(), "resolved branch type was " + thenBranch.getType());
    }

    /**
     * Supplies the exit-statement fixtures used by the parameterized test.
     *
     * @return the exit-statement scenarios and their token types.
     */
    private static Stream<Arguments> exitStatementSources() {
        return Stream.of(
                Arguments.of("""
                        class Test {
                            void test() {
                                return;
                            }
                        }
                        """, TokenTypes.LITERAL_RETURN),
                Arguments.of("""
                        class Test {
                            void test() {
                                throw new IllegalStateException();
                            }
                        }
                        """, TokenTypes.LITERAL_THROW),
                Arguments.of("""
                        class Test {
                            void test() {
                                while (true) {
                                    break;
                                }
                            }
                        }
                        """, TokenTypes.LITERAL_BREAK),
                Arguments.of("""
                        class Test {
                            void test() {
                                while (true) {
                                    continue;
                                }
                            }
                        }
                        """, TokenTypes.LITERAL_CONTINUE));
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

    /**
     * Invokes the private then-branch resolver on the analyzer under test.
     *
     * @param ifStatement the {@code if} node to inspect.
     * @return the resolved then-branch node.
     * @throws Exception when reflection fails.
     */
    private static DetailAST invokeFindIfBranch(DetailAST ifStatement) throws Exception {
        Method method = ControlFlowExitAnalyzer.class.getDeclaredMethod("findIfBranch", DetailAST.class);
        method.setAccessible(true);
        return (DetailAST) method.invoke(null, ifStatement);
    }
}
