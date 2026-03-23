package dev.distrohelena.linter.checkstyle.checks;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

import com.puppycrawl.tools.checkstyle.api.Violation;
import java.util.List;
import java.util.SortedSet;
import org.junit.jupiter.api.Test;

/**
 * Verifies blank-line spacing after non-{@code if} control blocks.
 */
class ControlBlockFollowingSpacingCheckTests {

    /**
     * Confirms that blank lines after {@code for}, {@code while}, {@code do}, {@code switch}, and
     * {@code try} statements are accepted.
     *
     * @throws Exception when the sample cannot be read or parsed.
     */
    @Test
    void shouldAllowBlankLineAfterControlBlocks() throws Exception {
        String source = CheckstyleCheckTestSupport.readResource("/samples/following-spacing/control-block-valid.java");
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new ControlBlockFollowingSpacingCheck(), source);

        assertTrue(violations.isEmpty(), () -> "Unexpected violations: " + CheckstyleCheckTestSupport.violationLines(violations));
    }

    /**
     * Confirms that completed control blocks without a separating blank line are reported.
     *
     * @throws Exception when the sample cannot be read or parsed.
     */
    @Test
    void shouldRejectControlBlocksWithoutFollowingBlankLine() throws Exception {
        String source = CheckstyleCheckTestSupport.readResource("/samples/following-spacing/control-block-invalid.java");
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new ControlBlockFollowingSpacingCheck(), source);

        assertEquals(List.of(3, 6, 9, 12, 19), CheckstyleCheckTestSupport.violationLines(violations));
    }
}
