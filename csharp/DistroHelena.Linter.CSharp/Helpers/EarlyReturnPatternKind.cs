namespace DistroHelena.Linter.CSharp.Helpers;

/// <summary>
/// Identifies the supported early-return rewrite families.
/// </summary>
public enum EarlyReturnPatternKind
{
    /// <summary>
    /// An <c>if / else</c> where the <c>if</c> branch exits and the <c>else</c> branch is hoisted.
    /// </summary>
    ExitingIfBranch,

    /// <summary>
    /// An <c>if / else</c> where the <c>else</c> branch exits and the <c>if</c> branch is hoisted after inversion.
    /// </summary>
    ExitingElseBranch,

    /// <summary>
    /// A wrapped happy-path <c>if</c> followed by an exiting sibling statement.
    /// </summary>
    WrappedHappyPath,
}
