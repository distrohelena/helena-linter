package dev.distrohelena.linter.checkstyle.checks;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

import com.puppycrawl.tools.checkstyle.api.Violation;
import java.util.List;
import java.util.SortedSet;
import org.junit.jupiter.api.Test;

/**
 * Verifies blank-line spacing before top-level {@code if} statements.
 */
class IfLeadingSpacingCheckTests {

    /**
     * Confirms that an {@code if} statement following another sibling statement requires a blank line.
     *
     * @throws Exception when the sample cannot be read or parsed.
     */
    @Test
    void shouldRejectIfStatementWithoutLeadingBlankLine() throws Exception {
        String source = CheckstyleCheckTestSupport.readResource("/samples/if-leading-spacing/invalid.java");
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new IfLeadingSpacingCheck(), source);

        assertEquals(List.of(4, 11), CheckstyleCheckTestSupport.violationLines(violations));
    }

    /**
     * Confirms that a blank line before an {@code if} statement keeps the block valid.
     *
     * @throws Exception when the sample cannot be read or parsed.
     */
    @Test
    void shouldAllowIfStatementWithLeadingBlankLine() throws Exception {
        String source = CheckstyleCheckTestSupport.readResource("/samples/if-leading-spacing/valid.java");
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new IfLeadingSpacingCheck(), source);

        assertTrue(violations.isEmpty(), () -> "Unexpected violations: " + CheckstyleCheckTestSupport.violationLines(violations));
    }
}
