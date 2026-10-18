namespace FolderSC.Core.Services;

/// <summary>
/// Provides raw binary bytes of the pre-compiled runner stub executable.
/// </summary>
public interface IStubBinaryProvider
{
    /// <summary>
    /// Gets the byte array of the standalone runner stub executable.
    /// </summary>
    /// <returns>Byte array of the executable stub.</returns>
    byte[] GetStubBytes();
}
