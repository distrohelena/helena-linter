import ts from "typescript";
import { ESLintUtils } from "@typescript-eslint/utils";
import {
    getLineDistanceBetweenStatements,
    getSpacingRangeBetweenStatements,
    getStatementIndentation,
    getStatementStartLocation,
} from "../utils/line-spacing.js";
import { createReportLoc } from "../utils/report-loc.js";

const createRule = ESLintUtils.RuleCreator(
    (ruleName) =>
        `https://github.com/distrohelena/linter/docs/rules/${ruleName}`,
);

/**
 * Enforces spacing after completed control-flow block statements.
 */
export const controlBlockFollowingSpacingRule = createRule({
    name: "control-block-following-spacing",
    meta: {
        type: "layout",
        fixable: "whitespace",
        docs: {
            description:
                "Require a blank line after a completed control-flow block before the next sibling statement.",
        },
        messages: {
            controlBlockFollowingSpacing:
                "Insert a blank line after a completed control-flow block before the next sibling statement.",
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
                 * Inspects sibling statements for missing blank lines after control blocks.
                 * @param statements Statement collection to inspect.
                 */
                function inspectStatements(
                    statements: ts.NodeArray<ts.Statement>,
                ): void {
                    for (
                        let index = 0;
                        index < statements.length - 1;
                        index += 1
                    ) {
                        const statement = statements[index];

                        if (!isSupportedControlBlockStatement(statement)) {
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
                            loc: createReportLoc(
                                location.line,
                                location.column,
                            ),
                            messageId: "controlBlockFollowingSpacing",
                            fix(fixer) {
                                const newline = detectNewline(sourceText);

                                return fixer.replaceTextRange(
                                    getSpacingRangeBetweenStatements(
                                        statement,
                                        nextStatement,
                                        sourceFile,
                                    ),
                                    `${newline}${newline}${getStatementIndentation(nextStatement, sourceFile)}`,
                                );
                            },
                        });
                    }
                }
            },
        };
    },
});

/**
 * Determines whether a statement is a supported completed control-flow block.
 * @param statement Statement to inspect.
 * @returns Whether the statement should require following spacing.
 */
function isSupportedControlBlockStatement(statement: ts.Statement): boolean {
    return (
        ts.isForStatement(statement) ||
        ts.isForInStatement(statement) ||
        ts.isForOfStatement(statement) ||
        ts.isWhileStatement(statement) ||
        ts.isDoStatement(statement) ||
        ts.isSwitchStatement(statement) ||
        ts.isTryStatement(statement)
    );
}

/**
 * Detects the newline sequence used by the current source text.
 * @param sourceText Current source text.
 * @returns Preferred newline sequence.
 */
function detectNewline(sourceText: string): string {
    return sourceText.includes("\r\n") ? "\r\n" : "\n";
}
