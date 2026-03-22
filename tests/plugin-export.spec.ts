import { describe, expect, it } from "vitest";
import helenaLinter from "../src/index";

describe("helena-linter package export", () => {
    it("exposes a recommended config and named rules", () => {
        expect(Array.isArray(helenaLinter.configs.recommended)).toBe(true);
        expect(Object.keys(helenaLinter.rules).length).toBeGreaterThan(0);
    });

    it("uses @distrohelena/linter as the canonical ESLint plugin prefix", () => {
        const recommendedConfig = helenaLinter.configs.recommended[0];

        expect(recommendedConfig?.plugins).toHaveProperty(
            "@distrohelena/linter",
        );
        expect(recommendedConfig?.rules).toMatchObject({
            "@distrohelena/linter/control-block-following-spacing": "error",
            "@distrohelena/linter/declaration-leading-spacing": "error",
            "@distrohelena/linter/declaration-spacing": "error",
            "@distrohelena/linter/early-return": "error",
            "@distrohelena/linter/exit-spacing": "error",
            "@distrohelena/linter/if-else-if-chain": "error",
            "@distrohelena/linter/if-leading-spacing": "error",
            "@distrohelena/linter/if-following-spacing": "error",
            "@distrohelena/linter/redundant-else-if": "error",
        });
    });
});
