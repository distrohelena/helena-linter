import { multilineBlockLayoutRule } from "../../src/rules/multiline-block-layout-rule";
import { createRuleTester } from "../helpers/rule-tester";

createRuleTester().run("multiline-block-layout", multilineBlockLayoutRule, {
    valid: [
        {
            code: [
                "class Sample {",
                "    run(flag: boolean): number {",
                "        if (flag) {",
                "            return 1;",
                "        }",
                "",
                "        try {",
                "            return 2;",
                "        } catch {}",
                "    }",
                "}",
            ].join("\n"),
        },
        {
            code: [
                "function run(): void {}",
                "",
                "try {} catch {} finally {}",
            ].join("\n"),
        },
    ],
    invalid: [
        {
            code: [
                "class Sample {",
                "    run(flag: boolean): number {",
                "        if (flag) { return 1; }",
                "        try { return 2; } catch {}",
                "    }",
                "}",
            ].join("\n"),
            output: [
                "class Sample {",
                "    run(flag: boolean): number {",
                "        if (flag) {",
                "            return 1;",
                "        }",
                "        try {",
                "            return 2;",
                "        } catch {}",
                "    }",
                "}",
            ].join("\n"),
            errors: [
                { messageId: "multilineBlockLayout" },
                { messageId: "multilineBlockLayout" },
            ],
        },
        {
            code: [
                "function run(): number { return 1; }",
            ].join("\n"),
            output: [
                "function run(): number {",
                "    return 1;",
                "}",
            ].join("\n"),
            errors: [{ messageId: "multilineBlockLayout" }],
        },
    ],
});
