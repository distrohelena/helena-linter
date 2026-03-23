import ts from "typescript";
import { ESLintUtils } from "@typescript-eslint/utils";
import { createReportLoc } from "../utils/report-loc.js";

const createRule = ESLintUtils.RuleCreator(
    (ruleName) =>
        `https://github.com/distrohelena/linter/docs/rules/${ruleName}`,
);

/**
 * Enforces braces around single-statement control-flow bodies.
 */
export const controlBodyBracesRule = createRule({
    name: "control-body-braces",
    meta: {
        type: "layout",
        fixable: "code",
        docs: {
            description:
                "Require braces for if, else, for, for-in, for-of, while, and do-while bodies.",
        },
        messages: {
            controlBodyBraces:
                "Wrap this control-flow body in braces.",
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
                 * Recursively walks the syntax tree and inspects control-flow bodies.
                 * @param node Current syntax node.
                 */
                function visitNode(node: ts.Node): void {
                    if (ts.isIfStatement(node)) {
                        reportIfBodies(node);
                    } else if (
                        ts.isForStatement(node) ||
                        ts.isForInStatement(node) ||
                        ts.isForOfStatement(node) ||
                        ts.isWhileStatement(node) ||
                        ts.isDoStatement(node)
                    ) {
                        reportBody(node.statement, node);
                    }

                    ts.forEachChild(node, visitNode);
                }

                /**
                 * Reports bare if, else-if, and else bodies.
                 * @param statement If statement to inspect.
                 */
                function reportIfBodies(statement: ts.IfStatement): void {
                    reportBody(statement.thenStatement, statement);

                    if (
                        statement.elseStatement !== undefined &&
                        !ts.isIfStatement(statement.elseStatement)
                    ) {
                        reportBody(statement.elseStatement, statement);
                    }
                }

                /**
                 * Reports a control-flow body when it is not already wrapped in a block.
                 * @param statement Body statement to inspect.
                 * @param owner Owning control-flow node.
                 */
                function reportBody(
                    statement: ts.Statement,
                    owner: ts.Node,
                ): void {
                    if (ts.isBlock(statement)) {
                        return;
                    }

                    const position = sourceFile.getLineAndCharacterOfPosition(
                        statement.getStart(sourceFile),
                    );

                    context.report({
                        loc: createReportLoc(
                            position.line + 1,
                            position.character + 1,
                        ),
                        messageId: "controlBodyBraces",
                        fix(fixer) {
                            const openingRange = getBodyBoundaryRange(
                                owner,
                                statement,
                                sourceFile,
                            );
                            const closingRange = getBodyClosingRange(
                                owner,
                                statement,
                                sourceFile,
                            );
                            const newline = detectNewline(sourceText);
                            const outerIndentation = getLineIndentation(
                                owner.getStart(sourceFile),
                                sourceText,
                            );
                            const innerIndentation = `${outerIndentation}    `;
                            const closingText = getClosingBraceText(
                                owner,
                                statement,
                                newline,
                                outerIndentation,
                            );

                            return [
                                fixer.replaceTextRange(
                                    [
                                        openingRange[0],
                                        statement.getStart(sourceFile),
                                    ],
                                    ` {${newline}${innerIndentation}`,
                                ),
                                fixer.replaceTextRange(
                                    closingRange,
                                    closingText,
                                ),
                            ];
                        },
                    });
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
 * Resolves the insertion boundary between a control-flow header and its body.
 * @param owner Owning control-flow node.
 * @param statement Bare body statement.
 * @param sourceFile Source file used for token lookups.
 * @returns Source range whose end should be replaced with the opening brace segment.
 */
function getBodyBoundaryRange(
    owner: ts.Node,
    statement: ts.Statement,
    sourceFile: ts.SourceFile,
): [number, number] {
    if (ts.isIfStatement(owner) && owner.elseStatement === statement) {
        return [owner.getChildren(sourceFile).find((child) => child.kind === ts.SyntaxKind.ElseKeyword)?.getEnd() ?? statement.getStart(sourceFile), statement.getStart(sourceFile)];
    }

    if (ts.isDoStatement(owner)) {
        return [owner.getChildren(sourceFile).find((child) => child.kind === ts.SyntaxKind.DoKeyword)?.getEnd() ?? statement.getStart(sourceFile), statement.getStart(sourceFile)];
    }

    return [
        getOwnerHeaderEnd(owner, sourceFile),
        statement.getStart(sourceFile),
    ];
}

/**
 * Resolves the source range after a bare control-flow body where the closing brace should be written.
 * @param owner Owning control-flow node.
 * @param statement Bare body statement.
 * @param sourceFile Source file used for token lookups.
 * @returns Source range to replace with the closing brace segment.
 */
function getBodyClosingRange(
    owner: ts.Node,
    statement: ts.Statement,
    sourceFile: ts.SourceFile,
): [number, number] {
    if (
        ts.isIfStatement(owner) &&
        owner.thenStatement === statement &&
        owner.elseStatement !== undefined
    ) {
        const elseKeyword = owner
            .getChildren(sourceFile)
            .find((child) => child.kind === ts.SyntaxKind.ElseKeyword);

        return [
            statement.getEnd(),
            elseKeyword?.getStart(sourceFile) ?? owner.elseStatement.getStart(sourceFile),
        ];
    }

    if (ts.isDoStatement(owner)) {
        const whileKeyword = owner
            .getChildren(sourceFile)
            .find((child) => child.kind === ts.SyntaxKind.WhileKeyword);

        return [
            statement.getEnd(),
            whileKeyword?.getStart(sourceFile) ?? statement.getEnd(),
        ];
    }

    return [statement.getEnd(), statement.getEnd()];
}

/**
 * Finds the end position of the control-flow header that owns the bare body.
 * @param owner Owning control-flow node.
 * @param sourceFile Source file used for token lookups.
 * @returns End position of the header token that should precede the opening brace.
 */
function getOwnerHeaderEnd(
    owner: ts.Node,
    sourceFile: ts.SourceFile,
): number {
    const children = owner.getChildren(sourceFile);
    let closeParenToken: ts.Node | undefined;

    for (let index = children.length - 1; index >= 0; index -= 1) {
        const child = children[index];

        if (child.kind === ts.SyntaxKind.CloseParenToken) {
            closeParenToken = child;
            break;
        }
    }

    if (closeParenToken !== undefined) {
        return closeParenToken.getEnd();
    }

    return owner.getStart(sourceFile);
}

/**
 * Gets the leading indentation for the line containing the supplied position.
 * @param position Absolute source position.
 * @param sourceText Current source text.
 * @returns Leading indentation characters for the containing line.
 */
function getLineIndentation(position: number, sourceText: string): string {
    const lineStart = sourceText.lastIndexOf("\n", position - 1);
    const segmentStart = lineStart >= 0 ? lineStart + 1 : 0;
    const lineText = sourceText.slice(segmentStart, position);
    return lineText.match(/^\s*/)?.[0] ?? "";
}

/**
 * Builds the closing brace text for a wrapped control-flow body.
 * @param owner Owning control-flow node.
 * @param statement Bare body statement.
 * @param newline Preferred newline sequence.
 * @param outerIndentation Indentation used by the owning control-flow statement.
 * @returns Replacement text for the closing brace segment.
 */
function getClosingBraceText(
    owner: ts.Node,
    statement: ts.Statement,
    newline: string,
    outerIndentation: string,
): string {
    if (
        (ts.isIfStatement(owner) &&
            owner.thenStatement === statement &&
            owner.elseStatement !== undefined) ||
        ts.isDoStatement(owner)
    ) {
        return `${newline}${outerIndentation}} `;
    }

    return `${newline}${outerIndentation}}`;
}
