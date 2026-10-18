using FolderSC.Core.Models;
using FolderSC.Core.Services;

namespace FolderSC.Tests;

public class PathNormalizerTests
{
    private readonly IPathNormalizer _normalizer = new PathNormalizer();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Normalize_WhenPathIsNullOrEmpty_ThrowsArgumentException(string? invalidPath)
    {
        Assert.Throws<ArgumentException>(() => _normalizer.Normalize(invalidPath!));
    }

    [Fact]
    public void Normalize_LocalApplicationData_TranslatesToLocalAppData()
    {
        const string input = @"C:\Users\GamerUser\AppData\Local\GameStudio\GameTitle\Saved";
        var result = _normalizer.Normalize(input, useDynamicVariables: true);

        Assert.Equal(@"%LOCALAPPDATA%\GameStudio\GameTitle\Saved", result.NormalizedPath);
        Assert.True(result.HasDynamicVariables);
        Assert.Equal("Saved", result.LeafDirectoryName);
        Assert.Equal("Create_Saved_Folder", result.SuggestedFileName);
    }

    [Fact]
    public void Normalize_ApplicationDataRoaming_TranslatesToAppData()
    {
        const string input = @"C:\Users\GamerUser\AppData\Roaming\RetroArch\saves";
        var result = _normalizer.Normalize(input, useDynamicVariables: true);

        Assert.Equal(@"%APPDATA%\RetroArch\saves", result.NormalizedPath);
        Assert.True(result.HasDynamicVariables);
        Assert.Equal("saves", result.LeafDirectoryName);
        Assert.Equal("Create_saves_Folder", result.SuggestedFileName);
    }

    [Fact]
    public void Normalize_SavedGames_TranslatesToUserProfileSavedGames()
    {
        const string input = @"C:\Users\JohnDoe\Saved Games\CD Projekt Red\Cyberpunk 2077";
        var result = _normalizer.Normalize(input, useDynamicVariables: true);

        Assert.Equal(@"%USERPROFILE%\Saved Games\CD Projekt Red\Cyberpunk 2077", result.NormalizedPath);
        Assert.True(result.HasDynamicVariables);
        Assert.Equal("Cyberpunk 2077", result.LeafDirectoryName);
        Assert.Equal("Create_Cyberpunk 2077_Folder", result.SuggestedFileName);
    }

    [Fact]
    public void Normalize_Documents_TranslatesToUserProfileDocuments()
    {
        const string input = @"C:\Users\JohnDoe\Documents\My Games\Skyrim Special Edition";
        var result = _normalizer.Normalize(input, useDynamicVariables: true);

        Assert.Equal(@"%USERPROFILE%\Documents\My Games\Skyrim Special Edition", result.NormalizedPath);
        Assert.True(result.HasDynamicVariables);
        Assert.Equal("Skyrim Special Edition", result.LeafDirectoryName);
    }

    [Fact]
    public void Normalize_GenericUserProfileDirectory_TranslatesToUserProfile()
    {
        const string input = @"C:\Users\JohnDoe\.config\r2modmanPlus-local";
        var result = _normalizer.Normalize(input, useDynamicVariables: true);

        Assert.Equal(@"%USERPROFILE%\.config\r2modmanPlus-local", result.NormalizedPath);
        Assert.True(result.HasDynamicVariables);
        Assert.Equal("r2modmanPlus-local", result.LeafDirectoryName);
    }

    [Fact]
    public void Normalize_CustomDrivePath_PreservesVerbatim()
    {
        const string input = @"D:\Games\Emulators\Yuzu\nand\user\save";
        var result = _normalizer.Normalize(input, useDynamicVariables: true);

        Assert.Equal(@"D:\Games\Emulators\Yuzu\nand\user\save", result.NormalizedPath);
        Assert.False(result.HasDynamicVariables);
        Assert.Equal("save", result.LeafDirectoryName);
        Assert.Equal("Create_save_Folder", result.SuggestedFileName);
    }

    [Fact]
    public void Normalize_ForwardSlashesAndTrailingSlashes_NormalizesCorrectly()
    {
        const string input = "C:/Users/Gamer/AppData/Local/GameTitle/Saves/";
        var result = _normalizer.Normalize(input, useDynamicVariables: true);

        Assert.Equal(@"%LOCALAPPDATA%\GameTitle\Saves", result.NormalizedPath);
        Assert.Equal("Saves", result.LeafDirectoryName);
    }

    [Fact]
    public void Normalize_SurroundingQuotesAndSpaces_SanitizesProperly()
    {
        const string input = "  \"C:\\Users\\Gamer\\AppData\\Local\\GameTitle\\Saves\"  ";
        var result = _normalizer.Normalize(input, useDynamicVariables: true);

        Assert.Equal(@"%LOCALAPPDATA%\GameTitle\Saves", result.NormalizedPath);
    }

    [Fact]
    public void Normalize_WhenDynamicVariablesDisabled_KeepsLiteralPath()
    {
        const string input = @"C:\Users\GamerUser\AppData\Local\GameStudio\Saved";
        var result = _normalizer.Normalize(input, useDynamicVariables: false);

        Assert.Equal(@"C:\Users\GamerUser\AppData\Local\GameStudio\Saved", result.NormalizedPath);
        Assert.False(result.HasDynamicVariables);
        Assert.Equal("Saved", result.LeafDirectoryName);
    }

    [Fact]
    public void Normalize_CaseInsensitiveMatch_HandlesMixedCasing()
    {
        const string input = @"c:\users\gameruser\appdata\local\mygame";
        var result = _normalizer.Normalize(input, useDynamicVariables: true);

        Assert.Equal(@"%LOCALAPPDATA%\mygame", result.NormalizedPath);
        Assert.True(result.HasDynamicVariables);
    }

    [Fact]
    public void Normalize_AlreadyParameterizedPath_PreservesAndDetectsVariables()
    {
        const string input = @"%LOCALAPPDATA%\HadesII\SavedGames";
        var result = _normalizer.Normalize(input, useDynamicVariables: true);

        Assert.Equal(@"%LOCALAPPDATA%\HadesII\SavedGames", result.NormalizedPath);
        Assert.True(result.HasDynamicVariables);
        Assert.Equal("SavedGames", result.LeafDirectoryName);
    }

    [Fact]
    public void Normalize_MultipleConsecutiveBackslashes_CollapsesToSingleBackslash()
    {
        const string input = @"C:\\Users\\\User\\\\AppData\\Local\\Game";
        var result = _normalizer.Normalize(input, useDynamicVariables: true);

        Assert.Equal(@"%LOCALAPPDATA%\Game", result.NormalizedPath);
    }

    [Fact]
    public void Normalize_DriveRoot_HandlesLeafAndFilenameGracefully()
    {
        const string input = @"C:\";
        var result = _normalizer.Normalize(input, useDynamicVariables: true);

        Assert.Equal(@"C:\", result.NormalizedPath);
        Assert.Equal("C", result.LeafDirectoryName);
        Assert.Equal("Create_C_Folder", result.SuggestedFileName);
    }

    [Fact]
    public void Normalize_UnicodeCharacters_PreservesCorrectly()
    {
        const string input = @"C:\Users\Player\AppData\Local\ゲーム\セーブ";
        var result = _normalizer.Normalize(input, useDynamicVariables: true);

        Assert.Equal(@"%LOCALAPPDATA%\ゲーム\セーブ", result.NormalizedPath);
        Assert.Equal("セーブ", result.LeafDirectoryName);
        Assert.Equal("Create_セーブ_Folder", result.SuggestedFileName);
    }
}
