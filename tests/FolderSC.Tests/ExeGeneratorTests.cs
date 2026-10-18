using System.Diagnostics;
using FolderSC.Core.Models;
using FolderSC.Core.Services;

namespace FolderSC.Tests;

public class ExeGeneratorTests : IDisposable
{
    private readonly IExeGenerator _generator = new ExeGenerator();
    private readonly IPayloadPacker _packer = new PayloadPacker();
    private readonly string _testDirectory;

    public ExeGeneratorTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "FolderSC_ExeGenTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup errors in temp folder
        }
    }

    [Fact]
    public async Task GenerateExeAsync_WhenOptionsNull_ThrowsArgumentNullException()
    {
        string dest = Path.Combine(_testDirectory, "test.exe");
        await Assert.ThrowsAsync<ArgumentNullException>(() => _generator.GenerateExeAsync(dest, null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GenerateExeAsync_WhenDestinationPathNullOrEmpty_ThrowsArgumentException(string? invalidDest)
    {
        var options = new ExeGenerationOptions
        {
            TargetPath = @"%LOCALAPPDATA%\Test"
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _generator.GenerateExeAsync(invalidDest!, options));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GenerateExeAsync_WhenTargetPathNullOrEmpty_ThrowsArgumentException(string? invalidPath)
    {
        string dest = Path.Combine(_testDirectory, "test.exe");
        var options = new ExeGenerationOptions
        {
            TargetPath = invalidPath!
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _generator.GenerateExeAsync(dest, options));
    }

    [Fact]
    public async Task GenerateExeAsync_WithMockStub_ProducesCombinedFile()
    {
        byte[] dummyStub = new byte[] { 0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00 };
        var mockProvider = new MockStubProvider(dummyStub);
        var generator = new ExeGenerator(mockProvider);

        string dest = Path.Combine(_testDirectory, "MockOutput.exe");
        var options = new ExeGenerationOptions
        {
            TargetPath = @"%LOCALAPPDATA%\GameStudio\Saved",
            OpenExplorerOnCompletion = false,
            Title = "MockGame"
        };

        string resultPath = await generator.GenerateExeAsync(dest, options);

        Assert.True(File.Exists(resultPath));
        byte[] fileBytes = await File.ReadAllBytesAsync(resultPath);

        // Header starts with MZ
        Assert.Equal((byte)'M', fileBytes[0]);
        Assert.Equal((byte)'Z', fileBytes[1]);

        // Can unpack payload from generated file
        var unpacked = _packer.Unpack(fileBytes);
        Assert.NotNull(unpacked);
        Assert.Equal(@"%LOCALAPPDATA%\GameStudio\Saved", unpacked.TargetPath);
        Assert.False(unpacked.OpenInExplorer);
        Assert.Equal("MockGame", unpacked.Title);
    }

    [Fact]
    public async Task GenerateExeAsync_WithRealEmbeddedStub_GeneratesValidExecutableWithPeHeader()
    {
        string dest = Path.Combine(_testDirectory, "RealOutput.exe");
        var options = new ExeGenerationOptions
        {
            TargetPath = @"%LOCALAPPDATA%\RealGame\Saves",
            OpenExplorerOnCompletion = true,
            Title = "RealGame"
        };

        string resultPath = await _generator.GenerateExeAsync(dest, options);

        Assert.True(File.Exists(resultPath));
        var fi = new FileInfo(resultPath);
        Assert.True(fi.Length > 1_000_000); // Embedded stub is > 1MB

        // Verify PE MZ magic bytes
        using var fs = new FileStream(resultPath, FileMode.Open, FileAccess.Read);
        byte[] header = new byte[2];
        fs.ReadExactly(header);
        Assert.Equal((byte)'M', header[0]);
        Assert.Equal((byte)'Z', header[1]);

        // Verify payload can be unpacked
        byte[] allBytes = await File.ReadAllBytesAsync(resultPath);
        var unpacked = _packer.Unpack(allBytes);
        Assert.Equal(@"%LOCALAPPDATA%\RealGame\Saves", unpacked.TargetPath);
        Assert.True(unpacked.OpenInExplorer);
        Assert.Equal("RealGame", unpacked.Title);
    }

    [Fact]
    public async Task GenerateExeAsync_SubprocessExecution_PhysicallyCreatesTargetDirectory()
    {
        string targetDirectory = Path.Combine(_testDirectory, "NestedA", "NestedB", "DeepGameSaves");
        string exePath = Path.Combine(_testDirectory, "RunMe.exe");

        var options = new ExeGenerationOptions
        {
            TargetPath = targetDirectory,
            OpenExplorerOnCompletion = false,
            Title = "IntegrationTest"
        };

        await _generator.GenerateExeAsync(exePath, options);

        Assert.False(Directory.Exists(targetDirectory));

        // Execute generated executable with --silent
        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = "--silent",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        Assert.NotNull(process);
        await process.WaitForExitAsync();

        Assert.Equal(0, process.ExitCode);
        Assert.True(Directory.Exists(targetDirectory));
    }

    private sealed class MockStubProvider(byte[] bytes) : IStubBinaryProvider
    {
        public byte[] GetStubBytes() => bytes;
    }
}
