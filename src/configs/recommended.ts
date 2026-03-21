import type { Linter } from "eslint";
import tsParser from "@typescript-eslint/parser";
import type { HelenaLinterPlugin } from "../types/plugin-export.js";

/**
 * Builds the recommended Helena linter flat config.
 * @param plugin Plugin instance to register with ESLint.
 * @returns Recommended flat config entries.
 */
export function createRecommendedConfig(
    plugin: HelenaLinterPlugin,
): Linter.Config[] {
    return [
        {
            files: ["**/*.ts", "**/*.tsx", "**/*.mts", "**/*.cts"],
            languageOptions: {
                parser: tsParser,
                ecmaVersion: 2022,
                sourceType: "module",
            },
            plugins: {
                "@distrohelena/linter": plugin as unknown as NonNullable<
                    Linter.Config["plugins"]
                >[string],
            },
            rules: {
                "@distrohelena/linter/control-block-following-spacing": "error",
                "@distrohelena/linter/declaration-leading-spacing": "error",
                "@distrohelena/linter/declaration-spacing": "error",
                "@distrohelena/linter/early-return": "error",
                "@distrohelena/linter/exit-spacing": "error",
                "@distrohelena/linter/if-else-if-chain": "error",
                "@distrohelena/linter/if-following-spacing": "error",
                "@distrohelena/linter/redundant-else-if": "error",
            },
        },
    ];
}
