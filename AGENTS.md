# AGENTS.md — Agent Development & Documentation Guide

Welcome to the **FolderSC** project. This document defines the mandatory guidelines, development workflows, documentation standards, and git procedures that all AI agents must follow when contributing to this repository.

---

## 1. Core Development Methodology: Test-Driven Development (TDD)

This project strictly follows **Test-Driven Development (TDD)**. All feature implementations and bug fixes must follow the **Red $\rightarrow$ Green $\rightarrow$ Refactor** cycle:

1. **Red (Write Tests First):**
   - Before writing any production code, write comprehensive unit and/or integration tests defining the expected behavior, edge cases, and interfaces.
   - Run the tests (`dotnet test`) and verify that they fail for the expected reason.
2. **Green (Minimal Implementation):**
   - Write the simplest, cleanest implementation necessary to make all tests pass.
   - Run `dotnet test` to verify green status.
3. **Refactor:**
   - Clean up code, remove redundancies, and optimize architecture while ensuring all tests continue to pass with 100% success rate.
4. **Zero Regressions:**
   - Under no circumstances should an agent consider a task complete if tests are failing.

---

## 2. Documentation Rules & Storage

### 2.1 Documentation Location
- **All documentation must go into the `.agents/` folder.**
- Do not create loose markdown or text documentation at the project root or scattered throughout source code folders (except this `AGENTS.md` and standard project README).

### 2.2 Chronological Folder Organization Convention
All agent documentation (issues, feature specs, plans, reports, changelogs) must strictly adhere to the organization conventions defined in:
👉 [.agents/agent-files-convention.md](file:///C:/others/projects/foldersc/.agents/agent-files-convention.md)

#### Directory Structure:
```text
.agents/<category>/<year>/<month>/week<N>/<file-name>.md
```

- **`<category>`**: Functional category (`feature`, `issue`, `plan`, `changelog`, `report`).
- **`<year>`**: 4-digit year (e.g., `2026`).
- **`<month>`**: Full lowercase English month name (e.g., `january`, `october`).
- **`week<N>`**: Calendar week (`week1`, `week2`, etc.) based on the Monday–Sunday Calendar Week model:
  - `week1` begins on the 1st day of the month and ends on the month's first Sunday.
  - Every subsequent Monday starts a new week (`week2`, `week3`, etc.).
- **`<file-name>.md>`**: Descriptive, lowercase kebab-cased markdown filename.

---

## 3. Post-Development Workflow: Changelog Creation

Immediately after successfully developing a feature or fixing an issue, the agent **must create a changelog** using the guidelines from the `create-changelog` skill.

### 3.1 Destination Path
Place changelog files in `.agents/changelog/` organized chronologically according to the convention above:
```text
.agents/changelog/<year>/<month>/week<N>/<slugged-changelog-name>.md
```
*(Example: `.agents/changelog/2026/october/week3/feat-path-normalizer-service.md`)*

### 3.2 Required Changelog Sections

#### For Features:
```markdown
# Feature: <Feature Title>

**Feature:** <Short summary of the feature>  
**App Build:** net8.0-windows  
**Date Added:** YYYY-MM-DD  

---

## Changelogs
- <Bullet point user/developer visible changes>

## Plan
- <Implementation summary>

## Modified Files
- `path/to/file`
  - <Summary of changes>

## Test Results
- `dotnet test ...` — Passed (<N> tests).

## Reference
- <Links to PRD, Plan, or Task specifications>
```

#### For Issue Fixes:
```markdown
# Fix: <Issue Title>

**Issue:** <Short summary of the problem>  
**App Build:** net8.0-windows  
**Date Fixed:** YYYY-MM-DD  

---

## Changelogs
- <Bullet point fixes>

## Root Causes
- <Verified reason the issue occurred>

## Applied Fixes
- <Technical changes that resolved the issue>

## Modified Files
- `path/to/file`
  - <Summary of changes>

## Test Results
- `dotnet test ...` — Passed.

## Reference
- <Links to relevant issues or reports>
```

### 3.3 Changelog Rules
- **Fact-Based Only:** Never invent test results, root causes, or modified files. Report only what was verified with actual command outputs and git diffs.
- **Slugged Filename:** Lowercase, hyphen-separated, descriptive (e.g., `fix-appdata-path-escaping.md`).

---

## 4. Git Commit Rules

Once the feature/fix is completed, verified with tests, and the changelog is written:

### 4.1 Generate Commit Message
Use the `generate-commit-message` skill principles:
- Inspect changes using `git status` and `git diff`.
- Formulate a concise, imperative commit message following Conventional Commits (e.g., `feat: ...`, `fix: ...`, `test: ...`, `docs: ...`, `refactor: ...`), preferably under 50 characters for the subject line.

### 4.2 Commit Changes
1. Stage the relevant source files, test files, and changelog:
   ```powershell
   git add <files>
   ```
2. Commit with the generated message:
   ```powershell
   git commit -m "<type>: <concise description>"
   ```

### 4.3 ⚠️ Critical Constraint: Never Push
> [!CAUTION]
> **STOP AT COMMIT.**
> Agents must NEVER run `git push` or execute any remote push command. Leave the commit local for the user to review and push at their discretion.

---

## 5. Technology Stack & Verification Commands

- **Language & Runtime:** C# / .NET 8 (`net8.0-windows`)
- **UI Framework:** WPF (Windows Presentation Foundation)
- **Testing Framework:** xUnit / MSTest

### Essential Commands:
- **Build Solution:**
  ```powershell
  dotnet build FolderSC.sln
  ```
- **Run All Tests:**
  ```powershell
  dotnet test
  ```
- **Run Specific Test Suite:**
  ```powershell
  dotnet test --filter FullyQualifiedName~<TestClassName>
  ```
- **Check Git Status & Diffs:**
  ```powershell
  git status
  git diff
  ```

---

## 6. Summary Checklist for Agents

Before completing any development session, ensure:
- [ ] Tests were written first (TDD approach).
- [ ] Code is implemented and all unit/integration tests pass.
- [ ] Code adheres to clean architecture and preserves existing formatting.
- [ ] All new documentation is placed inside `.agents/` adhering to [.agents/agent-files-convention.md](file:///C:/others/projects/foldersc/.agents/agent-files-convention.md).
- [ ] Changelog created in `.agents/changelog/<year>/<month>/week<N>/` using `create-changelog` format.
- [ ] Staged and committed locally with a clear commit message.
- [ ] **No `git push` executed.**
