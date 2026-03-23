package dev.distrohelena.linter.checkstyle.checks;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

import com.puppycrawl.tools.checkstyle.api.Violation;
import java.util.List;
import java.util.SortedSet;
import org.junit.jupiter.api.Test;

/**
 * Verifies redundant complementary {@code else if} branches.
 */
class RedundantElseIfCheckTests {

    /**
     * Confirms that a null-check complement is reported on the {@code else if} branch.
     *
     * @throws Exception when the sample cannot be read or parsed.
     */
    @Test
    void shouldRejectComplementaryNullElseIfBranch() throws Exception {
        String source = """
                class RedundantElseIfNullSample {
                    void test(Object value) {
                        if (value == null) {
                            work();
                        } else if (value != null) {
                            work();
                        }
                    }

                    void work() {
                    }
                }
                """;
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new RedundantElseIfCheck(), source);

        assertEquals(List.of(5), CheckstyleCheckTestSupport.violationLines(violations));
    }

    /**
     * Confirms that a boolean negation complement is reported on the {@code else if} branch.
     *
     * @throws Exception when the sample cannot be read or parsed.
     */
    @Test
    void shouldRejectComplementaryBooleanElseIfBranch() throws Exception {
        String source = """
                class RedundantElseIfBooleanSample {
                    void test(boolean flag) {
                        if (!flag) {
                            work();
                        } else if (flag) {
                            work();
                        }
                    }

                    void work() {
                    }
                }
                """;
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new RedundantElseIfCheck(), source);

        assertEquals(List.of(5), CheckstyleCheckTestSupport.violationLines(violations));
    }

    /**
     * Confirms that unrelated {@code else if} conditions are ignored.
     *
     * @throws Exception when the sample cannot be read or parsed.
     */
    @Test
    void shouldAllowNonComplementaryElseIfBranch() throws Exception {
        String source = """
                class RedundantElseIfNegativeSample {
                    void test(Object value, Object other) {
                        if (value == null) {
                            work();
                        } else if (other == null) {
                            work();
                        }
                    }

                    void work() {
                    }
                }
                """;
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new RedundantElseIfCheck(), source);

        assertTrue(violations.isEmpty(), () -> "Unexpected violations: " + CheckstyleCheckTestSupport.violationLines(violations));
    }
}
