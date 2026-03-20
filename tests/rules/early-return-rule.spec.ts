import { earlyReturnRule } from "../../src/rules/early-return-rule";
import { createRuleTester } from "../helpers/rule-tester";

createRuleTester().run("early-return", earlyReturnRule, {
  valid: [
    {
      code: [
        "class Sample {",
        "    test(value: number): number {",
        "        if (value <= 0) {",
        "            return 0;",
        "        }",
        "",
        "        const adjusted = value + 1;",
        "",
        "        return adjusted;",
        "    }",
        "}",
      ].join("\n"),
    },
    {
      code: [
        "class Sample {",
        "    test(value: number): number {",
        "        if (value > 0) {",
        "            return value;",
        "        }",
        "",
        "        return 0;",
        "    }",
        "}",
      ].join("\n"),
    },
  ],
  invalid: [
    {
      code: [
        "class Sample {",
        "    test(value: number): number {",
        "        if (value > 0) {",
        "            const adjusted = value + 1;",
        "            return adjusted;",
        "        }",
        "",
        "        return 0;",
        "    }",
        "}",
      ].join("\n"),
      errors: [{ messageId: "earlyReturn" }],
    },
    {
      code: [
        "class Sample {",
        "    test(value: string | undefined): string {",
        "        if (value) {",
        "            const trimmed = value.trim();",
        "            return trimmed;",
        "        }",
        "",
        "        throw new Error('Missing value');",
        "    }",
        "}",
      ].join("\n"),
      errors: [{ messageId: "earlyReturn" }],
    },
  ],
});
