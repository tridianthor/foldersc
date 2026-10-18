namespace FolderSC.Core.Models;

/// <summary>
/// Data payload embedded into standalone runner executables.
/// </summary>
public record RunnerPayload
{
    /// <summary>
    /// Target directory to create on the handheld device (may contain dynamic tokens like %LOCALAPPDATA%).
    /// </summary>
    public required string TargetPath { get; init; }

    /// <summary>
    /// Whether to open Windows File Explorer at the target folder after creation.
    /// Default is true.
    /// </summary>
    public bool OpenInExplorer { get; init; } = true;

    /// <summary>
    /// Optional title or game name displayed in message dialogs.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Timestamp when this package was created (UTC).
    /// </summary>
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
}
