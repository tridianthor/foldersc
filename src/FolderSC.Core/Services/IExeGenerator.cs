using FolderSC.Core.Models;

namespace FolderSC.Core.Services;

/// <summary>
/// Service responsible for generating standalone, portable Windows executables (.exe)
/// with embedded binary overlay payloads for one-click directory replication.
/// </summary>
public interface IExeGenerator
{
    /// <summary>
    /// Generates a standalone executable containing the embedded runner and custom configuration overlay.
    /// </summary>
    /// <param name="destinationFilePath">Full destination path for the output .exe file.</param>
    /// <param name="options">Executable generation options.</param>
    /// <returns>The normalized full path of the created executable.</returns>
    Task<string> GenerateExeAsync(string destinationFilePath, ExeGenerationOptions options);
}
