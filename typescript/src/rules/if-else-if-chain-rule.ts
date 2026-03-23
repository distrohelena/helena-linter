import ts from "typescript";
import { ESLintUtils } from "@typescript-eslint/utils";
import { doesStatementAlwaysExit } from "../utils/control-flow-exit.js";
import { getSpacingRangeBetweenStatements } from "../utils/line-spacing.js";
import { createReportLoc } from "../utils/report-loc.js";
import {
    tryGetNullishComparisonDetails,
} from "../utils/expression-comparison.js";

const createRule = ESLintUtils.RuleCreator(
    (ruleName) =>
        `https://github.com/distrohelena/linter/docs/rules/${ruleName}`,
);

/**
 * Enforces `if / else if` chains for mutually exclusive branches.
 */
export const ifElseIfChainRule = createRule({
    name: "if-else-if-chain",
    meta: {
        type: "suggestion",
        fixable: "code",
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

                        reportSiblingViolation(
                            statement,
                            index + 1 < statements.length
                                ? statements[index + 1]
                                : undefined,
                        );
                    }
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

                    if (
                        currentComparison === undefined ||
                        nextComparison === undefined
                    ) {
                        if (!doesStatementAlwaysExit(statement.thenStatement)) {
                            return;
                        }
                    } else if (
                        currentComparison.targetText !==
                        nextComparison.targetText
                    ) {
                        return;
                    } else if (
                        currentComparison.nullishKind ===
                        nextComparison.nullishKind
                    ) {
                        return;
                    }

                    const position = sourceFile.getLineAndCharacterOfPosition(
                        nextStatement.getStart(sourceFile),
                    );
                    context.report({
                        loc: createReportLoc(
                            position.line + 1,
                            position.character + 1,
                        ),
                        messageId: "ifElseIfChain",
                        fix(fixer) {
                            return fixer.replaceTextRange(
                                getSpacingRangeBetweenStatements(
                                    statement,
                                    nextStatement,
                                    sourceFile,
                                ),
                                " else ",
                            );
                        },
                    });
                }
            },
        };
    },
});
