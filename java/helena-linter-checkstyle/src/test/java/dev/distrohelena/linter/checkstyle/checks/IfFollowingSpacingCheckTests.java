package dev.distrohelena.linter.checkstyle.checks;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

import com.puppycrawl.tools.checkstyle.api.Violation;
import java.util.List;
import java.util.SortedSet;
import org.junit.jupiter.api.Test;

/**
 * Verifies blank-line spacing after completed {@code if} statement chains.
 */
class IfFollowingSpacingCheckTests {

    /**
     * Confirms that a blank line after an {@code if} chain prevents a violation.
     *
     * @throws Exception when the sample cannot be read or parsed.
     */
    @Test
    void shouldAllowBlankLineAfterIfChain() throws Exception {
        String source = CheckstyleCheckTestSupport.readResource("/samples/following-spacing/if-valid.java");
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new IfFollowingSpacingCheck(), source);

        assertTrue(violations.isEmpty(), () -> "Unexpected violations: " + CheckstyleCheckTestSupport.violationLines(violations));
    }

    /**
     * Confirms that a completed {@code if} chain without a separating blank line is reported.
     *
     * @throws Exception when the sample cannot be read or parsed.
     */
    @Test
    void shouldRejectIfChainWithoutFollowingBlankLine() throws Exception {
        String source = CheckstyleCheckTestSupport.readResource("/samples/following-spacing/if-invalid.java");
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new IfFollowingSpacingCheck(), source);

        assertEquals(List.of(3), CheckstyleCheckTestSupport.violationLines(violations));
    }
}
