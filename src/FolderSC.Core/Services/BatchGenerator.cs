using System.Text;
using FolderSC.Core.Models;

namespace FolderSC.Core.Services;

/// <summary>
/// Implements <see cref="IBatchGenerator"/> to create robust, portable Windows Batch scripts
/// for recreating folder structures across devices.
/// </summary>
public class BatchGenerator : IBatchGenerator
{
    public string GenerateScriptContent(BatchGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.TargetPath))
        {
            throw new ArgumentException("Target path cannot be empty or whitespace.", nameof(options));
        }

        var sb = new StringBuilder();

        string bannerTitle = string.IsNullOrWhiteSpace(options.Title)
            ? "FolderSC - Directory Setup"
            : $"FolderSC - Setup: {options.Title.Trim()}";

        sb.AppendLine("@echo off");
        sb.AppendLine($"title {bannerTitle}");
        sb.AppendLine();
        sb.AppendLine("echo ========================================================");
        sb.AppendLine($"echo   {bannerTitle}");
        sb.AppendLine("echo ========================================================");
        sb.AppendLine("echo.");
        sb.AppendLine($"set \"TARGET_PATH={options.TargetPath.Trim()}\"");
        sb.AppendLine();
        sb.AppendLine("echo Target Directory:");
        sb.AppendLine("echo   \"%TARGET_PATH%\"");
        sb.AppendLine("echo.");
        sb.AppendLine("if not exist \"%TARGET_PATH%\" (");
        sb.AppendLine("    echo Creating directory...");
        sb.AppendLine("    mkdir \"%TARGET_PATH%\"");
        sb.AppendLine("    if errorlevel 1 (");
        sb.AppendLine("        echo.");
        sb.AppendLine("        echo [ERROR] Failed to create folder. Please ensure sufficient permissions.");
        sb.AppendLine("        pause");
        sb.AppendLine("        exit /b 1");
        sb.AppendLine("    )");
        sb.AppendLine("    echo [SUCCESS] Folder created successfully.");
        sb.AppendLine(") else (");
        sb.AppendLine("    echo [INFO] Folder already exists.");
        sb.AppendLine(")");

        if (options.OpenExplorerOnCompletion)
        {
            sb.AppendLine();
            sb.AppendLine("echo.");
            sb.AppendLine("echo Opening folder in File Explorer...");
            sb.AppendLine("start \"\" explorer.exe \"%TARGET_PATH%\"");
        }

        if (options.PauseOnCompletion)
        {
            sb.AppendLine();
            sb.AppendLine("echo.");
            sb.AppendLine("echo Press any key to exit...");
            sb.AppendLine("pause >nul");
        }

        sb.AppendLine();
        sb.AppendLine("exit /b 0");

        return sb.ToString();
    }

    public async Task<string> WriteToFileAsync(string destinationFilePath, BatchGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(destinationFilePath))
        {
            throw new ArgumentException("Destination file path cannot be empty or whitespace.", nameof(destinationFilePath));
        }

        string fullPath = Path.GetFullPath(destinationFilePath);
        string? directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string scriptContent = GenerateScriptContent(options);

        // Batch files on Windows expect CRLF line endings
        string crlfContent = scriptContent.Replace("\r\n", "\n").Replace("\n", "\r\n");

        await File.WriteAllTextAsync(fullPath, crlfContent, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        return fullPath;
    }
}
