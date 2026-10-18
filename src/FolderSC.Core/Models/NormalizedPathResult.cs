namespace FolderSC.Core.Models;

/// <summary>
/// Represents the result of a path normalization operation.
/// </summary>
public record NormalizedPathResult
{
    /// <summary>
    /// The original input path trimmed of outer quotes and whitespace.
    /// </summary>
    public required string OriginalPath { get; init; }

    /// <summary>
    /// The normalized target path with backslashes, optional environment variable tokens, and clean formatting.
    /// </summary>
    public required string NormalizedPath { get; init; }

    /// <summary>
    /// Whether any environment variable token (e.g., %LOCALAPPDATA%, %APPDATA%, %USERPROFILE%) was applied.
    /// </summary>
    public required bool HasDynamicVariables { get; init; }

    /// <summary>
    /// The detected leaf directory name (e.g., "SaveGames" or "Saved").
    /// </summary>
    public required string LeafDirectoryName { get; init; }

    /// <summary>
    /// Suggested script or executable name, e.g., "Create_SaveGames_Folder".
    /// </summary>
    public required string SuggestedFileName { get; init; }
}
