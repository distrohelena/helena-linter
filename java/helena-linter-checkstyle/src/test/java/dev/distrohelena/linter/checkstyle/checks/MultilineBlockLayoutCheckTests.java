package dev.distrohelena.linter.checkstyle.checks;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

import com.puppycrawl.tools.checkstyle.api.Violation;
import java.util.List;
import java.util.SortedSet;
import org.junit.jupiter.api.Test;

/**
 * Verifies multiline layout for non-empty Java code blocks.
 */
class MultilineBlockLayoutCheckTests {

    /**
     * Confirms that single-line non-empty blocks are reported.
     *
     * @throws Exception when the sample cannot be read or parsed.
     */
    @Test
    void shouldRejectSingleLineNonEmptyBlocks() throws Exception {
        String source = CheckstyleCheckTestSupport.readResource("/samples/multiline-block-layout/invalid.java");
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new MultilineBlockLayoutCheck(), source);

        assertEquals(List.of(3, 6, 9), CheckstyleCheckTestSupport.violationLines(violations));
    }

    /**
     * Confirms that multiline blocks, empty blocks, and initializer braces remain valid.
     *
     * @throws Exception when the sample cannot be read or parsed.
     */
    @Test
    void shouldAllowMultilineAndEmptyBlocks() throws Exception {
        String source = CheckstyleCheckTestSupport.readResource("/samples/multiline-block-layout/valid.java");
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new MultilineBlockLayoutCheck(), source);

        assertTrue(violations.isEmpty(), () -> "Unexpected violations: " + CheckstyleCheckTestSupport.violationLines(violations));
    }
}
