import ts from "typescript";
import { ESLintUtils } from "@typescript-eslint/utils";
import { tryGetComplementComparison } from "../utils/expression-comparison.js";
import { createReportLoc } from "../utils/report-loc.js";

const createRule = ESLintUtils.RuleCreator(
    (ruleName) =>
        `https://github.com/distrohelena/linter/docs/rules/${ruleName}`,
);

/**
 * Enforces plain else for complementary else-if branches.
 */
export const redundantElseIfRule = createRule({
    name: "redundant-else-if",
    meta: {
        type: "suggestion",
        fixable: "code",
        docs: {
            description:
                "Require plain else when an else-if condition is the exact complement of the previous if.",
        },
        messages: {
            redundantElseIf:
                "Use a plain `else` for this complementary branch instead of `else if`.",
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
                 * Recursively walks the syntax tree and inspects if / else if chains.
                 * @param node Current syntax node.
                 */
                function visitNode(node: ts.Node): void {
                    if (ts.isIfStatement(node)) {
                        reportViolation(node);
                    }

                    ts.forEachChild(node, visitNode);
                }

                /**
                 * Reports a redundant else-if branch when detected.
                 * @param statement Statement to inspect.
                 */
                function reportViolation(statement: ts.IfStatement): void {
                    const elseIfStatement = statement.elseStatement;

                    if (
                        elseIfStatement === undefined ||
                        !ts.isIfStatement(elseIfStatement)
                    ) {
                        return;
                    }

                    const comparison = tryGetComplementComparison(
                        statement.expression,
                        elseIfStatement.expression,
                    );
                    if (comparison === undefined) {
                        return;
                    }

                    const position = sourceFile.getLineAndCharacterOfPosition(
                        elseIfStatement.expression.getStart(sourceFile),
                    );

                    context.report({
                        loc: createReportLoc(
                            position.line + 1,
                            position.character + 1,
                        ),
                        messageId: "redundantElseIf",
                        data: {
                            targetText: comparison.targetText,
                        },
                        fix(fixer) {
                            return fixer.replaceTextRange(
                                [
                                    elseIfStatement.getStart(sourceFile),
                                    elseIfStatement.thenStatement.getStart(
                                        sourceFile,
                                    ),
                                ],
                                "",
                            );
                        },
                    });
                }
            },
        };
    },
});
