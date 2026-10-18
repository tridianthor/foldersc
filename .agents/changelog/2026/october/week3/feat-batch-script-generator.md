# Feature: Batch Script Generator

**Feature:** Standalone Windows Batch script generator service for one-click folder creation  
**App Build:** net8.0-windows  
**Date Added:** 2026-10-18  

---

## Changelogs
- Implemented `IBatchGenerator` interface and `BatchGenerator` service to produce standalone Windows Batch scripts (`.bat`).
- Added recursive directory creation syntax (`if not exist "%TARGET_PATH%" mkdir "%TARGET_PATH%"`) with error-level checks.
- Added configurable completion options for opening Windows File Explorer at the target folder and optional console pause.
- Handled path escaping for spaces, special characters, and dynamic environment variables (`%LOCALAPPDATA%`, `%APPDATA%`, `%USERPROFILE%`).
- Added comprehensive unit and integration tests executing the generated `.bat` scripts in a sandboxed temporary environment via `cmd.exe`.

## Plan
- Follow strict TDD: Write unit and sandbox execution tests in `BatchGeneratorTests.cs` first (Red).
- Define `BatchGenerationOptions` configuration model and `IBatchGenerator` interface.
- Implement `BatchGenerator` with CRLF-formatted batch template and async file writing (Green).
- Refactor completion pause checks and verify sandboxed folder creation (Refactor).

## Modified Files
- `.agents/changelog/2026/october/week3/feat-batch-script-generator.md`
  - Created changelog for Task 3.
- `src/FolderSC.Core/Models/BatchGenerationOptions.cs`
  - Created configuration options model for batch script generation.
- `src/FolderSC.Core/Services/IBatchGenerator.cs`
  - Created interface contract for batch generation.
- `src/FolderSC.Core/Services/BatchGenerator.cs`
  - Implemented script generation logic and async CRLF file writer.
- `tests/FolderSC.Tests/BatchGeneratorTests.cs`
  - Added unit and sandboxed process execution tests.

## Test Results
- `dotnet test` — Passed (30 tests passed, 0 failed).
- `dotnet build FolderSC.sln` — Passed (0 warnings, 0 errors).

## Reference
- [.agents/plan.md](file:///C:/others/projects/foldersc/.agents/plan.md)
- [.agents/prd.md](file:///C:/others/projects/foldersc/.agents/prd.md)
