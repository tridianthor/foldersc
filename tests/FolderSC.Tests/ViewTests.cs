using System.Globalization;
using System.Windows;
using FolderSC.Core.Models;
using FolderSC.UI.Converters;
using FolderSC.UI.ViewModels;

namespace FolderSC.Tests;

public class ViewTests
{
    [Fact]
    public void FormatToBooleanConverter_ConvertsEnumToBooleanCorrectly()
    {
        var converter = new FormatToBooleanConverter();

        // Convert Executable when parameter is "Executable"
        object result1 = converter.Convert(OutputFormat.Executable, typeof(bool), "Executable", CultureInfo.InvariantCulture);
        Assert.Equal(true, result1);

        // Convert Executable when parameter is "Batch"
        object result2 = converter.Convert(OutputFormat.Executable, typeof(bool), "Batch", CultureInfo.InvariantCulture);
        Assert.Equal(false, result2);

        // ConvertBack when isChecked is true for "Batch"
        object backResult = converter.ConvertBack(true, typeof(OutputFormat), "Batch", CultureInfo.InvariantCulture);
        Assert.Equal(OutputFormat.Batch, backResult);
    }

    [Fact]
    public void InvertedBooleanToVisibilityConverter_ConvertsCorrectly()
    {
        var converter = new InvertedBooleanToVisibilityConverter();

        Assert.Equal(Visibility.Collapsed, converter.Convert(true, typeof(Visibility), null, CultureInfo.InvariantCulture));
        Assert.Equal(Visibility.Visible, converter.Convert(false, typeof(Visibility), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void DragDropHelper_ExtractsFolderPathCorrectly()
    {
        string tempDir = Path.GetTempPath();
        string resolved = FolderSC.UI.MainWindow.ResolveDropPath(new[] { tempDir });
        Assert.Equal(tempDir, resolved);

        // Dropping a file inside a folder should resolve to its containing folder
        string tempFile = Path.Combine(tempDir, "sample_test_file.txt");
        File.WriteAllText(tempFile, "test");
        try
        {
            string fileResolved = FolderSC.UI.MainWindow.ResolveDropPath(new[] { tempFile });
            Assert.Equal(Path.GetDirectoryName(tempFile), fileResolved);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
