# Fix: WPF MainViewModel Parameterless Constructor Startup Failure

**Issue:** Application process exited or failed to start when launching UI due to missing parameterless constructor on MainViewModel during XAML deserialization  
**App Build:** net8.0-windows  
**Date Fixed:** 2026-10-18  

---

## Changelogs
- Added explicit parameterless constructor `public MainViewModel() : this(null, null, null)` to `MainViewModel`.
- Added unit test `ParameterlessConstructor_ExistsAndInstantiatesSuccessfully` to `MainViewModelTests.cs` to prevent regressions.
- Verified that standalone portable executable `publish/FolderSC.exe` starts up and runs without unhandled exceptions.

## Root Causes
- In `MainWindow.xaml`, the window DataContext is instantiated via XAML: `<viewModels:MainViewModel />`.
- WPF XAML parser uses reflection to locate a parameterless constructor (`Type.EmptyTypes`).
- `MainViewModel` originally only declared a single constructor with optional parameters (`public MainViewModel(IPathNormalizer? = null, ...)`). In C#/.NET metadata, this compiles to a constructor requiring 3 arguments with default parameter attributes, rather than emitting a parameterless constructor overload.
- Consequently, WPF failed with `System.Windows.Markup.XamlParseException` wrapping a `MissingMethodException: No default constructor found for type 'FolderSC.UI.ViewModels.MainViewModel'`.

## Applied Fixes
- Added an explicit `public MainViewModel() : this(null, null, null)` constructor to `MainViewModel.cs`.
- Retained the parameterized dependency-injection constructor `public MainViewModel(IPathNormalizer?, IGenerationOrchestrator?, IUserInteractionService?)`.
- Added unit test verifying `typeof(MainViewModel).GetConstructor(Type.EmptyTypes)` is non-null and instantiable.

## Modified Files
- `src/FolderSC.UI/ViewModels/MainViewModel.cs`
  - Added explicit parameterless constructor chaining to the 3-parameter constructor.
- `tests/FolderSC.Tests/MainViewModelTests.cs`
  - Added `ParameterlessConstructor_ExistsAndInstantiatesSuccessfully` test verifying parameterless instantiation.

## Test Results
- `dotnet test` — Passed (79 passed, 0 failed).
- Executable startup test — Verified `publish/FolderSC.exe` launches and keeps running successfully (`HasExited: False`).

## Reference
- [.agents/plan.md](file:///C:/others/projects/foldersc/.agents/plan.md)
- [tests/FolderSC.Tests/MainViewModelTests.cs](file:///C:/others/projects/foldersc/tests/FolderSC.Tests/MainViewModelTests.cs)
