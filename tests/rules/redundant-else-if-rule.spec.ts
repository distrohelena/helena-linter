import { redundantElseIfRule } from "../../src/rules/redundant-else-if-rule";
import { createRuleTester } from "../helpers/rule-tester";

createRuleTester().run("redundant-else-if", redundantElseIfRule, {
    valid: [
        {
            code: [
                "class Sample {",
                "    test(value: string, other: boolean): string {",
                "        if (!other) {",
                "            return 'first';",
                "        } else if (value === 'main') {",
                "            return 'second';",
                "        }",
                "        return 'third';",
                "    }",
                "}",
            ].join("\n"),
        },
    ],
    invalid: [
        {
            code: [
                "class Sample {",
                "    test(pluginKey?: string): void {",
                "        if (!pluginKey) {",
                "            return;",
                "        } else if (pluginKey) {",
                "            console.log(pluginKey);",
                "        }",
                "    }",
                "}",
            ].join("\n"),
            output: [
                "class Sample {",
                "    test(pluginKey?: string): void {",
                "        if (!pluginKey) {",
                "            return;",
                "        } else {",
                "            console.log(pluginKey);",
                "        }",
                "    }",
                "}",
            ].join("\n"),
            errors: [{ messageId: "redundantElseIf" }],
        },
        {
            code: [
                "class Sample {",
                "    test(value: string): string {",
                "        if (value === 'main') {",
                "            return 'main';",
                "        } else if (value !== 'main') {",
                "            return 'other';",
                "        }",
                "        return value;",
                "    }",
                "}",
            ].join("\n"),
            output: [
                "class Sample {",
                "    test(value: string): string {",
                "        if (value === 'main') {",
                "            return 'main';",
                "        } else {",
                "            return 'other';",
                "        }",
                "        return value;",
                "    }",
                "}",
            ].join("\n"),
            errors: [{ messageId: "redundantElseIf" }],
        },
    ],
});
