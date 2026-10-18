using System.IO;
using System.Windows;
using FolderSC.UI.ViewModels;

namespace FolderSC.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    public static string ResolveDropPath(string[]? droppedPaths)
    {
        if (droppedPaths == null || droppedPaths.Length == 0)
        {
            return string.Empty;
        }

        string first = droppedPaths[0];
        if (Directory.Exists(first))
        {
            return first;
        }

        if (File.Exists(first))
        {
            return Path.GetDirectoryName(first) ?? first;
        }

        return first;
    }

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
            string resolved = ResolveDropPath(files);

            if (!string.IsNullOrWhiteSpace(resolved) && DataContext is MainViewModel vm)
            {
                vm.SourcePath = resolved;
            }
        }
    }
}