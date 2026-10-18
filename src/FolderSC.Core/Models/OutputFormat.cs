namespace FolderSC.Core.Models;

/// <summary>
/// Specifies the generated output script or executable format.
/// </summary>
public enum OutputFormat
{
    /// <summary>
    /// Standalone executable (.exe) using pre-packaged runner stub and FSC1 overlay.
    /// </summary>
    Executable,

    /// <summary>
    /// Standard Windows command-line batch script (.bat).
    /// </summary>
    Batch
}
