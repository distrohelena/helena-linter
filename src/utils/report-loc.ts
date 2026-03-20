/**
 * Creates an ESLint source location from one-based line and column values.
 * @param line One-based line.
 * @param column One-based column.
 * @returns ESLint source location.
 */
export function createReportLoc(line: number, column: number) {
    return {
        start: {
            line,
            column: column - 1,
        },
        end: {
            line,
            column,
        },
    };
}
