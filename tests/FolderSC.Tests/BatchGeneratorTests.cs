using System.Diagnostics;
using FolderSC.Core.Models;
using FolderSC.Core.Services;

namespace FolderSC.Tests;

public class BatchGeneratorTests : IDisposable
{
    private readonly IBatchGenerator _generator = new BatchGenerator();
    private readonly string _testDirectory;

    public BatchGeneratorTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "FolderSC_BatchTests_" + Guid.NewGuid().ToString("N"));
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
            // Ignore cleanup failures in temp directory
        }
    }

    [Fact]
    public void GenerateScriptContent_WhenOptionsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _generator.GenerateScriptContent(null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerateScriptContent_WhenTargetPathIsNullOrWhitespace_ThrowsArgumentException(string? invalidPath)
    {
        var options = new BatchGenerationOptions
        {
            TargetPath = invalidPath!
        };

        Assert.Throws<ArgumentException>(() => _generator.GenerateScriptContent(options));
    }

    [Fact]
    public void GenerateScriptContent_ContainsTargetDirectoryAndMkdirCommand()
    {
        var options = new BatchGenerationOptions
        {
            TargetPath = @"%LOCALAPPDATA%\TestGame\SaveGames"
        };

        string content = _generator.GenerateScriptContent(options);

        Assert.Contains(@"set ""TARGET_PATH=%LOCALAPPDATA%\TestGame\SaveGames""", content);
        Assert.Contains(@"mkdir ""%TARGET_PATH%""", content);
        Assert.Contains(@"if not exist ""%TARGET_PATH%""", content);
    }

    [Fact]
    public void GenerateScriptContent_WhenOpenExplorerTrue_IncludesExplorerCommand()
    {
        var options = new BatchGenerationOptions
        {
            TargetPath = @"C:\Games\Saves",
            OpenExplorerOnCompletion = true
        };

        string content = _generator.GenerateScriptContent(options);

        Assert.Contains(@"start """" explorer.exe ""%TARGET_PATH%""", content);
    }

    [Fact]
    public void GenerateScriptContent_WhenOpenExplorerFalse_OmitsExplorerCommand()
    {
        var options = new BatchGenerationOptions
        {
            TargetPath = @"C:\Games\Saves",
            OpenExplorerOnCompletion = false
        };

        string content = _generator.GenerateScriptContent(options);

        Assert.DoesNotContain(@"start """" explorer.exe", content);
    }

    [Fact]
    public void GenerateScriptContent_WhenPauseTrue_IncludesPauseCommand()
    {
        var options = new BatchGenerationOptions
        {
            TargetPath = @"C:\Games\Saves",
            PauseOnCompletion = true
        };

        string content = _generator.GenerateScriptContent(options);

        Assert.Contains("pause >nul", content);
        Assert.Contains("Press any key to exit", content);
    }

    [Fact]
    public void GenerateScriptContent_WhenPauseFalse_OmitsPauseCommand()
    {
        var options = new BatchGenerationOptions
        {
            TargetPath = @"C:\Games\Saves",
            PauseOnCompletion = false
        };

        string content = _generator.GenerateScriptContent(options);

        Assert.DoesNotContain("pause >nul", content);
        Assert.DoesNotContain("Press any key to exit", content);
    }

    [Fact]
    public void GenerateScriptContent_IncludesCustomTitleWhenProvided()
    {
        var options = new BatchGenerationOptions
        {
            TargetPath = @"C:\Games\Saves",
            Title = "Cyberpunk 2077"
        };

        string content = _generator.GenerateScriptContent(options);

        Assert.Contains("Cyberpunk 2077", content);
    }

    [Fact]
    public async Task WriteToFileAsync_CreatesFileWithCorrectContent()
    {
        string scriptPath = Path.Combine(_testDirectory, "test_script.bat");
        var options = new BatchGenerationOptions
        {
            TargetPath = @"%LOCALAPPDATA%\Studio\Game\Save",
            PauseOnCompletion = false,
            OpenExplorerOnCompletion = false
        };

        string resultPath = await _generator.WriteToFileAsync(scriptPath, options);

        Assert.True(File.Exists(resultPath));
        string fileContent = await File.ReadAllTextAsync(resultPath);
        Assert.Contains(@"%LOCALAPPDATA%\Studio\Game\Save", fileContent);
    }

    [Fact]
    public async Task ExecuteGeneratedBatchScript_InSandbox_PhysicallyCreatesTargetDirectory()
    {
        string targetToCreate = Path.Combine(_testDirectory, "SubDir1", "SubDir2", "DeepSaveDir");
        string scriptPath = Path.Combine(_testDirectory, "create_deep_folder.bat");

        var options = new BatchGenerationOptions
        {
            TargetPath = targetToCreate,
            PauseOnCompletion = false,
            OpenExplorerOnCompletion = false
        };

        await _generator.WriteToFileAsync(scriptPath, options);

        Assert.False(Directory.Exists(targetToCreate));

        // Execute batch file via cmd.exe
        var startInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c \"\"{scriptPath}\"\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        Assert.NotNull(process);
        await process.WaitForExitAsync();

        Assert.Equal(0, process.ExitCode);
        Assert.True(Directory.Exists(targetToCreate));
    }
}
