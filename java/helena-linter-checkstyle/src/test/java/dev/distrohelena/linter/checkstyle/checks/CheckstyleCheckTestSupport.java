package dev.distrohelena.linter.checkstyle.checks;

import com.puppycrawl.tools.checkstyle.JavaParser;
import com.puppycrawl.tools.checkstyle.DefaultConfiguration;
import com.puppycrawl.tools.checkstyle.api.AbstractCheck;
import com.puppycrawl.tools.checkstyle.api.CheckstyleException;
import com.puppycrawl.tools.checkstyle.api.DetailAST;
import com.puppycrawl.tools.checkstyle.api.FileContents;
import com.puppycrawl.tools.checkstyle.api.FileText;
import com.puppycrawl.tools.checkstyle.api.Violation;
import java.io.File;
import java.io.IOException;
import java.net.URISyntaxException;
import java.net.URL;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.SortedSet;

/**
 * Provides shared Checkstyle test plumbing for the custom Helena checks.
 */
final class CheckstyleCheckTestSupport {

    /**
     * Prevents direct instantiation.
     */
    private CheckstyleCheckTestSupport() {
    }

    /**
     * Reads a UTF-8 sample resource into memory.
     *
     * @param resourcePath the classpath resource path to load.
     * @return the sample source text.
     * @throws IOException when the resource cannot be read.
     * @throws URISyntaxException when the resource URL cannot be converted to a path.
     */
    static String readResource(String resourcePath) throws IOException, URISyntaxException {
        URL resourceUrl = CheckstyleCheckTestSupport.class.getResource(resourcePath);

        if (resourceUrl == null) {
            throw new IOException("Missing test resource: " + resourcePath);
        }

        return Files.readString(Path.of(resourceUrl.toURI()));
    }

    /**
     * Executes a Checkstyle check against the supplied source text.
     *
     * @param check the check under test.
     * @param source the Java source to analyze.
     * @return the violations reported by the check.
     * @throws CheckstyleException when the source cannot be parsed.
     */
    static SortedSet<Violation> runCheck(AbstractCheck check, String source) throws CheckstyleException {
        FileText fileText = new FileText(new File("Test.java"), Arrays.asList(source.split("\\R", -1)));
        FileContents fileContents = new FileContents(fileText);
        DetailAST root = JavaParser.appendHiddenCommentNodes(JavaParser.parse(fileContents));

        check.configure(new DefaultConfiguration(check.getClass().getName()));
        check.setFileContents(fileContents);
        check.beginTree(root);
        walkTree(check, root);
        check.finishTree(root);

        return check.getViolations();
    }

    /**
     * Extracts the line numbers reported by a set of violations.
     *
     * @param violations the violations to inspect.
     * @return the reported line numbers in ascending order.
     */
    static List<Integer> violationLines(SortedSet<Violation> violations) {
        List<Integer> lines = new ArrayList<>();

        for (Violation violation : violations) {
            lines.add(violation.getLineNo());
        }

        return lines;
    }

    /**
     * Walks the AST and dispatches matching tokens to the check.
     *
     * @param check the check under test.
     * @param node the AST node to inspect.
     */
    private static void walkTree(AbstractCheck check, DetailAST node) {
        if (node == null) {
            return;
        }

        if (matchesDefaultToken(check, node.getType())) {
            check.visitToken(node);
        }

        for (DetailAST child = node.getFirstChild(); child != null; child = child.getNextSibling()) {
            walkTree(check, child);
        }

        if (matchesDefaultToken(check, node.getType())) {
            check.leaveToken(node);
        }
    }

    /**
     * Determines whether the supplied token type is handled by the check.
     *
     * @param check the check under test.
     * @param tokenType the token type to compare.
     * @return {@code true} when the token type is part of the check's default token set; otherwise {@code false}.
     */
    private static boolean matchesDefaultToken(AbstractCheck check, int tokenType) {
        for (int defaultToken : check.getDefaultTokens()) {
            if (defaultToken == tokenType) {
                return true;
            }
        }

        return false;
    }
}
