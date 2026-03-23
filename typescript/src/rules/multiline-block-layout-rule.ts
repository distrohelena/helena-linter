import ts from "typescript";
import { ESLintUtils } from "@typescript-eslint/utils";
import { createReportLoc } from "../utils/report-loc.js";

const createRule = ESLintUtils.RuleCreator(
    (ruleName) =>
        `https://github.com/distrohelena/linter/docs/rules/${ruleName}`,
);

/**
 * Enforces multiline layout for non-empty block bodies.
 */
export const multilineBlockLayoutRule = createRule({
    name: "multiline-block-layout",
    meta: {
        type: "layout",
        fixable: "code",
        docs: {
            description:
                "Require non-empty block bodies to break across lines while allowing empty blocks on one line.",
        },
        messages: {
            multilineBlockLayout:
                "Break this non-empty block across multiple lines.",
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
                 * Recursively inspects block nodes in the current source file.
                 * @param node Current syntax node.
                 */
                function visitNode(node: ts.Node): void {
                    if (ts.isBlock(node) || ts.isModuleBlock(node)) {
                        reportBlock(node);
                    }

                    ts.forEachChild(node, visitNode);
                }

                /**
                 * Reports a non-empty block that is kept on a single line.
                 * @param block Block node to inspect.
                 */
                function reportBlock(
                    block: ts.Block | ts.ModuleBlock,
                ): void {
                    if (block.statements.length === 0) {
                        return;
                    }

                    const openBraceToken = block
                        .getChildren(sourceFile)
                        .find((child) => child.kind === ts.SyntaxKind.OpenBraceToken);
                    const closeBraceToken = block
                        .getChildren(sourceFile)
                        .find((child) => child.kind === ts.SyntaxKind.CloseBraceToken);

                    if (openBraceToken === undefined || closeBraceToken === undefined) {
                        return;
                    }

                    const openBraceLine = sourceFile.getLineAndCharacterOfPosition(
                        openBraceToken.getStart(sourceFile),
                    ).line;
                    const closeBraceLine = sourceFile.getLineAndCharacterOfPosition(
                        closeBraceToken.getStart(sourceFile),
                    ).line;

                    if (openBraceLine !== closeBraceLine) {
                        return;
                    }

                    const position = sourceFile.getLineAndCharacterOfPosition(
                        openBraceToken.getStart(sourceFile),
                    );

                    context.report({
                        loc: createReportLoc(
                            position.line + 1,
                            position.character + 1,
                        ),
                        messageId: "multilineBlockLayout",
                        fix(fixer) {
                            const newline = detectNewline(sourceText);
                            const outerIndentation = getLineIndentation(
                                block.getStart(sourceFile),
                                sourceText,
                            );
                            const innerIndentation = `${outerIndentation}    `;
                            const innerText = sourceText
                                .slice(openBraceToken.getEnd(), closeBraceToken.getStart(sourceFile))
                                .trim();

                            return fixer.replaceTextRange(
                                [block.getStart(sourceFile), block.getEnd()],
                                `{${newline}${innerIndentation}${innerText}${newline}${outerIndentation}}`,
                            );
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
