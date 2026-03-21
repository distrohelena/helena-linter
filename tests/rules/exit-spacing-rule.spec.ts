import { exitSpacingRule } from "../../src/rules/exit-spacing-rule";
import { createRuleTester } from "../helpers/rule-tester";

createRuleTester().run("exit-spacing", exitSpacingRule, {
    valid: [
        {
            code: [
                "class Sample {",
                "    test(flag: boolean): string {",
                "        if (!flag) {",
                "            return 'no';",
                "        }",
                "",
                "        return 'yes';",
                "    }",
                "}",
            ].join("\n"),
        },
        {
            code: [
                "class Sample {",
                "    test(items: string[]): string {",
                "        for (const item of items) {",
                "            if (item === 'stop') {",
                "                break;",
                "            }",
                "        }",
                "",
                "        return 'done';",
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
                "        const value = flag ? 'yes' : 'no';",
                "        return value;",
                "    }",
                "}",
            ].join("\n"),
            output: [
                "class Sample {",
                "    test(flag: boolean): string {",
                "        const value = flag ? 'yes' : 'no';",
                "",
                "        return value;",
                "    }",
                "}",
            ].join("\n"),
            errors: [{ messageId: "exitSpacing" }],
        },
        {
            code: [
                "class Sample {",
                "    test(items: string[]): string {",
                "        for (const item of items) {",
                "            if (item === 'stop') {",
                "                console.log(item);",
                "                break;",
                "            }",
                "        }",
                "",
                "        return 'done';",
                "    }",
                "}",
            ].join("\n"),
            output: [
                "class Sample {",
                "    test(items: string[]): string {",
                "        for (const item of items) {",
                "            if (item === 'stop') {",
                "                console.log(item);",
                "",
                "                break;",
                "            }",
                "        }",
                "",
                "        return 'done';",
                "    }",
                "}",
            ].join("\n"),
            errors: [{ messageId: "exitSpacing" }],
        },
    ],
});
