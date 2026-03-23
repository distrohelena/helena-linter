package dev.distrohelena.linter.checkstyle.checks;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

import com.puppycrawl.tools.checkstyle.api.Violation;
import java.util.List;
import java.util.SortedSet;
import org.junit.jupiter.api.Test;

/**
 * Verifies blank-line spacing after local declarations.
 */
class DeclarationSpacingCheckTests {

    /**
     * Confirms that a declaration must be followed by a blank line before the next sibling statement.
     *
     * @throws Exception when the sample cannot be read or parsed.
     */
    @Test
    void shouldRejectDeclarationsWithoutFollowingBlankLine() throws Exception {
        String source = CheckstyleCheckTestSupport.readResource("/samples/declaration-spacing/invalid.java");
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new DeclarationSpacingCheck(), source);

        assertEquals(List.of(3, 4), CheckstyleCheckTestSupport.violationLines(violations));
    }

    /**
     * Confirms that blank lines after local declarations keep the block valid.
     *
     * @throws Exception when the sample cannot be read or parsed.
     */
    @Test
    void shouldAllowDeclarationsWithFollowingBlankLine() throws Exception {
        String source = CheckstyleCheckTestSupport.readResource("/samples/declaration-spacing/valid.java");
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new DeclarationSpacingCheck(), source);

        assertTrue(violations.isEmpty(), () -> "Unexpected violations: " + CheckstyleCheckTestSupport.violationLines(violations));
    }
}
