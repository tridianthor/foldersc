using FolderSC.Core.Models;

namespace FolderSC.Core.Services;

/// <summary>
/// Service responsible for normalizing Windows folder paths and abstracting user-specific directories
/// into cross-device environment variables (%LOCALAPPDATA%, %APPDATA%, %USERPROFILE%).
/// </summary>
public interface IPathNormalizer
{
    /// <summary>
    /// Normalizes a given raw folder path and optionally applies dynamic environment variable translations.
    /// </summary>
    /// <param name="rawPath">The input path (e.g. copied from Explorer or typed by user).</param>
    /// <param name="useDynamicVariables">Whether to replace user-specific paths with environment variables.</param>
    /// <returns>A <see cref="NormalizedPathResult"/> containing normalized path details.</returns>
    NormalizedPathResult Normalize(string rawPath, bool useDynamicVariables = true);
}
