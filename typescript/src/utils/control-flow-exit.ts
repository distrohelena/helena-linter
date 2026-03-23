import ts from "typescript";

/**
 * Determines whether a TypeScript statement always exits the current control-flow path.
 * @param statement Statement to inspect.
 * @returns True when execution cannot continue past the statement.
 */
export function doesStatementAlwaysExit(statement: ts.Statement): boolean {
    if (
        ts.isReturnStatement(statement) ||
        ts.isThrowStatement(statement) ||
        ts.isContinueStatement(statement) ||
        ts.isBreakStatement(statement)
    ) {
        return true;
    } else if (ts.isBlock(statement)) {
        for (const blockStatement of statement.statements) {
            if (doesStatementAlwaysExit(blockStatement)) {
                return true;
            }
        }

        return false;
    } else if (ts.isIfStatement(statement)) {
        if (statement.elseStatement === undefined) {
            return false;
        }

        return (
            doesStatementAlwaysExit(statement.thenStatement) &&
            doesStatementAlwaysExit(statement.elseStatement)
        );
    } else if (ts.isTryStatement(statement)) {
        if (
            statement.finallyBlock !== undefined &&
            doesStatementAlwaysExit(statement.finallyBlock)
        ) {
            return true;
        } else if (statement.catchClause === undefined) {
            return false;
        }

        return (
            doesStatementAlwaysExit(statement.tryBlock) &&
            doesStatementAlwaysExit(statement.catchClause.block)
        );
    } else if (ts.isLabeledStatement(statement)) {
        return doesStatementAlwaysExit(statement.statement);
    }

    return false;
}
