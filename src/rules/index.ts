import { declarationSpacingRule } from "./declaration-spacing-rule.js";
import { earlyReturnRule } from "./early-return-rule.js";
import { ifElseIfChainRule } from "./if-else-if-chain-rule.js";
import { ifFollowingSpacingRule } from "./if-following-spacing-rule.js";
import { redundantElseIfRule } from "./redundant-else-if-rule.js";

/**
 * Collects all public Helena linter rules.
 */
export const helenaRules = {
    "declaration-spacing": declarationSpacingRule,
    "early-return": earlyReturnRule,
    "if-else-if-chain": ifElseIfChainRule,
    "if-following-spacing": ifFollowingSpacingRule,
    "redundant-else-if": redundantElseIfRule,
};
