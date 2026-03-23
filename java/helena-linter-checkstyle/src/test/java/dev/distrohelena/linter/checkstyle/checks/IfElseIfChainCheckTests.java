package dev.distrohelena.linter.checkstyle.checks;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

import com.puppycrawl.tools.checkstyle.api.Violation;
import java.util.List;
import java.util.SortedSet;
import java.util.stream.Stream;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.Arguments;
import org.junit.jupiter.params.provider.MethodSource;
import org.junit.jupiter.api.Test;

/**
 * Verifies sibling {@code if} statements that should be folded into an {@code else if} chain.
 */
class IfElseIfChainCheckTests {

    /**
     * Confirms that a sibling {@code if} after a definite exit is reported.
     *
     * @param source the source code under test.
     * @param expectedLine the line number of the reported {@code if}.
     * @throws Exception when the sample cannot be read or parsed.
     */
    @ParameterizedTest
    @MethodSource("exitingSiblingIfSources")
    void shouldRejectSiblingIfAfterDefiniteExit(String source, int expectedLine) throws Exception {
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new IfElseIfChainCheck(), source);

        assertEquals(List.of(expectedLine), CheckstyleCheckTestSupport.violationLines(violations));
    }

    /**
     * Confirms that a sibling {@code if} is ignored when the prior branch can still fall through.
     *
     * @throws Exception when the sample cannot be read or parsed.
     */
    @Test
    void shouldAllowSiblingIfWhenPreviousBranchDoesNotExit() throws Exception {
        String source = """
                class IfElseIfChainNegativeSample {
                    void test(boolean flag, boolean otherFlag) {
                        if (flag) {
                            work();
                        }
                        if (otherFlag) {
                            work();
                        }
                    }

                    void work() {
                    }
                }
                """;
        SortedSet<Violation> violations = CheckstyleCheckTestSupport.runCheck(new IfElseIfChainCheck(), source);

        assertTrue(violations.isEmpty(), () -> "Unexpected violations: " + CheckstyleCheckTestSupport.violationLines(violations));
    }

    /**
     * Supplies the sibling-{@code if} scenarios that should be reported.
     *
     * @return the source text and expected line number for each scenario.
     */
    private static Stream<Arguments> exitingSiblingIfSources() {
        return Stream.of(
                Arguments.of("""
                        class IfElseIfChainReturnSample {
                            void test(boolean flag, boolean otherFlag) {
                                if (flag) {
                                    return;
                                }
                                if (otherFlag) {
                                    work();
                                }
                            }

                            void work() {
                            }
                        }
                        """, 6),
                Arguments.of("""
                        class IfElseIfChainThrowSample {
                            void test(boolean flag, boolean otherFlag) {
                                if (flag) {
                                    throw new IllegalStateException();
                                }
                                if (otherFlag) {
                                    work();
                                }
                            }

                            void work() {
                            }
                        }
                        """, 6),
                Arguments.of("""
                        class IfElseIfChainBreakSample {
                            void test(boolean flag, boolean otherFlag) {
                                while (true) {
                                    if (flag) {
                                        break;
                                    }
                                    if (otherFlag) {
                                        work();
                                    }
                                }
                            }

                            void work() {
                            }
                        }
                        """, 7),
                Arguments.of("""
                        class IfElseIfChainContinueSample {
                            void test(boolean flag, boolean otherFlag) {
                                while (true) {
                                    if (flag) {
                                        continue;
                                    }
                                    if (otherFlag) {
                                        work();
                                    }
                                }
                            }

                            void work() {
                            }
                        }
                        """, 7));
    }
}
