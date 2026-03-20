import { RuleTester } from "@typescript-eslint/rule-tester";
import { afterAll, describe, it } from "vitest";

RuleTester.afterAll = afterAll;
RuleTester.describe = describe;
RuleTester.it = it;

/**
 * Creates a shared RuleTester instance for Helena lint rules.
 * @returns Configured rule tester.
 */
export function createRuleTester(): RuleTester {
    return new RuleTester({
        languageOptions: {
            parserOptions: {
                ecmaVersion: 2022,
                sourceType: "module",
            },
        },
    });
}
