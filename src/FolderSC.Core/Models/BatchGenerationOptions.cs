namespace FolderSC.Core.Models;

/// <summary>
/// Configuration options for generating a Windows batch script.
/// </summary>
public record BatchGenerationOptions
{
    /// <summary>
    /// Target path to create (may include environment variables such as %LOCALAPPDATA%\... or absolute paths).
    /// </summary>
    public required string TargetPath { get; init; }

    /// <summary>
    /// Whether to open Windows File Explorer at the target folder after creating it.
    /// Default is true.
    /// </summary>
    public bool OpenExplorerOnCompletion { get; init; } = true;

    /// <summary>
    /// Whether to pause the console at the end before closing so the user can see the output.
    /// Default is true.
    /// </summary>
    public bool PauseOnCompletion { get; init; } = true;

    /// <summary>
    /// Optional title or game name for display in the console banner.
    /// </summary>
    public string? Title { get; init; }
}
