# Feature: UI ViewModel & State Management

**Feature:** MVVM presentation architecture and reactive state management for FolderSC desktop interface  
**App Build:** net8.0-windows  
**Date Added:** 2026-10-18  

---

## Changelogs
- Implemented `MainViewModel` handling input validation, live path normalization preview, format selection, and generation triggers.
- Implemented `ViewModelBase` providing property change notifications via `INotifyPropertyChanged`.
- Implemented `RelayCommand` and `AsyncRelayCommand` supporting execution states and `CanExecute` change triggers.
- Implemented `IUserInteractionService` and `WindowsUserInteractionService` decoupling folder browser dialogs, Windows clipboard, and Explorer launches for unit testability.
- Added comprehensive unit tests covering reactive path recalculations, command invocations, error handling, and clipboard interactions.

## Plan
- Follow strict TDD: Write unit tests in `MainViewModelTests.cs` defining expected presentation behavior and state changes (Red).
- Define `IUserInteractionService` and `WindowsUserInteractionService`.
- Implement `ViewModelBase`, command infrastructure, and `MainViewModel` (Green).
- Resolve WPF `System.IO.Path` ambiguity in tests with global usings and verify clean build (Refactor).

## Modified Files
- `.agents/changelog/2026/october/week3/feat-ui-viewmodel-state-management.md`
  - Created changelog for Task 8.
- `src/FolderSC.UI/Services/IUserInteractionService.cs`
  - Created interface contract for UI shell interactions.
- `src/FolderSC.UI/Services/WindowsUserInteractionService.cs`
  - Implemented folder browser dialog and clipboard integration for WPF.
- `src/FolderSC.UI/ViewModels/MainViewModel.cs`
  - Implemented main application ViewModel with reactive previewing and commands.
- `src/FolderSC.UI/ViewModels/RelayCommand.cs`
  - Implemented synchronous and asynchronous command infrastructure.
- `src/FolderSC.UI/ViewModels/ViewModelBase.cs`
  - Created base class for property change notification.
- `tests/FolderSC.Tests/FolderSC.Tests.csproj`
  - Added reference to `FolderSC.UI` and global `System.IO` using.
- `tests/FolderSC.Tests/MainViewModelTests.cs`
  - Added unit tests for all properties, commands, and states.

## Test Results
- `dotnet test` — Passed (75 tests passed, 0 failed).
- `dotnet build FolderSC.sln` — Passed (0 warnings, 0 errors).

## Reference
- [.agents/plan.md](file:///C:/others/projects/foldersc/.agents/plan.md)
- [.agents/prd.md](file:///C:/others/projects/foldersc/.agents/prd.md)
