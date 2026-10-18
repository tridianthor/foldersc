using System.IO;
using FolderSC.Core.Models;
using FolderSC.Core.Services;
using FolderSC.UI.Services;

namespace FolderSC.UI.ViewModels;

/// <summary>
/// ViewModel managing the main application state, validation, commands, and generation workflow.
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly IPathNormalizer _pathNormalizer;
    private readonly IGenerationOrchestrator _orchestrator;
    private readonly IUserInteractionService _interactionService;

    private string _sourcePath = string.Empty;
    private string _normalizedPreviewPath = string.Empty;
    private string _suggestedFileName = string.Empty;
    private string _leafFolderName = string.Empty;
    private bool _hasDynamicVariables;
    private bool _useDynamicVariables = true;
    private OutputFormat _selectedFormat = OutputFormat.Executable;
    private bool _openExplorerOnExecution = true;
    private string _outputDirectory = string.Empty;
    private string _customOutputFileName = string.Empty;
    private string _title = string.Empty;
    private bool _isProcessing;
    private bool _isSuccess;
    private bool _hasError;
    private string _statusMessage = string.Empty;
    private string? _lastGeneratedFilePath;

    public MainViewModel() : this(null, null, null)
    {
    }

    public MainViewModel(
        IPathNormalizer? pathNormalizer,
        IGenerationOrchestrator? orchestrator,
        IUserInteractionService? interactionService)
    {
        _pathNormalizer = pathNormalizer ?? new PathNormalizer();
        _orchestrator = orchestrator ?? new GenerationOrchestrator();
        _interactionService = interactionService ?? new WindowsUserInteractionService();

        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        _outputDirectory = string.IsNullOrWhiteSpace(desktop) ? Directory.GetCurrentDirectory() : desktop;

        BrowseSourceFolderCommand = new RelayCommand(BrowseSourceFolder);
        BrowseOutputDirectoryCommand = new RelayCommand(BrowseOutputDirectory);
        ProcessCommand = new AsyncRelayCommand(ProcessAsync, CanProcess);
        OpenOutputFolderCommand = new RelayCommand(OpenOutputFolder, () => !string.IsNullOrEmpty(LastGeneratedFilePath));
        CopyNormalizedPathCommand = new RelayCommand(CopyNormalizedPath, () => !string.IsNullOrEmpty(NormalizedPreviewPath));
        ResetCommand = new RelayCommand(Reset);
    }

    public string SourcePath
    {
        get => _sourcePath;
        set
        {
            if (SetProperty(ref _sourcePath, value))
            {
                UpdatePathPreview();
                ProcessCommand.RaiseCanExecuteChanged();
                ResetStatus();
            }
        }
    }

    public string NormalizedPreviewPath
    {
        get => _normalizedPreviewPath;
        private set
        {
            if (SetProperty(ref _normalizedPreviewPath, value))
            {
                CopyNormalizedPathCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string SuggestedFileName
    {
        get => _suggestedFileName;
        private set => SetProperty(ref _suggestedFileName, value);
    }

    public string LeafFolderName
    {
        get => _leafFolderName;
        private set => SetProperty(ref _leafFolderName, value);
    }

    public bool HasDynamicVariables
    {
        get => _hasDynamicVariables;
        private set => SetProperty(ref _hasDynamicVariables, value);
    }

    public bool UseDynamicVariables
    {
        get => _useDynamicVariables;
        set
        {
            if (SetProperty(ref _useDynamicVariables, value))
            {
                UpdatePathPreview();
            }
        }
    }

    public OutputFormat SelectedFormat
    {
        get => _selectedFormat;
        set => SetProperty(ref _selectedFormat, value);
    }

    public bool OpenExplorerOnExecution
    {
        get => _openExplorerOnExecution;
        set => SetProperty(ref _openExplorerOnExecution, value);
    }

    public string OutputDirectory
    {
        get => _outputDirectory;
        set => SetProperty(ref _outputDirectory, value);
    }

    public string CustomOutputFileName
    {
        get => _customOutputFileName;
        set => SetProperty(ref _customOutputFileName, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public bool IsProcessing
    {
        get => _isProcessing;
        private set
        {
            if (SetProperty(ref _isProcessing, value))
            {
                ProcessCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsSuccess
    {
        get => _isSuccess;
        private set => SetProperty(ref _isSuccess, value);
    }

    public bool HasError
    {
        get => _hasError;
        private set => SetProperty(ref _hasError, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string? LastGeneratedFilePath
    {
        get => _lastGeneratedFilePath;
        set
        {
            if (SetProperty(ref _lastGeneratedFilePath, value))
            {
                OpenOutputFolderCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public RelayCommand BrowseSourceFolderCommand { get; }
    public RelayCommand BrowseOutputDirectoryCommand { get; }
    public AsyncRelayCommand ProcessCommand { get; }
    public RelayCommand OpenOutputFolderCommand { get; }
    public RelayCommand CopyNormalizedPathCommand { get; }
    public RelayCommand ResetCommand { get; }

    private bool CanProcess()
    {
        return !IsProcessing && !string.IsNullOrWhiteSpace(SourcePath);
    }

    private void UpdatePathPreview()
    {
        if (string.IsNullOrWhiteSpace(SourcePath))
        {
            NormalizedPreviewPath = string.Empty;
            SuggestedFileName = string.Empty;
            LeafFolderName = string.Empty;
            HasDynamicVariables = false;
            return;
        }

        try
        {
            var result = _pathNormalizer.Normalize(SourcePath, UseDynamicVariables);
            NormalizedPreviewPath = result.NormalizedPath;
            SuggestedFileName = result.SuggestedFileName;
            LeafFolderName = result.LeafDirectoryName;
            HasDynamicVariables = result.HasDynamicVariables;
        }
        catch
        {
            NormalizedPreviewPath = SourcePath.Trim();
            SuggestedFileName = "Create_Folder";
            LeafFolderName = string.Empty;
            HasDynamicVariables = false;
        }
    }

    private async Task ProcessAsync()
    {
        IsProcessing = true;
        ResetStatus();

        try
        {
            var request = new GenerationRequest
            {
                SourcePath = SourcePath,
                Format = SelectedFormat,
                UseDynamicVariables = UseDynamicVariables,
                OpenExplorerOnCompletion = OpenExplorerOnExecution,
                OutputDirectory = OutputDirectory,
                OutputFileName = CustomOutputFileName,
                Title = Title
            };

            var result = await _orchestrator.ProcessAsync(request);

            if (result.Success)
            {
                IsSuccess = true;
                HasError = false;
                LastGeneratedFilePath = result.OutputFilePath;
                string formatName = result.Format == OutputFormat.Executable ? "Executable (.exe)" : "Batch Script (.bat)";
                StatusMessage = $"Successfully generated {formatName} at:\n{result.OutputFilePath}";
            }
            else
            {
                IsSuccess = false;
                HasError = true;
                StatusMessage = result.ErrorMessage ?? "Generation failed with an unknown error.";
            }
        }
        catch (Exception ex)
        {
            IsSuccess = false;
            HasError = true;
            StatusMessage = $"Unexpected error: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private void BrowseSourceFolder()
    {
        string? picked = _interactionService.PickFolder(SourcePath);
        if (!string.IsNullOrWhiteSpace(picked))
        {
            SourcePath = picked;
        }
    }

    private void BrowseOutputDirectory()
    {
        string? picked = _interactionService.PickFolder(OutputDirectory);
        if (!string.IsNullOrWhiteSpace(picked))
        {
            OutputDirectory = picked;
        }
    }

    private void OpenOutputFolder()
    {
        if (!string.IsNullOrEmpty(LastGeneratedFilePath))
        {
            _interactionService.OpenInExplorer(LastGeneratedFilePath);
        }
    }

    private void CopyNormalizedPath()
    {
        if (!string.IsNullOrEmpty(NormalizedPreviewPath))
        {
            _interactionService.CopyToClipboard(NormalizedPreviewPath);
        }
    }

    private void Reset()
    {
        SourcePath = string.Empty;
        CustomOutputFileName = string.Empty;
        Title = string.Empty;
        ResetStatus();
    }

    private void ResetStatus()
    {
        IsSuccess = false;
        HasError = false;
        StatusMessage = string.Empty;
    }
}
