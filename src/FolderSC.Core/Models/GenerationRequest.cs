namespace FolderSC.Core.Models;

/// <summary>
/// Encapsulates all user parameters required to generate a folder replication script or executable.
/// </summary>
public record GenerationRequest
{
    /// <summary>
    /// The raw source folder path provided by the user.
    /// </summary>
    public required string SourcePath { get; init; }

    /// <summary>
    /// The desired output format (Executable or Batch).
    /// Default is Executable.
    /// </summary>
    public OutputFormat Format { get; init; } = OutputFormat.Executable;

    /// <summary>
    /// Whether to replace user-specific directories with dynamic environment variables (%LOCALAPPDATA%, etc.).
    /// Default is true.
    /// </summary>
    public bool UseDynamicVariables { get; init; } = true;

    /// <summary>
    /// Whether the generated script/executable should open File Explorer at the target directory upon execution.
    /// Default is true.
    /// </summary>
    public bool OpenExplorerOnCompletion { get; init; } = true;

    /// <summary>
    /// Optional custom destination directory for the generated file. If null or empty, defaults to the user's Desktop.
    /// </summary>
    public string? OutputDirectory { get; init; }

    /// <summary>
    /// Optional custom filename for the generated file (without extension or with extension).
    /// If null or empty, auto-deduced from leaf folder name: "Create_<Leaf>_Folder".
    /// </summary>
    public string? OutputFileName { get; init; }

    /// <summary>
    /// Optional title or game name for display in generated banners or dialogs.
    /// </summary>
    public string? Title { get; init; }
}
