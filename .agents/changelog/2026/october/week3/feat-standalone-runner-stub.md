# Feature: Standalone Runner Stub

**Feature:** Lightweight standalone executable stub for handheld runner execution with self-overlay parsing  
**App Build:** net8.0-windows  
**Date Added:** 2026-10-18  

---

## Changelogs
- Implemented `FolderSC.Stub` entry point that reads its own binary stream, unpacks the `FSC1` overlay, and creates the requested directory.
- Added native Win32 `MessageBoxW` dialogs via P/Invoke for lightweight, dependency-free user notifications on Windows handhelds.
- Added support for `--silent` argument and `FOLDERSC_SILENT` environment variable to facilitate automated testing.
- Switched `PayloadPacker` to source-generated `JsonSerializerContext` to eliminate reflection and trim warnings.
- Embedded pre-packaged `StubRunner.bin` resource into `FolderSC.Core` for zero-compiler standalone `.exe` generation.
- Added integration tests executing the runner executable in a real subprocess and verifying physical directory hierarchy creation.

## Plan
- Follow strict TDD: Write subprocess integration tests in `StubRunnerTests.cs` first (Red).
- Implement `Program.cs` in `FolderSC.Stub` with binary overlay extraction, environment expansion, Explorer launching, and Win32 dialogs (Green).
- Optimize with trimmed single-file compilation, embed stub asset into Core resources, and verify all tests pass (Refactor).

## Modified Files
- `.agents/changelog/2026/october/week3/feat-standalone-runner-stub.md`
  - Created changelog for Task 5.
- `.gitignore`
  - Added publish directory to ignore rules.
- `src/FolderSC.Core/FolderSC.Core.csproj`
  - Embedded `StubRunner.bin` resource.
- `src/FolderSC.Core/Resources/StubRunner.bin`
  - Bundled standalone executable template.
- `src/FolderSC.Core/Services/PayloadPacker.cs`
  - Added source-generated `JsonSerializerContext`.
- `src/FolderSC.Stub/FolderSC.Stub.csproj`
  - Removed desktop UI framework dependencies, targeting pure Win32 WinExe.
- `src/FolderSC.Stub/Program.cs`
  - Implemented runner execution lifecycle and self-unpacking logic.
- `tests/FolderSC.Tests/FolderSC.Tests.csproj`
  - Referenced `FolderSC.Stub` project.
- `tests/FolderSC.Tests/StubRunnerTests.cs`
  - Added end-to-end subprocess execution tests.

## Test Results
- `dotnet test` — Passed (45 tests passed, 0 failed).
- `dotnet build FolderSC.sln` — Passed (0 warnings, 0 errors).

## Reference
- [.agents/plan.md](file:///C:/others/projects/foldersc/.agents/plan.md)
- [.agents/prd.md](file:///C:/others/projects/foldersc/.agents/prd.md)
