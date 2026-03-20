import { ifElseIfChainRule } from "../../src/rules/if-else-if-chain-rule";
import { createRuleTester } from "../helpers/rule-tester";

createRuleTester().run("if-else-if-chain", ifElseIfChainRule, {
  valid: [
    {
      code: [
        "class Sample {",
        "    test(value?: string): string {",
        "        if (value === null) {",
        "            return 'null';",
        "        } else if (value === undefined) {",
        "            return 'undefined';",
        "        }",
        "        return 'ok';",
        "    }",
        "}",
      ].join("\n"),
    },
    {
      code: [
        "class Sample {",
        "    test(value: string, other: boolean): string {",
        "        if (!other) {",
        "            return 'first';",
        "        }",
        "",
        "        return value === 'main' ? 'main' : 'other';",
        "    }",
        "}",
      ].join("\n"),
    },
  ],
  invalid: [
    {
      code: [
        "class Sample {",
        "    test(value?: string): string {",
        "        if (value == null) {",
        "            return 'missing';",
        "        }",
        "        return 'ok';",
        "    }",
        "}",
      ].join("\n"),
      errors: [{ messageId: "ifElseIfChain" }],
    },
    {
      code: [
        "class Sample {",
        "    test(value?: string): string {",
        "        if (value === undefined || value === null) {",
        "            return 'missing';",
        "        }",
        "        return 'ok';",
        "    }",
        "}",
      ].join("\n"),
      errors: [{ messageId: "ifElseIfChain" }],
    },
    {
      code: [
        "class Sample {",
        "    test(value?: string): string {",
        "        if (value === null) {",
        "            return 'null';",
        "        }",
        "        if (value === undefined) {",
        "            return 'undefined';",
        "        }",
        "        return 'ok';",
        "    }",
        "}",
      ].join("\n"),
      errors: [{ messageId: "ifElseIfChain" }],
    },
  ],
});
