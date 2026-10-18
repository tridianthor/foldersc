using FolderSC.Core.Models;
using FolderSC.Core.Services;

namespace FolderSC.Tests;

public class PayloadPackerTests
{
    private readonly IPayloadPacker _packer = new PayloadPacker();

    [Fact]
    public void Pack_WhenPayloadIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _packer.Pack(null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Pack_WhenTargetPathIsNullOrEmpty_ThrowsArgumentException(string? invalidPath)
    {
        var payload = new RunnerPayload
        {
            TargetPath = invalidPath!
        };

        Assert.Throws<ArgumentException>(() => _packer.Pack(payload));
    }

    [Fact]
    public void Pack_StartsWithMagicBytesFSC1_AndHasValidStructure()
    {
        var payload = new RunnerPayload
        {
            TargetPath = @"%LOCALAPPDATA%\GameStudio\SaveGames",
            OpenInExplorer = true,
            Title = "GameStudio"
        };

        byte[] packed = _packer.Pack(payload);

        Assert.NotNull(packed);
        Assert.True(packed.Length > 8);

        // Check magic bytes: 'F', 'S', 'C', '1' -> 0x46, 0x53, 0x43, 0x31
        Assert.Equal((byte)'F', packed[0]);
        Assert.Equal((byte)'S', packed[1]);
        Assert.Equal((byte)'C', packed[2]);
        Assert.Equal((byte)'1', packed[3]);

        int payloadLength = BitConverter.ToInt32(packed, 4);
        Assert.Equal(packed.Length - 8, payloadLength);
    }

    [Fact]
    public void Unpack_RoundTrip_SuccessfullyRestoresPayload()
    {
        var original = new RunnerPayload
        {
            TargetPath = @"%LOCALAPPDATA%\CDProjektRed\Cyberpunk 2077",
            OpenInExplorer = true,
            Title = "Cyberpunk 2077",
            CreatedAtUtc = new DateTime(2026, 10, 18, 12, 0, 0, DateTimeKind.Utc)
        };

        byte[] packed = _packer.Pack(original);
        RunnerPayload unpacked = _packer.Unpack(packed);

        Assert.NotNull(unpacked);
        Assert.Equal(original.TargetPath, unpacked.TargetPath);
        Assert.Equal(original.OpenInExplorer, unpacked.OpenInExplorer);
        Assert.Equal(original.Title, unpacked.Title);
        Assert.Equal(original.CreatedAtUtc, unpacked.CreatedAtUtc);
    }

    [Fact]
    public void Unpack_WhenAppendedToSimulatedExeFile_FindsAndExtractsPayload()
    {
        var original = new RunnerPayload
        {
            TargetPath = @"%USERPROFILE%\Saved Games\Hades II",
            OpenInExplorer = false,
            Title = "Hades II"
        };

        byte[] payloadBytes = _packer.Pack(original);

        // Simulate an executable file (dummy binary prefix of 1024 bytes)
        byte[] dummyExe = new byte[1024];
        new Random(42).NextBytes(dummyExe);

        // Combine dummy executable + payload overlay
        byte[] combined = new byte[dummyExe.Length + payloadBytes.Length];
        Buffer.BlockCopy(dummyExe, 0, combined, 0, dummyExe.Length);
        Buffer.BlockCopy(payloadBytes, 0, combined, dummyExe.Length, payloadBytes.Length);

        RunnerPayload unpacked = _packer.Unpack(combined);

        Assert.NotNull(unpacked);
        Assert.Equal(original.TargetPath, unpacked.TargetPath);
        Assert.Equal(original.OpenInExplorer, unpacked.OpenInExplorer);
        Assert.Equal(original.Title, unpacked.Title);
    }

    [Fact]
    public void Unpack_WhenBinaryDataNullOrTooShort_ThrowsInvalidDataException()
    {
        Assert.Throws<InvalidDataException>(() => _packer.Unpack(null!));
        Assert.Throws<InvalidDataException>(() => _packer.Unpack(new byte[4]));
    }

    [Fact]
    public void Unpack_WhenMagicBytesMissing_ThrowsInvalidDataException()
    {
        byte[] corrupted = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x00, 0x00, 0x00, 0x7B, 0x7D };
        Assert.Throws<InvalidDataException>(() => _packer.Unpack(corrupted));
    }

    [Fact]
    public void Unpack_WhenLengthCorrupted_ThrowsInvalidDataException()
    {
        // "FSC1" followed by impossible length (999999 bytes)
        byte[] corrupted = new byte[] { (byte)'F', (byte)'S', (byte)'C', (byte)'1', 0xFF, 0xFF, 0x0F, 0x00, 0x7B, 0x7D };
        Assert.Throws<InvalidDataException>(() => _packer.Unpack(corrupted));
    }

    [Fact]
    public void Unpack_WhenJsonCorrupted_ThrowsInvalidDataException()
    {
        byte[] corruptedJson = new byte[] { (byte)'F', (byte)'S', (byte)'C', (byte)'1', 4, 0, 0, 0, (byte)'b', (byte)'a', (byte)'d', (byte)'!' };
        Assert.Throws<InvalidDataException>(() => _packer.Unpack(corruptedJson));
    }

    [Fact]
    public void TryUnpack_WithValidData_ReturnsTrueAndPayload()
    {
        var original = new RunnerPayload
        {
            TargetPath = @"D:\Games\Saves",
            OpenInExplorer = true
        };

        byte[] packed = _packer.Pack(original);
        bool success = _packer.TryUnpack(packed, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(@"D:\Games\Saves", result.TargetPath);
    }

    [Fact]
    public void TryUnpack_WithInvalidData_ReturnsFalseAndNull()
    {
        byte[] invalidData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        bool success = _packer.TryUnpack(invalidData, out var result);

        Assert.False(success);
        Assert.Null(result);
    }
}
