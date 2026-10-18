namespace FolderSC.Core.Services;

/// <summary>
/// Loads the pre-compiled runner stub executable from embedded assembly resources.
/// </summary>
public class EmbeddedStubBinaryProvider : IStubBinaryProvider
{
    private const string ResourceName = "FolderSC.Core.Resources.StubRunner.bin";
    private static byte[]? _cachedBytes;
    private static readonly object SyncLock = new();

    public byte[] GetStubBytes()
    {
        if (_cachedBytes != null)
        {
            return _cachedBytes;
        }

        lock (SyncLock)
        {
            if (_cachedBytes != null)
            {
                return _cachedBytes;
            }

            var assembly = typeof(EmbeddedStubBinaryProvider).Assembly;
            using var stream = assembly.GetManifestResourceStream(ResourceName);

            if (stream == null)
            {
                throw new FileNotFoundException(
                    $"Embedded runner resource '{ResourceName}' was not found in assembly '{assembly.FullName}'. " +
                    "Ensure StubRunner.bin is set as an EmbeddedResource.");
            }

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            _cachedBytes = ms.ToArray();

            return _cachedBytes;
        }
    }
}
