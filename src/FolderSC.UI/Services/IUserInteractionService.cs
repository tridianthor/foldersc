namespace FolderSC.UI.Services;

/// <summary>
/// Abstraction for UI shell interactions such as opening folder dialogs, File Explorer, and clipboard.
/// </summary>
public interface IUserInteractionService
{
    /// <summary>
    /// Displays a folder browser dialog and returns the selected path, or null if cancelled.
    /// </summary>
    string? PickFolder(string? initialDirectory = null);

    /// <summary>
    /// Opens Windows File Explorer at the specified file or folder path.
    /// </summary>
    void OpenInExplorer(string path);

    /// <summary>
    /// Copies text to the Windows system clipboard.
    /// </summary>
    void CopyToClipboard(string text);
}
