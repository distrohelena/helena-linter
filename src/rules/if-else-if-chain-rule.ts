import ts from "typescript";
import { ESLintUtils } from "@typescript-eslint/utils";
import { doesStatementAlwaysExit } from "../utils/control-flow-exit.js";
import { createReportLoc } from "../utils/report-loc.js";
import {
  tryGetNullishComparisonDetails,
  unwrapExpression,
} from "../utils/expression-comparison.js";

const createRule = ESLintUtils.RuleCreator(
  (ruleName) => `https://github.com/distrohelena/linter/docs/rules/${ruleName}`,
);

/**
 * Enforces `if / else if` chains for mutually exclusive branches.
 */
export const ifElseIfChainRule = createRule({
  name: "if-else-if-chain",
  meta: {
    type: "suggestion",
    docs: {
      description:
        "Require else-if chains when sibling if statements are mutually exclusive.",
    },
    messages: {
      ifElseIfChain: "Use `else if` for this mutually exclusive branch.",
    },
    schema: [],
  },
  defaultOptions: [],
  create(context) {
    return {
      Program(program): void {
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
         * Inspects sibling statements for invalid null-check chain patterns.
         * @param statements Statement collection to inspect.
         */
        function inspectStatements(
          statements: ts.NodeArray<ts.Statement>,
        ): void {
          for (let index = 0; index < statements.length; index += 1) {
            const statement = statements[index];

            if (!ts.isIfStatement(statement)) {
              continue;
            }

            reportCollapsedViolation(statement);
            reportSiblingViolation(
              statement,
              index + 1 < statements.length ? statements[index + 1] : undefined,
            );
          }
        }

        /**
         * Reports collapsed null-check violations.
         * @param statement Statement to inspect.
         */
        function reportCollapsedViolation(statement: ts.IfStatement): void {
          const condition = unwrapExpression(statement.expression);
          const position = sourceFile.getLineAndCharacterOfPosition(
            statement.expression.getStart(sourceFile),
          );
          const isLooseNullCheck =
            ts.isBinaryExpression(condition) &&
            condition.operatorToken.kind === ts.SyntaxKind.EqualsEqualsToken &&
            tryGetNullishComparisonDetails(condition, sourceFile)
              ?.nullishKind === "null";

          if (isLooseNullCheck) {
            context.report({
              loc: createReportLoc(position.line + 1, position.character + 1),
              messageId: "ifElseIfChain",
            });
            return;
          } else if (
            !ts.isBinaryExpression(condition) ||
            condition.operatorToken.kind !== ts.SyntaxKind.BarBarToken
          ) {
            return;
          }

          const leftComparison = tryGetNullishComparisonDetails(
            condition.left,
            sourceFile,
          );
          const rightComparison = tryGetNullishComparisonDetails(
            condition.right,
            sourceFile,
          );

          if (leftComparison === undefined || rightComparison === undefined) {
            return;
          } else if (leftComparison.targetText !== rightComparison.targetText) {
            return;
          } else if (
            leftComparison.nullishKind === rightComparison.nullishKind
          ) {
            return;
          }

          context.report({
            loc: createReportLoc(position.line + 1, position.character + 1),
            messageId: "ifElseIfChain",
          });
        }

        /**
         * Reports sibling if-statement violations.
         * @param statement Statement to inspect.
         * @param nextStatement Next sibling statement.
         */
        function reportSiblingViolation(
          statement: ts.IfStatement,
          nextStatement: ts.Statement | undefined,
        ): void {
          if (
            statement.elseStatement !== undefined ||
            nextStatement === undefined ||
            !ts.isIfStatement(nextStatement)
          ) {
            return;
          }

          const currentComparison = tryGetNullishComparisonDetails(
            statement.expression,
            sourceFile,
          );
          const nextComparison = tryGetNullishComparisonDetails(
            nextStatement.expression,
            sourceFile,
          );

          if (currentComparison === undefined || nextComparison === undefined) {
            if (!doesStatementAlwaysExit(statement.thenStatement)) {
              return;
            }
          } else if (
            currentComparison.targetText !== nextComparison.targetText
          ) {
            return;
          } else if (
            currentComparison.nullishKind === nextComparison.nullishKind
          ) {
            return;
          }

          const position = sourceFile.getLineAndCharacterOfPosition(
            nextStatement.getStart(sourceFile),
          );
          context.report({
            loc: createReportLoc(position.line + 1, position.character + 1),
            messageId: "ifElseIfChain",
          });
        }
      },
    };
  },
});
