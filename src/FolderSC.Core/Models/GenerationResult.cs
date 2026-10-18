namespace FolderSC.Core.Models;

/// <summary>
/// Encapsulates the outcome of a generation operation.
/// </summary>
public record GenerationResult
{
    /// <summary>
    /// Whether the generation succeeded.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Absolute path to the generated file (.exe or .bat).
    /// </summary>
    public string? OutputFilePath { get; init; }

    /// <summary>
    /// The normalized target folder path that the generated file will create.
    /// </summary>
    public string? NormalizedTargetPath { get; init; }

    /// <summary>
    /// Error message if generation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Output format generated.
    /// </summary>
    public OutputFormat Format { get; init; }

    public static GenerationResult Ok(string outputFilePath, string normalizedTargetPath, OutputFormat format) =>
        new()
        {
            Success = true,
            OutputFilePath = outputFilePath,
            NormalizedTargetPath = normalizedTargetPath,
            Format = format
        };

    public static GenerationResult Fail(string errorMessage, OutputFormat format, string? normalizedTargetPath = null) =>
        new()
        {
            Success = false,
            ErrorMessage = errorMessage,
            NormalizedTargetPath = normalizedTargetPath,
            Format = format
        };
}
