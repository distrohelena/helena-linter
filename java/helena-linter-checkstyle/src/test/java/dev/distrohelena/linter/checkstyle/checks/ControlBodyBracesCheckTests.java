package dev.distrohelena.linter.checkstyle.checks;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

import com.puppycrawl.tools.checkstyle.api.Violation;
import java.util.List;
import java.util.SortedSet;
import org.junit.jupiter.api.Test;

/**
 * Verifies that control statements use braces for their executable bodies.
 */
class ControlBodyBracesCheckTests {

    /**
     * Confirms that braced control bodies, including {@code else if} chains and try-with-resources
     * blocks, are accepted.
     *
     * @throws Exception when the sample cannot be read or parsed.
     */
    @Test
    void shouldAllowBracedControlBodiesAndElseIfChains() throws Exception {
        String source = CheckstyleCheckTestSupport.readResource("/samples/control-body-braces/valid.java");
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new ControlBodyBracesCheck(), source);

        assertTrue(violations.isEmpty(), () -> "Unexpected violations: " + CheckstyleCheckTestSupport.violationLines(violations));
    }

    /**
     * Confirms that unbraced control bodies are reported, while the {@code else if} chaining syntax
     * itself remains valid.
     *
     * @throws Exception when the sample cannot be read or parsed.
     */
    @Test
    void shouldRejectUnbracedControlBodiesButAllowElseIfChains() throws Exception {
        String source = CheckstyleCheckTestSupport.readResource("/samples/control-body-braces/invalid.java");
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new ControlBodyBracesCheck(), source);

        assertEquals(List.of(3, 5, 7, 10, 13, 16, 19), CheckstyleCheckTestSupport.violationLines(violations));
    }
}
