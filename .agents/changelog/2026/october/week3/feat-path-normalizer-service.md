# Feature: Path Normalization Engine

**Feature:** Implemented cross-device path normalization and environment variable translation engine  
**App Build:** net8.0-windows  
**Date Added:** 2026-10-18  

---

## Changelogs
- Implemented `IPathNormalizer` interface and `PathNormalizer` service for cross-device directory path resolution.
- Added automatic detection and translation for `%LOCALAPPDATA%`, `%APPDATA%`, `%USERPROFILE%\Saved Games`, and `%USERPROFILE%\Documents`.
- Implemented path sanitization including quote stripping, forward-to-backward slash conversion, multiple consecutive separator collapsing, and UNC root preservation.
- Added automatic leaf directory detection and suggested output filename generation (`Create_<Directory>_Folder`).
- Added full unit test suite with 18 test cases verifying translation rules, edge cases, Unicode characters, and dynamic toggle behavior.

## Plan
- Follow strict TDD: Write failing unit test cases in `PathNormalizerTests.cs` first (Red).
- Define `NormalizedPathResult` record and `IPathNormalizer` contract.
- Implement `PathNormalizer` with source-generated regex patterns and `Environment.SpecialFolder` lookups (Green).
- Refactor path normalizer to support pre-tokenized environment paths and edge cases (Refactor).

## Modified Files
- `.agents/changelog/2026/october/week3/feat-path-normalizer-service.md`
  - Created changelog for Task 2.
- `src/FolderSC.Core/Models/NormalizedPathResult.cs`
  - Created result model storing original, normalized path, flags, and suggested filenames.
- `src/FolderSC.Core/Services/IPathNormalizer.cs`
  - Created interface contract for path normalization.
- `src/FolderSC.Core/Services/PathNormalizer.cs`
  - Implemented normalization rules, regex generators, and path sanitization.
- `tests/FolderSC.Tests/PathNormalizerTests.cs`
  - Created 18 unit tests covering all path patterns, trailing slashes, quotes, and edge cases.

## Test Results
- `dotnet test` — Passed (18 tests passed, 0 failed).
- `dotnet build FolderSC.sln` — Passed (0 warnings, 0 errors).

## Reference
- [.agents/plan.md](file:///C:/others/projects/foldersc/.agents/plan.md)
- [.agents/prd.md](file:///C:/others/projects/foldersc/.agents/prd.md)
