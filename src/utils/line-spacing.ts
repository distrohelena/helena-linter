import ts from "typescript";

/**
 * Gets the line distance between two statement boundaries.
 * @param currentStatement Current statement.
 * @param nextStatement Next sibling statement.
 * @param sourceFile Source file used for position lookups.
 * @returns Zero-based line distance between statement boundaries.
 */
export function getLineDistanceBetweenStatements(
  currentStatement: ts.Statement,
  nextStatement: ts.Statement,
  sourceFile: ts.SourceFile,
): number {
  const currentStatementPosition = sourceFile.getLineAndCharacterOfPosition(
    getStatementTerminatorPosition(currentStatement, sourceFile),
  );
  const nextStatementPosition = sourceFile.getLineAndCharacterOfPosition(
    nextStatement.getStart(sourceFile),
  );

  return nextStatementPosition.line - currentStatementPosition.line;
}

/**
 * Gets the one-based location of a statement start.
 * @param statement Statement to inspect.
 * @param sourceFile Source file used for position lookups.
 * @returns One-based line and column.
 */
export function getStatementStartLocation(
  statement: ts.Statement,
  sourceFile: ts.SourceFile,
): { line: number; column: number } {
  const position = sourceFile.getLineAndCharacterOfPosition(
    statement.getStart(sourceFile),
  );

  return {
    line: position.line + 1,
    column: position.character + 1,
  };
}

/**
 * Resolves the source position of the last token in a statement.
 * @param statement Statement to inspect.
 * @param sourceFile Source file used for token lookups.
 * @returns End position for the statement terminator token.
 */
function getStatementTerminatorPosition(
  statement: ts.Statement,
  sourceFile: ts.SourceFile,
): number {
  const closingBraceToken = findLastClosingBraceToken(statement, sourceFile);

  if (closingBraceToken !== undefined) {
    return closingBraceToken.getStart(sourceFile);
  }

  return statement.getEnd();
}

/**
 * Finds the last closing brace token owned by the statement, when present.
 * @param statement Statement to inspect.
 * @param sourceFile Source file used for token lookups.
 * @returns Matching closing brace token, when available.
 */
function findLastClosingBraceToken(
  statement: ts.Statement,
  sourceFile: ts.SourceFile,
): ts.Node | undefined {
  let currentNode: ts.Node | undefined = statement;
  let lastClosingBraceToken: ts.Node | undefined;

  while (currentNode !== undefined) {
    const closingBraceToken = currentNode
      .getChildren(sourceFile)
      .find((child) => child.kind === ts.SyntaxKind.CloseBraceToken);

    if (closingBraceToken !== undefined) {
      lastClosingBraceToken = closingBraceToken;
    }

    if (
      ts.isIfStatement(currentNode) &&
      currentNode.elseStatement !== undefined
    ) {
      currentNode = currentNode.elseStatement;
    } else {
      break;
    }
  }

  return lastClosingBraceToken;
}
