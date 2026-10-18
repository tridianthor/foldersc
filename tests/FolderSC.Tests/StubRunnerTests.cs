using System.Diagnostics;
using FolderSC.Core;
using FolderSC.Core.Models;
using FolderSC.Core.Services;

namespace FolderSC.Tests;

public class StubRunnerTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly IPayloadPacker _packer = new PayloadPacker();

    public StubRunnerTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "FolderSC_StubTests_" + Guid.NewGuid().ToString("N"));
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

    private static byte[] GetStubBinary()
    {
        var assembly = typeof(FolderSCConstants).Assembly;
        using var stream = assembly.GetManifestResourceStream("FolderSC.Core.Resources.StubRunner.bin");
        if (stream != null)
        {
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        string localPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "src", "FolderSC.Stub", "publish", "FolderSC.Stub.exe"));
        if (File.Exists(localPath))
        {
            return File.ReadAllBytes(localPath);
        }

        throw new FileNotFoundException("StubRunner binary not found in embedded resources or publish folder.");
    }

    [Fact]
    public async Task RunStub_WithoutPayload_ExitsWithErrorCode1()
    {
        byte[] baseStubBytes = GetStubBinary();
        string testExePath = Path.Combine(_testDirectory, "UnconfiguredStub.exe");
        await File.WriteAllBytesAsync(testExePath, baseStubBytes);

        // Run unconfigured stub with --silent
        var psi = new ProcessStartInfo
        {
            FileName = testExePath,
            Arguments = "--silent",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        Assert.NotNull(process);
        await process.WaitForExitAsync();

        Assert.Equal(1, process.ExitCode);
    }

    [Fact]
    public async Task RunStub_WithValidAppendedPayload_CreatesDirectoryAndExitsWithZero()
    {
        byte[] baseStubBytes = GetStubBinary();
        string configuredExePath = Path.Combine(_testDirectory, "ConfiguredRunner.exe");
        string targetDirectoryToCreate = Path.Combine(_testDirectory, "Nested1", "Nested2", "TargetSaveGame");

        // 1. Pack payload
        var payload = new RunnerPayload
        {
            TargetPath = targetDirectoryToCreate,
            OpenInExplorer = false,
            Title = "TestRunner"
        };
        byte[] overlayBytes = _packer.Pack(payload);

        // 2. Write configured executable (stub + overlay)
        using (var fs = new FileStream(configuredExePath, FileMode.Create, FileAccess.Write))
        {
            await fs.WriteAsync(baseStubBytes);
            await fs.WriteAsync(overlayBytes);
        }

        Assert.False(Directory.Exists(targetDirectoryToCreate));

        // 3. Run configured executable with --silent
        var psi = new ProcessStartInfo
        {
            FileName = configuredExePath,
            Arguments = "--silent",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        Assert.NotNull(process);
        await process.WaitForExitAsync();

        Assert.Equal(0, process.ExitCode);
        Assert.True(Directory.Exists(targetDirectoryToCreate));

        // 4. Run again (idempotent verification)
        using var process2 = Process.Start(psi);
        Assert.NotNull(process2);
        await process2.WaitForExitAsync();

        Assert.Equal(0, process2.ExitCode);
    }
}
