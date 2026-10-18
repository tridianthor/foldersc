namespace FolderSC.Core.Models;

/// <summary>
/// Configuration options for generating a standalone runner executable (.exe).
/// </summary>
public record ExeGenerationOptions
{
    /// <summary>
    /// Target path to create on the handheld (may include environment variables such as %LOCALAPPDATA%\... or absolute paths).
    /// </summary>
    public required string TargetPath { get; init; }

    /// <summary>
    /// Whether to open Windows File Explorer at the target folder after creation.
    /// Default is true.
    /// </summary>
    public bool OpenExplorerOnCompletion { get; init; } = true;

    /// <summary>
    /// Optional title or game name for display in the notification dialog.
    /// </summary>
    public string? Title { get; init; }
}
