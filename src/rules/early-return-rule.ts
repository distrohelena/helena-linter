import ts from "typescript";
import { ESLintUtils } from "@typescript-eslint/utils";
import { doesStatementAlwaysExit } from "../utils/control-flow-exit.js";
import { getStatementIndentation } from "../utils/line-spacing.js";
import { createReportLoc } from "../utils/report-loc.js";

const createRule = ESLintUtils.RuleCreator(
    (ruleName) =>
        `https://github.com/distrohelena/linter/docs/rules/${ruleName}`,
);

/**
 * Enforces Helena early-return guard-clause style.
 */
export const earlyReturnRule = createRule({
    name: "early-return",
    meta: {
        type: "suggestion",
        fixable: "code",
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
                    for (
                        let index = 0;
                        index < statements.length - 1;
                        index += 1
                    ) {
                        const statement = statements[index];
                        const nextStatement = statements[index + 1];

                        if (
                            !ts.isIfStatement(statement) ||
                            statement.elseStatement !== undefined
                        ) {
                            continue;
                        } else if (!ts.isBlock(statement.thenStatement)) {
                            continue;
                        } else if (
                            statement.thenStatement.statements.length < 2
                        ) {
                            continue;
                        }

                        const lastThenStatement =
                            statement.thenStatement.statements.at(-1);
                        if (lastThenStatement === undefined) {
                            continue;
                        } else if (
                            !doesStatementAlwaysExit(lastThenStatement)
                        ) {
                            continue;
                        } else if (!doesStatementAlwaysExit(nextStatement)) {
                            continue;
                        }

                        const position =
                            sourceFile.getLineAndCharacterOfPosition(
                                statement.getStart(sourceFile),
                            );
                        context.report({
                            loc: createReportLoc(
                                position.line + 1,
                                position.character + 1,
                            ),
                            messageId: "earlyReturn",
                            fix(fixer) {
                                const replacement = buildEarlyReturnRewrite(
                                    statement,
                                    nextStatement,
                                );

                                return fixer.replaceTextRange(
                                    [
                                        statement.getStart(sourceFile),
                                        nextStatement.getEnd(),
                                    ],
                                    replacement,
                                );
                            },
                        });
                    }
                }

                /**
                 * Builds the rewritten guard-clause form for a supported early-return violation.
                 * @param statement Wrapped happy-path if statement.
                 * @param nextStatement Following terminating statement.
                 * @returns Rewritten guard-clause source text.
                 */
                function buildEarlyReturnRewrite(
                    statement: ts.IfStatement,
                    nextStatement: ts.Statement,
                ): string {
                    if (!ts.isBlock(statement.thenStatement)) {
                        return sourceText.slice(
                            statement.getStart(sourceFile),
                            nextStatement.getEnd(),
                        );
                    }

                    const newline = detectNewline(sourceText);
                    const statementIndentation = getStatementIndentation(
                        statement,
                        sourceFile,
                    );
                    const innerIndentation = `${statementIndentation}    `;
                    const thenStatements =
                        statement.thenStatement.statements.slice();
                    const firstThenStatement = thenStatements[0];
                    const lastThenStatement = thenStatements.at(-1);

                    if (
                        firstThenStatement === undefined ||
                        lastThenStatement === undefined
                    ) {
                        return sourceText.slice(
                            statement.getStart(sourceFile),
                            nextStatement.getEnd(),
                        );
                    }

                    const firstThenLineStart =
                        sourceFile.getPositionOfLineAndCharacter(
                            sourceFile.getLineAndCharacterOfPosition(
                                firstThenStatement.getStart(sourceFile),
                            ).line,
                            0,
                        );
                    const happyPathText = sourceText.slice(
                        firstThenLineStart,
                        lastThenStatement.getEnd(),
                    );
                    const guardStatementText = sourceText.slice(
                        nextStatement.getStart(sourceFile),
                        nextStatement.getEnd(),
                    );

                    return [
                        `if (${negateCondition(statement.expression)}) {`,
                        indentBlock(guardStatementText, innerIndentation),
                        `${statementIndentation}}`,
                        "",
                        dedentBlock(happyPathText, statementIndentation),
                    ].join(newline);
                }
            },
        };
    },
});

