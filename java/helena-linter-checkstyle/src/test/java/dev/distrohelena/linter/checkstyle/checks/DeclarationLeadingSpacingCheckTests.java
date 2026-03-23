package dev.distrohelena.linter.checkstyle.checks;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

import com.puppycrawl.tools.checkstyle.api.Violation;
import java.util.List;
import java.util.SortedSet;
import org.junit.jupiter.api.Test;

/**
 * Verifies blank-line spacing before local declarations.
 */
class DeclarationLeadingSpacingCheckTests {

    /**
     * Confirms that a declaration following a non-declaration sibling must be separated by a blank line.
     *
     * @throws Exception when the sample cannot be read or parsed.
     */
    @Test
    void shouldRejectDeclarationsWithoutLeadingBlankLineAfterStatements() throws Exception {
        String source = CheckstyleCheckTestSupport.readResource("/samples/declaration-leading-spacing/invalid.java");
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new DeclarationLeadingSpacingCheck(), source);

        assertEquals(List.of(4, 10), CheckstyleCheckTestSupport.violationLines(violations));
    }

    /**
     * Confirms that declarations with a leading blank line remain valid.
     *
     * @throws Exception when the sample cannot be read or parsed.
     */
    @Test
    void shouldAllowDeclarationsWithLeadingBlankLine() throws Exception {
        String source = CheckstyleCheckTestSupport.readResource("/samples/declaration-leading-spacing/valid.java");
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new DeclarationLeadingSpacingCheck(), source);

        assertTrue(violations.isEmpty(), () -> "Unexpected violations: " + CheckstyleCheckTestSupport.violationLines(violations));
    }
}
