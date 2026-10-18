using FolderSC.Core.Models;
using FolderSC.Core.Services;
using FolderSC.UI.Services;
using FolderSC.UI.ViewModels;

namespace FolderSC.Tests;

public class MainViewModelTests
{
    private readonly MockOrchestrator _mockOrchestrator = new();
    private readonly MockInteractionService _mockInteraction = new();
    private readonly IPathNormalizer _pathNormalizer = new PathNormalizer();

    private MainViewModel CreateViewModel()
    {
        return new MainViewModel(_pathNormalizer, _mockOrchestrator, _mockInteraction);
    }

    [Fact]
    public void ParameterlessConstructor_ExistsAndInstantiatesSuccessfully()
    {
        var constructor = typeof(MainViewModel).GetConstructor(Type.EmptyTypes);
        Assert.NotNull(constructor);

        var instance = Activator.CreateInstance<MainViewModel>();
        Assert.NotNull(instance);
        Assert.Equal(OutputFormat.Executable, instance.SelectedFormat);
    }

    [Fact]
    public void InitialState_HasExpectedDefaults()
    {
        var vm = CreateViewModel();

        Assert.Equal(OutputFormat.Executable, vm.SelectedFormat);
        Assert.True(vm.UseDynamicVariables);
        Assert.True(vm.OpenExplorerOnExecution);
        Assert.False(string.IsNullOrWhiteSpace(vm.OutputDirectory));
        Assert.False(vm.IsProcessing);
        Assert.False(vm.IsSuccess);
        Assert.False(vm.HasError);
        Assert.False(vm.ProcessCommand.CanExecute(null));
    }

    [Fact]
    public void SourcePath_WhenSet_UpdatesPreviewAndSuggestedFileNameAndEnablesCommand()
    {
        var vm = CreateViewModel();

        vm.SourcePath = @"C:\Users\Gamer\AppData\Local\GameStudio\Saves";

        Assert.Equal(@"%LOCALAPPDATA%\GameStudio\Saves", vm.NormalizedPreviewPath);
        Assert.True(vm.HasDynamicVariables);
        Assert.Equal("Saves", vm.LeafFolderName);
        Assert.Equal("Create_Saves_Folder", vm.SuggestedFileName);
        Assert.True(vm.ProcessCommand.CanExecute(null));
    }

    [Fact]
    public void SourcePath_WhenCleared_ResetsPreviewAndDisablesCommand()
    {
        var vm = CreateViewModel();

        vm.SourcePath = @"C:\Users\Gamer\AppData\Local\GameStudio\Saves";
        Assert.True(vm.ProcessCommand.CanExecute(null));

        vm.SourcePath = "";
        Assert.Equal("", vm.NormalizedPreviewPath);
        Assert.False(vm.HasDynamicVariables);
        Assert.False(vm.ProcessCommand.CanExecute(null));
    }

    [Fact]
    public void UseDynamicVariables_WhenToggled_RecalculatesPreviewPath()
    {
        var vm = CreateViewModel();

        vm.SourcePath = @"C:\Users\Gamer\AppData\Local\GameStudio\Saves";
        Assert.Equal(@"%LOCALAPPDATA%\GameStudio\Saves", vm.NormalizedPreviewPath);

        vm.UseDynamicVariables = false;
        Assert.Equal(@"C:\Users\Gamer\AppData\Local\GameStudio\Saves", vm.NormalizedPreviewPath);
        Assert.False(vm.HasDynamicVariables);

        vm.UseDynamicVariables = true;
        Assert.Equal(@"%LOCALAPPDATA%\GameStudio\Saves", vm.NormalizedPreviewPath);
        Assert.True(vm.HasDynamicVariables);
    }

    [Fact]
    public async Task ProcessCommand_WhenSuccessful_SetsSuccessStateAndLastGeneratedPath()
    {
        var vm = CreateViewModel();
        vm.SourcePath = @"C:\Games\Saves";
        _mockOrchestrator.NextResult = GenerationResult.Ok(@"C:\Desktop\Create_Saves_Folder.exe", @"C:\Games\Saves", OutputFormat.Executable);

        await vm.ProcessCommand.ExecuteAsync();

        Assert.True(vm.IsSuccess);
        Assert.False(vm.HasError);
        Assert.Equal(@"C:\Desktop\Create_Saves_Folder.exe", vm.LastGeneratedFilePath);
        Assert.NotNull(vm.StatusMessage);
    }

    [Fact]
    public async Task ProcessCommand_WhenFailed_SetsErrorState()
    {
        var vm = CreateViewModel();
        vm.SourcePath = @"C:\Games\Saves";
        _mockOrchestrator.NextResult = GenerationResult.Fail("Permission denied.", OutputFormat.Executable);

        await vm.ProcessCommand.ExecuteAsync();

        Assert.False(vm.IsSuccess);
        Assert.True(vm.HasError);
        Assert.Equal("Permission denied.", vm.StatusMessage);
    }

    [Fact]
    public void BrowseSourceFolderCommand_WhenUserPicksFolder_UpdatesSourcePath()
    {
        var vm = CreateViewModel();
        _mockInteraction.NextFolder = @"D:\Games\MyGame\Saves";

        vm.BrowseSourceFolderCommand.Execute(null);

        Assert.Equal(@"D:\Games\MyGame\Saves", vm.SourcePath);
    }

    [Fact]
    public void BrowseOutputDirectoryCommand_WhenUserPicksFolder_UpdatesOutputDirectory()
    {
        var vm = CreateViewModel();
        _mockInteraction.NextFolder = @"E:\SharedFolder";

        vm.BrowseOutputDirectoryCommand.Execute(null);

        Assert.Equal(@"E:\SharedFolder", vm.OutputDirectory);
    }

    [Fact]
    public void OpenOutputFolderCommand_InvokesInteractionService()
    {
        var vm = CreateViewModel();
        vm.LastGeneratedFilePath = @"C:\Users\User\Desktop\Create_Folder.exe";

        vm.OpenOutputFolderCommand.Execute(null);

        Assert.Equal(@"C:\Users\User\Desktop\Create_Folder.exe", _mockInteraction.LastOpenedInExplorer);
    }

    [Fact]
    public void CopyNormalizedPathCommand_CopiesPreviewToClipboard()
    {
        var vm = CreateViewModel();
        vm.SourcePath = @"C:\Users\User\AppData\Local\Game";

        vm.CopyNormalizedPathCommand.Execute(null);

        Assert.Equal(@"%LOCALAPPDATA%\Game", _mockInteraction.LastCopiedToClipboard);
    }

    private sealed class MockOrchestrator : IGenerationOrchestrator
    {
        public GenerationResult NextResult { get; set; } = GenerationResult.Ok("test.exe", "%LOCALAPPDATA%", OutputFormat.Executable);

        public Task<GenerationResult> ProcessAsync(GenerationRequest request)
        {
            return Task.FromResult(NextResult);
        }
    }

    private sealed class MockInteractionService : IUserInteractionService
    {
        public string? NextFolder { get; set; }
        public string? LastOpenedInExplorer { get; private set; }
        public string? LastCopiedToClipboard { get; private set; }

        public string? PickFolder(string? initialDirectory = null) => NextFolder;

        public void OpenInExplorer(string path) => LastOpenedInExplorer = path;

        public void CopyToClipboard(string text) => LastCopiedToClipboard = text;
    }
}
