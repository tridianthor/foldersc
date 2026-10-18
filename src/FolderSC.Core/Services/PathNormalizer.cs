using System.Text.RegularExpressions;
using FolderSC.Core.Models;

namespace FolderSC.Core.Services;

/// <summary>
/// Default implementation of <see cref="IPathNormalizer"/> that standardizes paths and translates
/// user-specific directories to dynamic environment variables (%LOCALAPPDATA%, %APPDATA%, %USERPROFILE%).
/// </summary>
public partial class PathNormalizer : IPathNormalizer
{
    private static readonly Regex LocalAppDataRegex = CreateLocalAppDataRegex();
    private static readonly Regex AppDataRoamingRegex = CreateAppDataRoamingRegex();
    private static readonly Regex SavedGamesRegex = CreateSavedGamesRegex();
    private static readonly Regex DocumentsRegex = CreateDocumentsRegex();
    private static readonly Regex UserProfileRegex = CreateUserProfileRegex();

    public NormalizedPathResult Normalize(string rawPath, bool useDynamicVariables = true)
    {
        if (string.IsNullOrWhiteSpace(rawPath))
        {
            throw new ArgumentException("Path cannot be empty or whitespace.", nameof(rawPath));
        }

        string cleaned = SanitizePath(rawPath);
        string originalCleaned = cleaned;

        bool hasDynamicVariables = false;

        if (useDynamicVariables)
        {
            cleaned = ApplyDynamicVariables(cleaned, out bool applied);
            hasDynamicVariables = applied || cleaned.Contains('%');
        }

        string leafDirectory = DetermineLeafDirectory(cleaned);
        string suggestedFileName = $"Create_{SanitizeFileName(leafDirectory)}_Folder";

        return new NormalizedPathResult
        {
            OriginalPath = originalCleaned,
            NormalizedPath = cleaned,
            HasDynamicVariables = hasDynamicVariables,
            LeafDirectoryName = leafDirectory,
            SuggestedFileName = suggestedFileName
        };
    }

    private static string SanitizePath(string path)
    {
        string trimmed = path.Trim();

        // Strip surrounding quotes if present
        if ((trimmed.StartsWith('"') && trimmed.EndsWith('"')) ||
            (trimmed.StartsWith('\'') && trimmed.EndsWith('\'')))
        {
            trimmed = trimmed[1..^1].Trim();
        }

        // Convert forward slashes to backslashes
        trimmed = trimmed.Replace('/', '\\');

        // Normalize multiple consecutive backslashes, preserving leading double backslash for UNC paths
        bool isUnc = trimmed.StartsWith(@"\\", StringComparison.Ordinal);
        trimmed = Regex.Replace(trimmed, @"\\+", @"\");
        if (isUnc && !trimmed.StartsWith(@"\\", StringComparison.Ordinal))
        {
            trimmed = @"\" + trimmed;
        }

        // Remove trailing backslash unless it's a drive root (e.g. "C:\" or "\")
        if (trimmed.EndsWith('\\') && trimmed.Length > 3)
        {
            trimmed = trimmed.TrimEnd('\\');
        }

        return trimmed;
    }

    private static string ApplyDynamicVariables(string path, out bool applied)
    {
        applied = false;

        // 1. %LOCALAPPDATA%
        if (LocalAppDataRegex.IsMatch(path))
        {
            applied = true;
            return LocalAppDataRegex.Replace(path, @"%LOCALAPPDATA%${rest}");
        }

        // Check against current system environment LocalAppData
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (!string.IsNullOrEmpty(localAppData) && path.StartsWith(localAppData, StringComparison.OrdinalIgnoreCase))
        {
            applied = true;
            return @"%LOCALAPPDATA%" + path[localAppData.Length..];
        }

        // 2. %APPDATA% (Roaming)
        if (AppDataRoamingRegex.IsMatch(path))
        {
            applied = true;
            return AppDataRoamingRegex.Replace(path, @"%APPDATA%${rest}");
        }

        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (!string.IsNullOrEmpty(appData) && path.StartsWith(appData, StringComparison.OrdinalIgnoreCase))
        {
            applied = true;
            return @"%APPDATA%" + path[appData.Length..];
        }

        // 3. %USERPROFILE%\Saved Games
        if (SavedGamesRegex.IsMatch(path))
        {
            applied = true;
            return SavedGamesRegex.Replace(path, @"%USERPROFILE%\Saved Games${rest}");
        }

        // 4. %USERPROFILE%\Documents
        if (DocumentsRegex.IsMatch(path))
        {
            applied = true;
            return DocumentsRegex.Replace(path, @"%USERPROFILE%\Documents${rest}");
        }

        string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (!string.IsNullOrEmpty(documents) && path.StartsWith(documents, StringComparison.OrdinalIgnoreCase))
        {
            applied = true;
            return @"%USERPROFILE%\Documents" + path[documents.Length..];
        }

        // 5. %USERPROFILE% generic
        if (UserProfileRegex.IsMatch(path))
        {
            applied = true;
            return UserProfileRegex.Replace(path, @"%USERPROFILE%${rest}");
        }

        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrEmpty(userProfile) && path.StartsWith(userProfile, StringComparison.OrdinalIgnoreCase))
        {
            applied = true;
            return @"%USERPROFILE%" + path[userProfile.Length..];
        }

        return path;
    }

    private static string DetermineLeafDirectory(string path)
    {
        string trimmed = path.TrimEnd('\\');
        int lastSlash = trimmed.LastIndexOf('\\');

        if (lastSlash >= 0 && lastSlash < trimmed.Length - 1)
        {
            return trimmed[(lastSlash + 1)..];
        }

        // If it's a drive root like "C:" or "C:\"
        if (trimmed.EndsWith(':') && trimmed.Length == 2)
        {
            return trimmed[0..1];
        }

        return string.IsNullOrEmpty(trimmed) ? "Folder" : trimmed;
    }

    private static string SanitizeFileName(string name)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(name.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "Folder" : sanitized;
    }

    [GeneratedRegex(@"^[A-Za-z]:\\Users\\[^\\]+\\AppData\\Local(?<rest>\\.*)?$", RegexOptions.IgnoreCase)]
    private static partial Regex CreateLocalAppDataRegex();

    [GeneratedRegex(@"^[A-Za-z]:\\Users\\[^\\]+\\AppData\\Roaming(?<rest>\\.*)?$", RegexOptions.IgnoreCase)]
    private static partial Regex CreateAppDataRoamingRegex();

    [GeneratedRegex(@"^[A-Za-z]:\\Users\\[^\\]+\\Saved Games(?<rest>\\.*)?$", RegexOptions.IgnoreCase)]
    private static partial Regex CreateSavedGamesRegex();

    [GeneratedRegex(@"^[A-Za-z]:\\Users\\[^\\]+\\Documents(?<rest>\\.*)?$", RegexOptions.IgnoreCase)]
    private static partial Regex CreateDocumentsRegex();

    [GeneratedRegex(@"^[A-Za-z]:\\Users\\[^\\]+(?<rest>\\.*)?$", RegexOptions.IgnoreCase)]
    private static partial Regex CreateUserProfileRegex();
}
