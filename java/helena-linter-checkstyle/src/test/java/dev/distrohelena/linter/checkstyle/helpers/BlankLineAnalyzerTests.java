package dev.distrohelena.linter.checkstyle.helpers;

import static org.junit.jupiter.api.Assertions.assertSame;
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
 * Verifies blank-line analysis and statement navigation across Checkstyle AST siblings.
 */
class BlankLineAnalyzerTests {

    /**
     * Confirms that a blank line is detected even when a comment sits between the sibling statements.
     *
     * @throws Exception when the source cannot be parsed for the test fixture.
     */
    @Test
    void shouldDetectBlankLineBeforeStatementPastCommentAndSemicolonTrivia() throws Exception {
        FileContents fileContents = parseFileContents("""
                class Test {
                    void test() {
                        first();

                        // comment

                        second();
                    }
                }
                """);
        DetailAST methodBody = findFirstToken(parseCompilationUnit(fileContents), TokenTypes.METHOD_DEF)
                .findFirstToken(TokenTypes.SLIST);
        DetailAST firstStatement = findFirstStatementChild(methodBody);
        DetailAST secondStatement = StatementAstNavigator.getNextSibling(firstStatement);

        assertSame(firstStatement, StatementAstNavigator.getPreviousSibling(secondStatement));
        assertTrue(BlankLineAnalyzer.hasBlankLineBefore(secondStatement, fileContents));
    }

    /**
     * Parses the supplied Java source into Checkstyle file contents.
     *
     * @param source the Java source used for the test fixture.
     * @return the parsed file contents.
     * @throws CheckstyleException when the source cannot be parsed.
     */
    private static FileContents parseFileContents(String source) throws CheckstyleException {
        FileText fileText = new FileText(new File("Test.java"), Arrays.asList(source.split("\\R", -1)));
        return new FileContents(fileText);
    }

    /**
     * Parses the supplied file contents into a Checkstyle AST.
     *
     * @param fileContents the file contents to parse.
     * @return the parsed compilation unit AST.
     * @throws CheckstyleException when the source cannot be parsed.
     */
    private static DetailAST parseCompilationUnit(FileContents fileContents) throws CheckstyleException {
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
     * Finds the first direct child of a block that represents a real statement.
     *
     * @param block the block whose statement children should be searched.
     * @return the first statement child.
     */
    private static DetailAST findFirstStatementChild(DetailAST block) {
        for (DetailAST child = block.getFirstChild(); child != null; child = child.getNextSibling()) {
            if (child.getType() != TokenTypes.SEMI
                    && child.getType() != TokenTypes.RCURLY
                    && child.getType() != TokenTypes.LCURLY
                    && child.getType() != TokenTypes.SINGLE_LINE_COMMENT
                    && child.getType() != TokenTypes.BLOCK_COMMENT_BEGIN
                    && child.getType() != TokenTypes.BLOCK_COMMENT_END
                    && child.getType() != TokenTypes.COMMENT_CONTENT) {
                return child;
            }
        }

        return null;
    }
}
