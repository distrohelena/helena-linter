import ts from "typescript";
import { ESLintUtils } from "@typescript-eslint/utils";
import {
  getLineDistanceBetweenStatements,
  getStatementStartLocation,
} from "../utils/line-spacing.js";
import { createReportLoc } from "../utils/report-loc.js";

const createRule = ESLintUtils.RuleCreator(
  (ruleName) => `https://github.com/distrohelena/linter/docs/rules/${ruleName}`,
);

/**
 * Enforces spacing after completed if statements.
 */
export const ifFollowingSpacingRule = createRule({
  name: "if-following-spacing",
  meta: {
    type: "layout",
    docs: {
      description:
        "Require a blank line after a completed if statement before the next sibling statement.",
    },
    messages: {
      ifFollowingSpacing:
        "Insert a blank line after a completed `if` statement before the next sibling statement.",
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
         * Inspects sibling statements for missing blank lines after if statements.
         * @param statements Statement collection to inspect.
         */
        function inspectStatements(
          statements: ts.NodeArray<ts.Statement>,
        ): void {
          for (let index = 0; index < statements.length - 1; index += 1) {
            const statement = statements[index];

            if (!ts.isIfStatement(statement)) {
              continue;
            }

            const nextStatement = statements[index + 1];
            if (
              getLineDistanceBetweenStatements(
                statement,
                nextStatement,
                sourceFile,
              ) >= 2
            ) {
              continue;
            }

            const location = getStatementStartLocation(
              nextStatement,
              sourceFile,
            );
            context.report({
              loc: createReportLoc(location.line, location.column),
              messageId: "ifFollowingSpacing",
            });
          }
        }
      },
    };
  },
});
