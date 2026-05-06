# Gate 1: Solution Scaffolding

Goal: Create the project structure and dependencies.

## Terminal Commands

Run these commands in sequence from repository root (C:/source/repos/afodom-spa-assessment).

### Step 1: Create solution file

```bash
dotnet new sln -n SubawardReader
```

Creates a solution file. Depending on SDK/tooling version, this may be `.sln` or `.slnx`.

### Step 2: Create console app project

```bash
dotnet new console -n SubawardReader -o SubawardReader
```

### Step 3: Create test project

```bash
dotnet new xunit -n SubawardReader.Tests -o SubawardReader.Tests
```

### Step 4: Add both projects to solution

```bash
dotnet sln <SolutionFile> add <AppName>/<AppName>.csproj
dotnet sln <SolutionFile> add <AppName>.Tests/<AppName>.Tests.csproj
```

Use the actual file created in Step 1, for example `SubawardReader.slnx`.

### Step 5: Add ClosedXML package

```bash
dotnet add SubawardReader/SubawardReader.csproj package ClosedXML
```

ClosedXML provides the Excel `.xlsx` parsing API used by this project. Adding it in Gate 1 verifies dependency restore works before parser implementation starts.

### Step 6: Add test dependencies

```bash
dotnet add SubawardReader.Tests/SubawardReader.Tests.csproj package xunit.runner.visualstudio
```

## Verification

After all commands complete:

```bash
dotnet restore
dotnet build
```

Both should complete without errors.

Repo hygiene: commit `.gitignore` before or with scaffolding so generated files are not accidentally staged.

## Exit Criteria

- [x] Solution file created
- [x] Console app project created
- [x] Test project created
- [x] Both projects added to solution
- [x] ClosedXML package added
- [x] xUnit dependencies added
- [x] Solution restores successfully
- [x] All projects compile without errors
