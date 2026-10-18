using FolderSC.Core.Models;

namespace FolderSC.Core.Services;

/// <summary>
/// Coordinates the generation workflow by normalizing the source path, deducing filenames,
/// resolving the output directory, and dispatching to the appropriate generator (.exe or .bat).
/// </summary>
public class GenerationOrchestrator : IGenerationOrchestrator
{
    private readonly IPathNormalizer _pathNormalizer;
    private readonly IBatchGenerator _batchGenerator;
    private readonly IExeGenerator _exeGenerator;

    public GenerationOrchestrator(
        IPathNormalizer? pathNormalizer = null,
        IBatchGenerator? batchGenerator = null,
        IExeGenerator? exeGenerator = null)
    {
        _pathNormalizer = pathNormalizer ?? new PathNormalizer();
        _batchGenerator = batchGenerator ?? new BatchGenerator();
        _exeGenerator = exeGenerator ?? new ExeGenerator();
    }

    public async Task<GenerationResult> ProcessAsync(GenerationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.SourcePath))
        {
            return GenerationResult.Fail("Source path cannot be empty or whitespace.", request.Format);
        }

        NormalizedPathResult normalizedResult;
        try
        {
            normalizedResult = _pathNormalizer.Normalize(request.SourcePath, request.UseDynamicVariables);
        }
        catch (Exception ex)
        {
            return GenerationResult.Fail($"Invalid source path: {ex.Message}", request.Format);
        }

        string outDir = ResolveOutputDirectory(request.OutputDirectory);
        string destinationFilePath = BuildDestinationFilePath(outDir, request.OutputFileName, normalizedResult.SuggestedFileName, request.Format);
        string title = string.IsNullOrWhiteSpace(request.Title) ? normalizedResult.LeafDirectoryName : request.Title.Trim();

        try
        {
            string generatedFilePath;

            if (request.Format == OutputFormat.Batch)
            {
                var options = new BatchGenerationOptions
                {
                    TargetPath = normalizedResult.NormalizedPath,
                    OpenExplorerOnCompletion = request.OpenExplorerOnCompletion,
                    PauseOnCompletion = true,
                    Title = title
                };

                generatedFilePath = await _batchGenerator.WriteToFileAsync(destinationFilePath, options);
            }
            else
            {
                var options = new ExeGenerationOptions
                {
                    TargetPath = normalizedResult.NormalizedPath,
                    OpenExplorerOnCompletion = request.OpenExplorerOnCompletion,
                    Title = title
                };

                generatedFilePath = await _exeGenerator.GenerateExeAsync(destinationFilePath, options);
            }

            return GenerationResult.Ok(generatedFilePath, normalizedResult.NormalizedPath, request.Format);
        }
        catch (Exception ex)
        {
            return GenerationResult.Fail($"Generation failed: {ex.Message}", request.Format, normalizedResult.NormalizedPath);
        }
    }

    private static string ResolveOutputDirectory(string? userProvidedDirectory)
    {
        if (!string.IsNullOrWhiteSpace(userProvidedDirectory))
        {
            return userProvidedDirectory.Trim();
        }

        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        if (!string.IsNullOrWhiteSpace(desktop))
        {
            return desktop;
        }

        return Directory.GetCurrentDirectory();
    }

    private static string BuildDestinationFilePath(
        string directory,
        string? userSpecifiedFileName,
        string suggestedFileName,
        OutputFormat format)
    {
        string extension = format == OutputFormat.Executable ? ".exe" : ".bat";
        string baseName;

        if (!string.IsNullOrWhiteSpace(userSpecifiedFileName))
        {
            string trimmed = userSpecifiedFileName.Trim();

            // Strip existing extension if entered by user to guarantee format matches request
            if (trimmed.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                trimmed.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) ||
                trimmed.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase))
            {
                int dotIndex = trimmed.LastIndexOf('.');
                baseName = trimmed[..dotIndex];
            }
            else
            {
                baseName = trimmed;
            }
        }
        else
        {
            baseName = suggestedFileName;
        }

        return Path.Combine(directory, $"{baseName}{extension}");
    }
}
