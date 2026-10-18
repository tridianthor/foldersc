using System.Diagnostics;
using System.Runtime.InteropServices;
using FolderSC.Core.Services;

namespace FolderSC.Stub;

public static class Program
{
    private const uint MbIconInformation = 0x00000040u;
    private const uint MbIconError = 0x00000010u;
    private const uint MbIconWarning = 0x00000030u;

    [DllImport("user32.dll", EntryPoint = "MessageBoxW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);

    public static int Main(string[] args)
    {
        bool silent = args.Contains("--silent") ||
                      Environment.GetEnvironmentVariable("FOLDERSC_SILENT") == "1";

        string? currentExe = Environment.ProcessPath;
        if (string.IsNullOrEmpty(currentExe) || !File.Exists(currentExe))
        {
            if (!silent)
            {
                ShowMessageBox("Unable to locate current executable path.", "FolderSC - Error", MbIconError);
            }
            return 1;
        }

        byte[] exeBytes;
        try
        {
            using var fs = new FileStream(currentExe, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            exeBytes = new byte[fs.Length];
            fs.ReadExactly(exeBytes);
        }
        catch (Exception ex)
        {
            if (!silent)
            {
                ShowMessageBox($"Failed to read executable file:\n{ex.Message}", "FolderSC - Error", MbIconError);
            }
            return 1;
        }

        var packer = new PayloadPacker();
        if (!packer.TryUnpack(exeBytes, out var payload))
        {
            if (!silent)
            {
                ShowMessageBox(
                    "No FolderSC payload found embedded in this executable.\n\nPlease generate a configured folder runner from the FolderSC app.",
                    "FolderSC - Missing Payload",
                    MbIconWarning);
            }
            return 1;
        }

        string targetPath = Environment.ExpandEnvironmentVariables(payload.TargetPath.Trim());

        try
        {
            bool alreadyExists = Directory.Exists(targetPath);
            Directory.CreateDirectory(targetPath);

            if (payload.OpenInExplorer)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"\"{targetPath}\"",
                        UseShellExecute = true
                    });
                }
                catch
                {
                    // Non-fatal if explorer fails to launch
                }
            }

            if (!silent)
            {
                string title = string.IsNullOrWhiteSpace(payload.Title)
                    ? "FolderSC - Folder Created"
                    : $"FolderSC - {payload.Title.Trim()}";

                string message = alreadyExists
                    ? $"Directory already exists:\n\n{targetPath}"
                    : $"Successfully created directory:\n\n{targetPath}";

                ShowMessageBox(message, title, MbIconInformation);
            }

            return 0;
        }
        catch (Exception ex)
        {
            if (!silent)
            {
                ShowMessageBox($"Failed to create directory:\n\n{targetPath}\n\nError: {ex.Message}", "FolderSC - Error", MbIconError);
            }
            return 2;
        }
    }

    private static void ShowMessageBox(string text, string title, uint type)
    {
        MessageBoxW(IntPtr.Zero, text, title, type);
    }
}
