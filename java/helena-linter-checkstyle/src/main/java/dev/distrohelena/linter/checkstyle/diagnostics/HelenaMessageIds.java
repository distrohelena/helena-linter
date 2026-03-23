package dev.distrohelena.linter.checkstyle.diagnostics;

/**
 * Holds reusable diagnostic message identifiers for the Helena Checkstyle package.
 */
public final class HelenaMessageIds {

    /**
     * Identifies diagnostics that describe two conditions as exact complements.
     */
    public static final String COMPLEMENTARY_CONDITION = "complementary-condition";

    /**
     * Identifies diagnostics that report a redundant complementary {@code else if} branch.
     */
    public static final String REDUNDANT_ELSE_IF = "redundant-else-if";

    /**
     * Identifies diagnostics that require a blank line before a target statement.
     */
    public static final String BLANK_LINE_BEFORE_STATEMENT = "blank-line-before-statement";

    /**
     * Identifies diagnostics that require a blank line after a target statement.
     */
    public static final String BLANK_LINE_AFTER_STATEMENT = "blank-line-after-statement";

    /**
     * Identifies diagnostics that describe a statement as a guaranteed control-flow exit.
     */
    public static final String CONTROL_FLOW_EXIT = "control-flow-exit";

    /**
     * Identifies diagnostics that require braces around an executable control body.
     */
    public static final String CONTROL_BODY_BRACES = "control-body-braces";

    /**
     * Identifies diagnostics that require a blank line after an {@code if} chain.
     */
    public static final String IF_FOLLOWING_SPACING = "if-following-spacing";

    /**
     * Identifies diagnostics that require sibling {@code if} statements to be folded into an {@code else if} chain.
     */
    public static final String IF_ELSE_IF_CHAIN = "if-else-if-chain";

    /**
     * Identifies diagnostics that require a blank line after a non-{@code if} control block.
     */
    public static final String CONTROL_BLOCK_FOLLOWING_SPACING = "control-block-following-spacing";

    /**
     * Identifies diagnostics that require a blank line after a local declaration.
     */
    public static final String DECLARATION_SPACING = "declaration-spacing";

    /**
     * Identifies diagnostics that require a blank line before a local declaration.
     */
    public static final String DECLARATION_LEADING_SPACING = "declaration-leading-spacing";

    /**
     * Identifies diagnostics that require a blank line before an exit statement.
     */
    public static final String EXIT_SPACING = "exit-spacing";

    /**
     * Identifies diagnostics that report a supported early-return guard-clause opportunity.
     */
    public static final String EARLY_RETURN = "early-return";

    /**
     * Identifies diagnostics that require a blank line before an {@code if} statement.
     */
    public static final String IF_LEADING_SPACING = "if-leading-spacing";

    /**
     * Prevents direct instantiation.
     */
    private HelenaMessageIds() {
    }
}
