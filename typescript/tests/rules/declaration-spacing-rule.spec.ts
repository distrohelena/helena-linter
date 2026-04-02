import { declarationSpacingRule } from "../../src/rules/declaration-spacing-rule";
import { createRuleTester } from "../helpers/rule-tester";

createRuleTester().run("declaration-spacing", declarationSpacingRule, {
    valid: [
        {
            code: [
                "class Sample {",
                "    test(): string[] {",
                "        const first = 'a';",
                "        const second = 'b';",
                "",
                "        return [first, second];",
                "    }",
                "}",
            ].join("\n"),
        },
        {
            code: [
                "class Sample {",
                "    test(flag: boolean): string {",
                "        const value = 'a';",
                "",
                "        if (!flag) {",
                "            return value;",
                "        }",
                "",
                "        return 'b';",
                "    }",
                "}",
            ].join("\n"),
        },
    ],
    invalid: [
        {
            code: [
                "class Sample {",
                "    test(): string[] {",
                "        const first = 'a';",
                "        const second = 'b';",
                "        return [first, second];",
                "    }",
                "}",
            ].join("\n"),
            output: [
                "class Sample {",
                "    test(): string[] {",
                "        const first = 'a';",
                "        const second = 'b';",
                "",
                "        return [first, second];",
                "    }",
                "}",
            ].join("\n"),
            errors: [{ messageId: "declarationSpacing" }],
        },
    ],
});
