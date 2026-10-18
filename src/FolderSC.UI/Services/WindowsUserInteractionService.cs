using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace FolderSC.UI.Services;

/// <summary>
/// Standard Windows implementation of <see cref="IUserInteractionService"/> for WPF.
/// </summary>
public class WindowsUserInteractionService : IUserInteractionService
{
    public string? PickFolder(string? initialDirectory = null)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "FolderSC - Select Folder"
        };

        if (!string.IsNullOrWhiteSpace(initialDirectory) && Directory.Exists(initialDirectory))
        {
            dialog.InitialDirectory = initialDirectory;
        }

        bool? result = dialog.ShowDialog();
        return result == true ? dialog.FolderName : null;
    }

    public void OpenInExplorer(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;

        try
        {
            string argument = File.Exists(path) ? $"/select,\"{path}\"" : $"\"{path}\"";
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = argument,
                UseShellExecute = true
            });
        }
        catch
        {
            // Non-fatal
        }
    }

    public void CopyToClipboard(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        try
        {
            Clipboard.SetText(text);
        }
        catch
        {
            // Clipboard might be locked by another application
        }
    }
}
