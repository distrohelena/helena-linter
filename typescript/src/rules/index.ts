import { controlBodyBracesRule } from "./control-body-braces-rule.js";
import { controlBlockFollowingSpacingRule } from "./control-block-following-spacing-rule.js";
import { declarationLeadingSpacingRule } from "./declaration-leading-spacing-rule.js";
import { declarationSpacingRule } from "./declaration-spacing-rule.js";
import { earlyReturnRule } from "./early-return-rule.js";
import { exitSpacingRule } from "./exit-spacing-rule.js";
import { ifElseIfChainRule } from "./if-else-if-chain-rule.js";
import { ifFollowingSpacingRule } from "./if-following-spacing-rule.js";
import { ifLeadingSpacingRule } from "./if-leading-spacing-rule.js";
import { multilineBlockLayoutRule } from "./multiline-block-layout-rule.js";
import { redundantElseIfRule } from "./redundant-else-if-rule.js";

/**
 * Collects all public Helena linter rules.
 */
export const helenaRules = {
    "control-body-braces": controlBodyBracesRule,
    "control-block-following-spacing": controlBlockFollowingSpacingRule,
    "declaration-leading-spacing": declarationLeadingSpacingRule,
    "declaration-spacing": declarationSpacingRule,
    "early-return": earlyReturnRule,
    "exit-spacing": exitSpacingRule,
    "if-else-if-chain": ifElseIfChainRule,
    "if-following-spacing": ifFollowingSpacingRule,
    "if-leading-spacing": ifLeadingSpacingRule,
    "multiline-block-layout": multilineBlockLayoutRule,
    "redundant-else-if": redundantElseIfRule,
};
