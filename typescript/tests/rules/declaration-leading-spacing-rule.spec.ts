import { declarationLeadingSpacingRule } from "../../src/rules/declaration-leading-spacing-rule";
import { createRuleTester } from "../helpers/rule-tester";

createRuleTester().run(
    "declaration-leading-spacing",
    declarationLeadingSpacingRule,
    {
        valid: [
            {
                code: [
                    "class Sample {",
                    "    test(flag: boolean): string {",
                    "        if (!flag) {",
                    "            return 'no';",
                    "        }",
                    "",
                    "        const value = 'yes';",
                    "",
                    "        return value;",
                    "    }",
                    "}",
                ].join("\n"),
            },
            {
                code: [
                    "class Sample {",
                    "    test(): string[] {",
                    "        const first = 'a';",
                    "",
                    "        const second = 'b';",
                    "",
                    "        return [first, second];",
                    "    }",
                    "}",
                ].join("\n"),
            },
        ],
        invalid: [
            {
                code: [
                    "class Sample {",
                    "    test(flag: boolean): string {",
                    "        if (!flag) {",
                    "            return 'no';",
                    "        }",
                    "        const value = 'yes';",
                    "",
                    "        return value;",
                    "    }",
                    "}",
                ].join("\n"),
                output: [
                    "class Sample {",
                    "    test(flag: boolean): string {",
                    "        if (!flag) {",
                    "            return 'no';",
                    "        }",
                    "",
                    "        const value = 'yes';",
                    "",
                    "        return value;",
                    "    }",
                    "}",
                ].join("\n"),
                errors: [{ messageId: "declarationLeadingSpacing" }],
            },
            {
                code: [
                    "class Sample {",
                    "    test(flag: boolean): string {",
                    "        console.log(flag); const value = 'yes';",
                    "",
                    "        return value;",
                    "    }",
                    "}",
                ].join("\n"),
                output: [
                    "class Sample {",
                    "    test(flag: boolean): string {",
                    "        console.log(flag);",
                    "",
                    "        const value = 'yes';",
                    "",
                    "        return value;",
                    "    }",
                    "}",
                ].join("\n"),
                errors: [{ messageId: "declarationLeadingSpacing" }],
            },
        ],
    },
);
