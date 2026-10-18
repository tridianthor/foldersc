using System.Diagnostics.CodeAnalysis;
using FolderSC.Core.Models;

namespace FolderSC.Core.Services;

/// <summary>
/// Service responsible for packing and unpacking runner payloads into binary overlays using the FSC1 format.
/// </summary>
public interface IPayloadPacker
{
    /// <summary>
    /// Serializes a <see cref="RunnerPayload"/> into binary overlay format: [FSC1 (4B)][Length (4B)][JSON bytes].
    /// </summary>
    /// <param name="payload">Payload to serialize.</param>
    /// <returns>Packed byte array ready to append to an executable.</returns>
    byte[] Pack(RunnerPayload payload);

    /// <summary>
    /// Unpacks a <see cref="RunnerPayload"/> from a binary array (either exact payload or executable with appended overlay).
    /// </summary>
    /// <param name="binaryData">Byte array containing the FSC1 overlay.</param>
    /// <returns>Deserialized <see cref="RunnerPayload"/>.</returns>
    /// <exception cref="InvalidDataException">Thrown if payload is missing or corrupted.</exception>
    RunnerPayload Unpack(byte[] binaryData);

    /// <summary>
    /// Attempts to unpack a <see cref="RunnerPayload"/> from binary data without throwing on format errors.
    /// </summary>
    /// <param name="binaryData">Byte array containing the FSC1 overlay.</param>
    /// <param name="payload">The deserialized payload if successful.</param>
    /// <returns>True if successfully unpacked; false otherwise.</returns>
    bool TryUnpack(byte[]? binaryData, [NotNullWhen(true)] out RunnerPayload? payload);
}
