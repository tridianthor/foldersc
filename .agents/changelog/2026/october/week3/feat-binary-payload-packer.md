# Feature: Binary Payload Contract & Packer

**Feature:** Binary overlay packaging and unpacking engine using the FSC1 format for standalone runner executables  
**App Build:** net8.0-windows  
**Date Added:** 2026-10-18  

---

## Changelogs
- Implemented `IPayloadPacker` interface and `PayloadPacker` service to encode and decode runner payloads into binary overlays.
- Created `RunnerPayload` model capturing target path, Explorer launch flag, title, and creation timestamp.
- Implemented `FSC1` binary format (`[Magic: 'FSC1' (4B)][Length: Int32 (4B)][UTF-8 JSON bytes (NB)]`).
- Added backward buffer scanning to locate and unpack payloads appended to arbitrary executable files.
- Added comprehensive unit tests covering round-trip packing/unpacking, simulated PE embedding, and corrupt stream handling.

## Plan
- Follow strict TDD: Write unit tests in `PayloadPackerTests.cs` defining expected binary layout and failure conditions (Red).
- Define `RunnerPayload` model and `IPayloadPacker` interface contract.
- Implement `PayloadPacker` with byte buffer manipulation, JSON serialization, and backward scanner (Green).
- Verify all unit tests and build status with 0 warnings/errors (Refactor).

## Modified Files
- `.agents/changelog/2026/october/week3/feat-binary-payload-packer.md`
  - Created changelog for Task 4.
- `src/FolderSC.Core/Models/RunnerPayload.cs`
  - Created executable runner payload data model.
- `src/FolderSC.Core/Services/IPayloadPacker.cs`
  - Created interface contract for binary payload serialization and unpacking.
- `src/FolderSC.Core/Services/PayloadPacker.cs`
  - Implemented binary packing, overlay decoding, and backward scanning logic.
- `tests/FolderSC.Tests/PayloadPackerTests.cs`
  - Added unit tests for round-trip, simulated PE embedding, and error conditions.

## Test Results
- `dotnet test` — Passed (43 tests passed, 0 failed).
- `dotnet build FolderSC.sln` — Passed (0 warnings, 0 errors).

## Reference
- [.agents/plan.md](file:///C:/others/projects/foldersc/.agents/plan.md)
- [.agents/prd.md](file:///C:/others/projects/foldersc/.agents/prd.md)
