using FolderSC.Core.Models;

namespace FolderSC.Core.Services;

/// <summary>
/// Implements <see cref="IExeGenerator"/> to output standalone, zero-dependency Windows executables (.exe)
/// by combining the embedded runner stub with an appended FSC1 binary overlay.
/// </summary>
public class ExeGenerator : IExeGenerator
{
    private readonly IStubBinaryProvider _stubProvider;
    private readonly IPayloadPacker _payloadPacker;

    public ExeGenerator(IStubBinaryProvider? stubProvider = null, IPayloadPacker? payloadPacker = null)
    {
        _stubProvider = stubProvider ?? new EmbeddedStubBinaryProvider();
        _payloadPacker = payloadPacker ?? new PayloadPacker();
    }

    public async Task<string> GenerateExeAsync(string destinationFilePath, ExeGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(destinationFilePath))
        {
            throw new ArgumentException("Destination file path cannot be empty or whitespace.", nameof(destinationFilePath));
        }

        if (string.IsNullOrWhiteSpace(options.TargetPath))
        {
            throw new ArgumentException("Target path cannot be empty or whitespace.", nameof(options));
        }

        string fullPath = Path.GetFullPath(destinationFilePath);
        string? directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        byte[] stubBytes = _stubProvider.GetStubBytes();

        var runnerPayload = new RunnerPayload
        {
            TargetPath = options.TargetPath.Trim(),
            OpenInExplorer = options.OpenExplorerOnCompletion,
            Title = options.Title?.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        byte[] overlayBytes = _payloadPacker.Pack(runnerPayload);

        using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true))
        {
            await fs.WriteAsync(stubBytes);
            await fs.WriteAsync(overlayBytes);
            await fs.FlushAsync();
        }

        return fullPath;
    }
}
