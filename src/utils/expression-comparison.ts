import ts from "typescript";

/**
 * Describes a supported nullish comparison.
 */
export interface NullishComparisonDetails {
  /**
   * Gets the compared expression text.
   */
  targetText: string;
  /**
   * Gets the nullish literal kind.
   */
  nullishKind: "null" | "undefined";
}

/**
 * Describes a complementary expression pair.
 */
export interface ComplementComparisonDetails {
  /**
   * Gets the target expression text for diagnostics.
   */
  targetText: string;
}

/**
 * Removes parentheses so comparisons can be matched consistently.
 * @param expression Expression to normalize.
 * @returns Unwrapped expression.
 */
export function unwrapExpression(expression: ts.Expression): ts.Expression {
  let currentExpression = expression;

  while (ts.isParenthesizedExpression(currentExpression)) {
    currentExpression = currentExpression.expression;
  }

  return currentExpression;
}

/**
 * Extracts supported null or undefined comparison details from an expression.
 * @param expression Expression to inspect.
 * @param sourceFile Source file used to normalize target text.
 * @returns Nullish comparison details when supported.
 */
export function tryGetNullishComparisonDetails(
  expression: ts.Expression,
  sourceFile: ts.SourceFile,
): NullishComparisonDetails | undefined {
  const unwrappedExpression = unwrapExpression(expression);

  if (!ts.isBinaryExpression(unwrappedExpression)) {
    return undefined;
  }

  const operatorKind = unwrappedExpression.operatorToken.kind;

  if (
    operatorKind !== ts.SyntaxKind.EqualsEqualsEqualsToken &&
    operatorKind !== ts.SyntaxKind.EqualsEqualsToken
  ) {
    return undefined;
  }

  const leftSideKind = tryGetNullishLiteralKind(unwrappedExpression.left);
  const rightSideKind = tryGetNullishLiteralKind(unwrappedExpression.right);

  if (leftSideKind !== undefined && rightSideKind === undefined) {
    return {
      targetText: unwrapExpression(unwrappedExpression.right).getText(
        sourceFile,
      ),
      nullishKind: leftSideKind,
    };
  } else if (rightSideKind !== undefined && leftSideKind === undefined) {
    return {
      targetText: unwrapExpression(unwrappedExpression.left).getText(
        sourceFile,
      ),
      nullishKind: rightSideKind,
    };
  }

  return undefined;
}

/**
 * Determines whether two expressions are exact logical complements.
 * @param firstExpression Leading expression.
 * @param secondExpression Complement candidate expression.
 * @returns Complement comparison details when the expressions are complementary.
 */
export function tryGetComplementComparison(
  firstExpression: ts.Expression,
  secondExpression: ts.Expression,
): ComplementComparisonDetails | undefined {
  const firstUnwrappedExpression = unwrapExpression(firstExpression);
  const secondUnwrappedExpression = unwrapExpression(secondExpression);

  if (ts.isPrefixUnaryExpression(firstUnwrappedExpression)) {
    if (firstUnwrappedExpression.operator !== ts.SyntaxKind.ExclamationToken) {
      return undefined;
    }

    const targetExpression = unwrapExpression(firstUnwrappedExpression.operand);
    if (targetExpression.getText() !== secondUnwrappedExpression.getText()) {
      return undefined;
    }

    return { targetText: secondUnwrappedExpression.getText() };
  } else if (ts.isPrefixUnaryExpression(secondUnwrappedExpression)) {
    if (secondUnwrappedExpression.operator !== ts.SyntaxKind.ExclamationToken) {
      return undefined;
    }

    const targetExpression = unwrapExpression(
      secondUnwrappedExpression.operand,
    );
    if (targetExpression.getText() !== firstUnwrappedExpression.getText()) {
      return undefined;
    }

    return { targetText: firstUnwrappedExpression.getText() };
  } else if (
    !ts.isBinaryExpression(firstUnwrappedExpression) ||
    !ts.isBinaryExpression(secondUnwrappedExpression)
  ) {
    return undefined;
  }

  const firstOperatorKind = firstUnwrappedExpression.operatorToken.kind;
  const secondOperatorKind = secondUnwrappedExpression.operatorToken.kind;
  const areComplementaryOperators =
    (firstOperatorKind === ts.SyntaxKind.EqualsEqualsEqualsToken &&
      secondOperatorKind === ts.SyntaxKind.ExclamationEqualsEqualsToken) ||
    (firstOperatorKind === ts.SyntaxKind.ExclamationEqualsEqualsToken &&
      secondOperatorKind === ts.SyntaxKind.EqualsEqualsEqualsToken) ||
    (firstOperatorKind === ts.SyntaxKind.EqualsEqualsToken &&
      secondOperatorKind === ts.SyntaxKind.ExclamationEqualsToken) ||
    (firstOperatorKind === ts.SyntaxKind.ExclamationEqualsToken &&
      secondOperatorKind === ts.SyntaxKind.EqualsEqualsToken);

  if (!areComplementaryOperators) {
    return undefined;
  }

  const sameDirectOrder =
    unwrapExpression(firstUnwrappedExpression.left).getText() ===
      unwrapExpression(secondUnwrappedExpression.left).getText() &&
    unwrapExpression(firstUnwrappedExpression.right).getText() ===
      unwrapExpression(secondUnwrappedExpression.right).getText();
  const sameSwappedOrder =
    unwrapExpression(firstUnwrappedExpression.left).getText() ===
      unwrapExpression(secondUnwrappedExpression.right).getText() &&
    unwrapExpression(firstUnwrappedExpression.right).getText() ===
      unwrapExpression(secondUnwrappedExpression.left).getText();

  if (!sameDirectOrder && !sameSwappedOrder) {
    return undefined;
  }

  return { targetText: secondUnwrappedExpression.getText() };
}

/**
 * Resolves whether an expression represents `null` or `undefined`.
 * @param expression Expression to inspect.
 * @returns Nullish literal kind when supported.
 */
function tryGetNullishLiteralKind(
  expression: ts.Expression,
): "null" | "undefined" | undefined {
  const unwrappedExpression = unwrapExpression(expression);

  if (unwrappedExpression.kind === ts.SyntaxKind.NullKeyword) {
    return "null";
  } else if (
    ts.isIdentifier(unwrappedExpression) &&
    unwrappedExpression.text === "undefined"
  ) {
    return "undefined";
  }

  return undefined;
}
