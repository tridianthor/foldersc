using FolderSC.Core.Models;
using FolderSC.Core.Services;

namespace FolderSC.Tests;

public class GenerationOrchestratorTests : IDisposable
{
    private readonly string _testDirectory;

    public GenerationOrchestratorTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "FolderSC_OrchTests_" + Guid.NewGuid().ToString("N"));
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
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task ProcessAsync_WhenRequestNull_ThrowsArgumentNullException()
    {
        var orchestrator = new GenerationOrchestrator();
        await Assert.ThrowsAsync<ArgumentNullException>(() => orchestrator.ProcessAsync(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ProcessAsync_WhenSourcePathEmpty_ReturnsFailureResult(string emptyPath)
    {
        var orchestrator = new GenerationOrchestrator();
        var request = new GenerationRequest
        {
            SourcePath = emptyPath,
            Format = OutputFormat.Batch
        };

        var result = await orchestrator.ProcessAsync(request);

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        Assert.Equal(OutputFormat.Batch, result.Format);
    }

    [Fact]
    public async Task ProcessAsync_WhenBatchFormatSelected_GeneratesBatFileAndReturnsSuccess()
    {
        var orchestrator = new GenerationOrchestrator();
        var request = new GenerationRequest
        {
            SourcePath = @"C:\Users\Player\AppData\Local\StudioGame\SaveData",
            Format = OutputFormat.Batch,
            OutputDirectory = _testDirectory
        };

        var result = await orchestrator.ProcessAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.OutputFilePath);
        Assert.True(File.Exists(result.OutputFilePath));
        Assert.EndsWith(".bat", result.OutputFilePath);
        Assert.Equal(@"%LOCALAPPDATA%\StudioGame\SaveData", result.NormalizedTargetPath);
    }

    [Fact]
    public async Task ProcessAsync_WhenExecutableFormatSelected_GeneratesExeFileAndReturnsSuccess()
    {
        var orchestrator = new GenerationOrchestrator();
        var request = new GenerationRequest
        {
            SourcePath = @"C:\Users\Player\AppData\Local\StudioGame\SaveData",
            Format = OutputFormat.Executable,
            OutputDirectory = _testDirectory
        };

        var result = await orchestrator.ProcessAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.OutputFilePath);
        Assert.True(File.Exists(result.OutputFilePath));
        Assert.EndsWith(".exe", result.OutputFilePath);
        Assert.Equal(@"%LOCALAPPDATA%\StudioGame\SaveData", result.NormalizedTargetPath);
    }

    [Fact]
    public async Task ProcessAsync_WhenOutputFileNameNotProvided_DeducesNameFromLeafFolder()
    {
        var orchestrator = new GenerationOrchestrator();
        var request = new GenerationRequest
        {
            SourcePath = @"C:\Games\Cyberpunk\SavedGames",
            Format = OutputFormat.Batch,
            OutputDirectory = _testDirectory
        };

        var result = await orchestrator.ProcessAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.OutputFilePath);
        string? fileName = Path.GetFileName(result.OutputFilePath);
        Assert.Equal("Create_SavedGames_Folder.bat", fileName);
    }

    [Fact]
    public async Task ProcessAsync_WhenCustomFileNameProvided_SanitizesAndAppendsExtension()
    {
        var orchestrator = new GenerationOrchestrator();
        var request = new GenerationRequest
        {
            SourcePath = @"C:\Games\Cyberpunk\SavedGames",
            Format = OutputFormat.Executable,
            OutputFileName = "Setup_Cyberpunk_Save",
            OutputDirectory = _testDirectory
        };

        var result = await orchestrator.ProcessAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.OutputFilePath);
        string? fileName = Path.GetFileName(result.OutputFilePath);
        Assert.Equal("Setup_Cyberpunk_Save.exe", fileName);
    }

    [Fact]
    public async Task ProcessAsync_WhenCustomFileNameHasDifferentExtension_ReplacesWithCorrectExtension()
    {
        var orchestrator = new GenerationOrchestrator();
        var request = new GenerationRequest
        {
            SourcePath = @"C:\Games\Cyberpunk\SavedGames",
            Format = OutputFormat.Executable,
            OutputFileName = "MyScript.bat", // user entered .bat, but wants .exe
            OutputDirectory = _testDirectory
        };

        var result = await orchestrator.ProcessAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.OutputFilePath);
        string? fileName = Path.GetFileName(result.OutputFilePath);
        Assert.Equal("MyScript.exe", fileName);
    }

    [Fact]
    public async Task ProcessAsync_WhenDynamicVariablesDisabled_PreservesLiteralPath()
    {
        var orchestrator = new GenerationOrchestrator();
        var request = new GenerationRequest
        {
            SourcePath = @"C:\Users\Player\AppData\Local\Game\Saves",
            Format = OutputFormat.Batch,
            UseDynamicVariables = false,
            OutputDirectory = _testDirectory
        };

        var result = await orchestrator.ProcessAsync(request);

        Assert.True(result.Success);
        Assert.Equal(@"C:\Users\Player\AppData\Local\Game\Saves", result.NormalizedTargetPath);
    }

    [Fact]
    public async Task ProcessAsync_WhenOutputDirectoryNull_DefaultsToDesktopDirectory()
    {
        // Mock generators to prevent writing real files on user's real Desktop during unit tests
        var mockBatchGen = new MockBatchGenerator();
        var orchestrator = new GenerationOrchestrator(batchGenerator: mockBatchGen);

        var request = new GenerationRequest
        {
            SourcePath = @"C:\Games\Saves",
            Format = OutputFormat.Batch,
            OutputDirectory = null // should default to Desktop
        };

        var result = await orchestrator.ProcessAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.OutputFilePath);
        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        Assert.StartsWith(desktop, result.OutputFilePath);
    }

    private sealed class MockBatchGenerator : IBatchGenerator
    {
        public string GenerateScriptContent(BatchGenerationOptions options) => "REM Mock";

        public Task<string> WriteToFileAsync(string destinationFilePath, BatchGenerationOptions options)
        {
            return Task.FromResult(destinationFilePath);
        }
    }
}
