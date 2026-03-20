import ts from "typescript";
import { ESLintUtils } from "@typescript-eslint/utils";
import { doesStatementAlwaysExit } from "../utils/control-flow-exit.js";
import { createReportLoc } from "../utils/report-loc.js";

const createRule = ESLintUtils.RuleCreator(
  (ruleName) => `https://github.com/distrohelena/linter/docs/rules/${ruleName}`,
);

/**
 * Enforces Helena early-return guard-clause style.
 */
export const earlyReturnRule = createRule({
  name: "early-return",
  meta: {
    type: "suggestion",
    docs: {
      description:
        "Prefer early returns and guard clauses over wrapping the happy path in an if block.",
    },
    messages: {
      earlyReturn:
        "Rewrite this wrapped happy-path branch as an early return guard clause.",
    },
    schema: [],
  },
  defaultOptions: [],
  create(context) {
    return {
      Program(): void {
        const sourceText = context.sourceCode.text;
        const sourceFile = ts.createSourceFile(
          "file.ts",
          sourceText,
          ts.ScriptTarget.Latest,
          true,
          ts.ScriptKind.TS,
        );

        visitNode(sourceFile);

        /**
         * Recursively walks the syntax tree and inspects statement containers.
         * @param node Current syntax node.
         */
        function visitNode(node: ts.Node): void {
          if (
            ts.isSourceFile(node) ||
            ts.isBlock(node) ||
            ts.isCaseClause(node) ||
            ts.isDefaultClause(node) ||
            ts.isModuleBlock(node)
          ) {
            inspectStatements(node.statements);
          }

          ts.forEachChild(node, visitNode);
        }

        /**
         * Inspects sibling statements for wrapped branches that should become guard clauses.
         * @param statements Statement collection to inspect.
         */
        function inspectStatements(
          statements: ts.NodeArray<ts.Statement>,
        ): void {
          for (let index = 0; index < statements.length - 1; index += 1) {
            const statement = statements[index];
            const nextStatement = statements[index + 1];

            if (
              !ts.isIfStatement(statement) ||
              statement.elseStatement !== undefined
            ) {
              continue;
            } else if (!ts.isBlock(statement.thenStatement)) {
              continue;
            } else if (statement.thenStatement.statements.length < 2) {
              continue;
            }

            const lastThenStatement = statement.thenStatement.statements.at(-1);
            if (lastThenStatement === undefined) {
              continue;
            } else if (!doesStatementAlwaysExit(lastThenStatement)) {
              continue;
            } else if (!doesStatementAlwaysExit(nextStatement)) {
              continue;
            }

            const position = sourceFile.getLineAndCharacterOfPosition(
              statement.getStart(sourceFile),
            );
            context.report({
              loc: createReportLoc(position.line + 1, position.character + 1),
              messageId: "earlyReturn",
            });
          }
        }
      },
    };
  },
});