/**
 * Detects the newline sequence used by the current source text.
 * @param sourceText Current source text.
 * @returns Preferred newline sequence.
 */
function detectNewline(sourceText: string): string {
    return sourceText.includes("\r\n") ? "\r\n" : "\n";
}

/**
 * Negates a condition expression while preferring readable operator inversions.
 * @param expression Expression to negate.
 * @returns Negated condition text.
 */
function negateCondition(expression: ts.Expression): string {
    const currentExpression = unwrapParentheses(expression);

    if (
        ts.isPrefixUnaryExpression(currentExpression) &&
        currentExpression.operator === ts.SyntaxKind.ExclamationToken
    ) {
        return currentExpression.operand.getText();
    } else if (ts.isBinaryExpression(currentExpression)) {
        const invertedOperator = invertBinaryOperator(
            currentExpression.operatorToken.kind,
        );

        if (invertedOperator !== undefined) {
            return `${currentExpression.left.getText()} ${invertedOperator} ${currentExpression.right.getText()}`;
        }
    }

    const currentText = currentExpression.getText();
    if (
        ts.isIdentifier(currentExpression) ||
        ts.isPropertyAccessExpression(currentExpression) ||
        ts.isElementAccessExpression(currentExpression) ||
        ts.isCallExpression(currentExpression)
    ) {
        return `!${currentText}`;
    }

    return `!(${currentText})`;
}

/**
 * Removes a single indentation level from each non-empty line in a moved block.
 * @param blockText Block text to normalize.
 * @param statementIndentation Destination statement indentation.
 * @returns Dedented block text.
 */
function dedentBlock(blockText: string, statementIndentation: string): string {
    return blockText
        .split(/\r?\n/)
        .map((line) => {
            if (line.startsWith(`${statementIndentation}    `)) {
                return line.slice(4);
            }

            return line;
        })
        .join("\n");
}

/**
 * Applies a target indentation to each non-empty line in a moved statement.
 * @param blockText Statement text to indent.
 * @param indentation Target indentation.
 * @returns Indented block text.
 */
function indentBlock(blockText: string, indentation: string): string {
    return blockText
        .split(/\r?\n/)
        .map((line) => (line.length === 0 ? line : `${indentation}${line}`))
        .join("\n");
}

/**
 * Inverts a supported binary operator for guard-clause rewrites.
 * @param operatorKind Binary operator to invert.
 * @returns Inverted operator text when supported.
 */
function invertBinaryOperator(operatorKind: ts.SyntaxKind): string | undefined {
    switch (operatorKind) {
        case ts.SyntaxKind.GreaterThanToken:
            return "<=";
        case ts.SyntaxKind.GreaterThanEqualsToken:
            return "<";
        case ts.SyntaxKind.LessThanToken:
            return ">=";
        case ts.SyntaxKind.LessThanEqualsToken:
            return ">";
        case ts.SyntaxKind.EqualsEqualsEqualsToken:
            return "!==";
        case ts.SyntaxKind.ExclamationEqualsEqualsToken:
            return "===";
        case ts.SyntaxKind.EqualsEqualsToken:
            return "!=";
        case ts.SyntaxKind.ExclamationEqualsToken:
            return "==";
        default:
            return undefined;
    }
}

/**
 * Removes outer parentheses so negation can inspect the real expression shape.
 * @param expression Expression to unwrap.
 * @returns Unwrapped expression.
 */
function unwrapParentheses(expression: ts.Expression): ts.Expression {
    let currentExpression = expression;

    while (ts.isParenthesizedExpression(currentExpression)) {
        currentExpression = currentExpression.expression;
    }

    return currentExpression;
}
