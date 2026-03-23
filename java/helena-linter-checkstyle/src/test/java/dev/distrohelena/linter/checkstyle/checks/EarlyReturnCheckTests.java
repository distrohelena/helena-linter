package dev.distrohelena.linter.checkstyle.checks;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

import com.puppycrawl.tools.checkstyle.api.Violation;
import java.util.List;
import java.util.SortedSet;
import java.util.stream.Stream;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.Arguments;
import org.junit.jupiter.params.provider.MethodSource;

/**
 * Verifies supported early-return guard-clause opportunities.
 */
class EarlyReturnCheckTests {

    /**
     * Confirms that a wrapped happy-path block followed by an exit statement is reported.
     *
     * @param source the source code under test.
     * @throws Exception when the sample cannot be read or parsed.
     */
    @ParameterizedTest
    @MethodSource("wrappedHappyPathSources")
    void shouldRejectWrappedHappyPathBlockFollowedByExit(String source) throws Exception {
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new EarlyReturnCheck(), source);

        assertEquals(List.of(3), CheckstyleCheckTestSupport.violationLines(violations));
    }

    /**
     * Confirms that an {@code if / else} statement is reported when the {@code if} branch exits.
     *
     * @throws Exception when the sample cannot be read or parsed.
     */
    @Test
    void shouldRejectIfElseWhenIfBranchExits() throws Exception {
        String source = """
                class EarlyReturnIfBranchSample {
                    void test(boolean flag) {
                        if (flag) {
                            return;
                        } else {
                            work();
                            work();
                        }
                    }

                    void work() {
                    }
                }
                """;
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new EarlyReturnCheck(), source);

        assertEquals(List.of(3), CheckstyleCheckTestSupport.violationLines(violations));
    }

    /**
     * Confirms that an {@code if / else} statement is reported when the {@code else} branch exits.
     *
     * @throws Exception when the sample cannot be read or parsed.
     */
    @Test
    void shouldRejectIfElseWhenElseBranchExits() throws Exception {
        String source = """
                class EarlyReturnElseBranchSample {
                    void test(boolean flag) {
                        if (flag) {
                            work();
                            work();
                        } else {
                            return;
                        }
                    }

                    void work() {
                    }
                }
                """;
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new EarlyReturnCheck(), source);

        assertEquals(List.of(3), CheckstyleCheckTestSupport.violationLines(violations));
    }

    /**
     * Confirms that an {@code if / else} statement is ignored when neither branch exits.
     *
     * @throws Exception when the sample cannot be read or parsed.
     */
    @Test
    void shouldAllowIfElseWhenNeitherBranchExits() throws Exception {
        String source = """
                class EarlyReturnNegativeSample {
                    void test(boolean flag, boolean otherFlag) {
                        if (flag) {
                            work();
                        } else {
                            work();
                        }
                    }

                    void work() {
                    }
                }
                """;
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new EarlyReturnCheck(), source);

        assertTrue(violations.isEmpty(), () -> "Unexpected violations: " + CheckstyleCheckTestSupport.violationLines(violations));
    }

    /**
     * Supplies the wrapped happy-path scenarios that should be reported.
     *
     * @return the source text for each wrapped happy-path scenario.
     */
    private static Stream<Arguments> wrappedHappyPathSources() {
        return Stream.of(
                Arguments.of("""
                        class EarlyReturnWrappedBlockSample {
                            void test(boolean flag) {
                                if (flag) {
                                    work();
                                    work();
                                }
                                return;
                            }

                            void work() {
                            }
                        }
                        """));
    }
}
