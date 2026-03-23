import type { Linter } from "eslint";
import type { helenaRules } from "../rules/index.js";

/**
 * Describes the public Helena linter package export.
 */
export interface HelenaLinterPlugin {
    /**
     * Gets package metadata for ESLint.
     */
    meta: {
        /**
         * Gets the package name.
         */
        name: string;
    };
    /**
     * Gets the Helena rule registry.
     */
    rules: typeof helenaRules;
    /**
     * Gets packaged Helena flat configs.
     */
    configs: {
        /**
         * Gets the recommended Helena flat config.
         */
        recommended: Linter.Config[];
    };
}
