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
 * Enforces spacing before exit statements.
 */
export const exitSpacingRule = createRule({
    name: "exit-spacing",
    meta: {
        type: "layout",
        fixable: "whitespace",
        docs: {
            description:
                "Require a blank line before exit statements like return, throw, break, and continue.",
        },
        messages: {
            exitSpacing: "Insert a blank line before this exit statement.",
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
                 * Inspects sibling statements for missing blank lines before exit statements.
                 * @param statements Statement collection to inspect.
                 */
                function inspectStatements(
                    statements: ts.NodeArray<ts.Statement>,
                ): void {
                    for (let index = 1; index < statements.length; index += 1) {
                        const previousStatement = statements[index - 1];
                        const statement = statements[index];

                        if (!isSupportedExitStatement(statement)) {
                            continue;
                        }

                        if (
                            getLineDistanceBetweenStatements(
                                previousStatement,
                                statement,
                                sourceFile,
                            ) >= 2
                        ) {
                            continue;
                        }

                        const location = getStatementStartLocation(
                            statement,
                            sourceFile,
                        );
                        context.report({
                            loc: createReportLoc(
                                location.line,
                                location.column,
                            ),
                            messageId: "exitSpacing",
                            fix(fixer) {
                                const newline = detectNewline(sourceText);

                                return fixer.replaceTextRange(
                                    getSpacingRangeBetweenStatements(
                                        previousStatement,
                                        statement,
                                        sourceFile,
                                    ),
                                    `${newline}${newline}${getStatementIndentation(statement, sourceFile)}`,
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
 * Determines whether a statement is a supported exit statement.
 * @param statement Statement to inspect.
 * @returns Whether the statement should require leading spacing.
 */
function isSupportedExitStatement(statement: ts.Statement): boolean {
    return (
        ts.isReturnStatement(statement) ||
        ts.isThrowStatement(statement) ||
        ts.isBreakStatement(statement) ||
        ts.isContinueStatement(statement)
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
