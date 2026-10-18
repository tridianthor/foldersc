using FolderSC.Core.Models;

namespace FolderSC.Core.Services;

/// <summary>
/// Unifying orchestrator service coordinating path normalization, filename deduction,
/// destination directory resolution, and generator dispatching.
/// </summary>
public interface IGenerationOrchestrator
{
    /// <summary>
    /// Processes a <see cref="GenerationRequest"/> and produces a ready-to-use .bat or .exe file.
    /// </summary>
    /// <param name="request">The generation request parameters.</param>
    /// <returns>A <see cref="GenerationResult"/> detailing the outcome and output file path.</returns>
    Task<GenerationResult> ProcessAsync(GenerationRequest request);
}
