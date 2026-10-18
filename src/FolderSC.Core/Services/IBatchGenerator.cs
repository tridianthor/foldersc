using FolderSC.Core.Models;

namespace FolderSC.Core.Services;

/// <summary>
/// Service responsible for generating portable Windows Batch (.bat) scripts to recreate folder structures.
/// </summary>
public interface IBatchGenerator
{
    /// <summary>
    /// Generates the raw batch script content based on the provided options.
    /// </summary>
    /// <param name="options">Batch generation options including target path and flags.</param>
    /// <returns>The generated batch script string.</returns>
    string GenerateScriptContent(BatchGenerationOptions options);

    /// <summary>
    /// Generates the batch script and writes it to the designated destination file path.
    /// </summary>
    /// <param name="destinationFilePath">Target path to write the .bat script.</param>
    /// <param name="options">Batch generation options.</param>
    /// <returns>The normalized full path of the created script file.</returns>
    Task<string> WriteToFileAsync(string destinationFilePath, BatchGenerationOptions options);
}
