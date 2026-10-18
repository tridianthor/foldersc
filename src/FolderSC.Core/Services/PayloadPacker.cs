using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using FolderSC.Core.Models;

namespace FolderSC.Core.Services;

[JsonSerializable(typeof(RunnerPayload))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    WriteIndented = false)]
internal partial class RunnerPayloadJsonContext : JsonSerializerContext
{
}

/// <summary>
/// Implements <see cref="IPayloadPacker"/> using the binary FSC1 format:
/// [Magic: 'F','S','C','1' (4B)][Length: Int32 (4B)][UTF-8 JSON bytes (NB)].
/// </summary>
public class PayloadPacker : IPayloadPacker
{
    public static readonly byte[] MagicMarker = [(byte)'F', (byte)'S', (byte)'C', (byte)'1'];
    private const int HeaderSize = 8; // 4 bytes magic + 4 bytes length

    public byte[] Pack(RunnerPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        if (string.IsNullOrWhiteSpace(payload.TargetPath))
        {
            throw new ArgumentException("Target path cannot be empty or whitespace.", nameof(payload));
        }

        byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(payload, RunnerPayloadJsonContext.Default.RunnerPayload);
        byte[] result = new byte[HeaderSize + jsonBytes.Length];

        // 1. Magic bytes
        Buffer.BlockCopy(MagicMarker, 0, result, 0, MagicMarker.Length);

        // 2. Length (little-endian Int32)
        byte[] lengthBytes = BitConverter.GetBytes(jsonBytes.Length);
        Buffer.BlockCopy(lengthBytes, 0, result, 4, lengthBytes.Length);

        // 3. Payload JSON
        Buffer.BlockCopy(jsonBytes, 0, result, HeaderSize, jsonBytes.Length);

        return result;
    }

    public RunnerPayload Unpack(byte[] binaryData)
    {
        if (binaryData == null || binaryData.Length < HeaderSize)
        {
            throw new InvalidDataException("Binary data is null or smaller than the required header size.");
        }

        if (!TryUnpack(binaryData, out var payload))
        {
            throw new InvalidDataException("Invalid, missing, or corrupted FolderSC payload in binary stream.");
        }

        return payload;
    }

    public bool TryUnpack(byte[]? binaryData, [NotNullWhen(true)] out RunnerPayload? payload)
    {
        payload = null;

        if (binaryData == null || binaryData.Length < HeaderSize)
        {
            return false;
        }

        int markerOffset = -1;

        // Check if magic bytes are directly at the start
        if (IsMagicAt(binaryData, 0))
        {
            markerOffset = 0;
        }
        else
        {
            // Search backwards from the end of the file for the magic marker
            for (int i = binaryData.Length - HeaderSize; i >= 0; i--)
            {
                if (IsMagicAt(binaryData, i))
                {
                    markerOffset = i;
                    break;
                }
            }
        }

        if (markerOffset < 0)
        {
            return false;
        }

        int payloadLength = BitConverter.ToInt32(binaryData, markerOffset + 4);

        if (payloadLength <= 0 || markerOffset + HeaderSize + payloadLength > binaryData.Length)
        {
            return false;
        }

        try
        {
            var jsonSpan = new ReadOnlySpan<byte>(binaryData, markerOffset + HeaderSize, payloadLength);
            var deserialized = JsonSerializer.Deserialize(jsonSpan, RunnerPayloadJsonContext.Default.RunnerPayload);

            if (deserialized == null || string.IsNullOrWhiteSpace(deserialized.TargetPath))
            {
                return false;
            }

            payload = deserialized;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool IsMagicAt(byte[] data, int offset)
    {
        if (offset + MagicMarker.Length > data.Length)
        {
            return false;
        }

        return data[offset] == MagicMarker[0] &&
               data[offset + 1] == MagicMarker[1] &&
               data[offset + 2] == MagicMarker[2] &&
               data[offset + 3] == MagicMarker[3];
    }
}
