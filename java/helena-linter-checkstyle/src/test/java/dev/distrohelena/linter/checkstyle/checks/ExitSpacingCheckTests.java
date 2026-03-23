package dev.distrohelena.linter.checkstyle.checks;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

import com.puppycrawl.tools.checkstyle.api.Violation;
import java.util.List;
import java.util.SortedSet;
import org.junit.jupiter.api.Test;

/**
 * Verifies blank-line spacing before control-flow exit statements.
 */
class ExitSpacingCheckTests {

    /**
     * Confirms that return, throw, break, and continue statements require a blank line before them.
     *
     * @throws Exception when the sample cannot be read or parsed.
     */
    @Test
    void shouldRejectExitStatementsWithoutLeadingBlankLine() throws Exception {
        String source = CheckstyleCheckTestSupport.readResource("/samples/exit-spacing/invalid.java");
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new ExitSpacingCheck(), source);

        assertEquals(List.of(4, 9, 15, 22), CheckstyleCheckTestSupport.violationLines(violations));
    }

    /**
     * Confirms that blank lines before exit statements keep the code valid.
     *
     * @throws Exception when the sample cannot be read or parsed.
     */
    @Test
    void shouldAllowExitStatementsWithLeadingBlankLine() throws Exception {
        String source = CheckstyleCheckTestSupport.readResource("/samples/exit-spacing/valid.java");
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new ExitSpacingCheck(), source);

        assertTrue(violations.isEmpty(), () -> "Unexpected violations: " + CheckstyleCheckTestSupport.violationLines(violations));
    }
}
