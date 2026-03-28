# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

CK3ModStudio (formerly CreatePeopleTool) is a WPF desktop application for creating and managing fictional family trees for **Crusader Kings 3 (CK3) game modding**. It exports character and dynasty data in CK3 format, and includes a landed titles editor for AGOT submod creation.

## Build Commands

```bash
# Build with MSBuild (Visual Studio must be installed)
msbuild CK3ModStudio.sln /p:Configuration=Debug
msbuild CK3ModStudio.sln /p:Configuration=Release

# Build only the IPC library
dotnet build PipeCommunicationLibrary/PipeCommunicationLibrary.csproj
```

- Main executable output: `CK3ModStudio/bin/Debug/CK3ModStudio.exe`
- There are no automated tests in this project.

## Architecture

### Tech Stack

- **.NET Framework 4.8** + **WPF** + **Prism 8.1.97** (MVVM + DI via Unity)
- **Entity Framework 6.5.1** with MySQL (`MySql.Data`), SQLite (`System.Data.SQLite`), and SQL Server support
- UI libraries: AdonisUI, HandyControl, ModernWpfUI, LiveCharts (charts), gong-wpf-dragdrop
- **Newtonsoft.Json** for serialization; **Google.Protobuf** + LZ4/Zstd for compression

### Two-Project Solution

| Project | Framework | Purpose |
|---|---|---|
| `CK3ModStudio/` | .NET 4.8 | Main WPF application |
| `PipeCommunicationLibrary/` | .NET Standard 2.0 | Named pipe IPC shared library |

### MVVM Structure

`App.xaml.cs` bootstraps Prism with Unity DI. All ViewModels are registered in `PrismApplication.RegisterTypes()`.

**Views → ViewModels (by convention):**
- `MainWindow` / `MainWindowViewModel` — Application shell, orchestrates FamilyList, MaleList, FemaleList; handles file I/O commands and pipe communication
- `GenealogyUserControl` / `GenealogyUserControlViewModel` — Core family editing interface
- `DatabaseUserControl` / `DatabaseUserControlViewModel` — Database CRUD operations
- `CountyTimelineUserControl` / `CountyTimelineUserControlViewModel` — LiveCharts timeline visualization
- `StatisticsUserControl` / `StatisticsUserControlViewModel` — Family statistics
- `FileReadWriteUserControl` / `FileReadWriteUserControlViewModel` — File import/export
- `FileContentUserControl` / `FileContentUserControlViewModel` — Raw file content display
- Dialog ViewModels implement `IDialogAware`: `AssignmentWindowViewModel`, `CommonAssignmentWindowViewModel`, `FamilyTreeWindowViewModel`

Navigation-aware ViewModels implement `INavigationAware`. Tabs are managed via `IRegionManager` with named regions.

### Core Domain Models

- **`People`** — Character with Name, Dynasty, Gender, Religion, Culture, Mom/Dad/Spouse/Children relationships, and `LifeEventList`. Implements `BindableBase`.
- **`Family`** — Dynasty with `ObservableCollection<People>`. Has `AddMember()`/`RemoveMember()` and CK3 format string generation.
- **`LifeEvent`** — Birth/Death/Marriage events with `EventDate` in `"yyyy.M.d"` format. Created via `LifeEventFactory` with random date generation using realistic statistical distributions.
- **`County`** — Geographic region for timeline tracking.

### Helpers

- **`DatabaseHelper.cs`** — `FamilyRepository` + `FamilyDbContext` (MySQL EF); async `LoadFamiliesAsync()`/`SaveFamiliesAsync()`
- **`SqlServerDatabaseHelper.cs`** / **`SqliteDatabaseHelper.cs`** — Database-specific implementations
- **`FamilyInheritanceManager.cs`** — Logic for inheriting properties from parents
- **`CountyParser.cs`** — Large file (8700+ lines) parsing CK3 county data
- **`NameListParser.cs`** — Parses name lists and generates random names
- **`FileHeartbeatChecker.cs`** — Monitors files for external changes

### Inter-Process Communication

`PipeCommunicationLibrary` defines the protocol for communication between two running instances:
- Pipe name: `"WpfAppCommunicationPipe"`
- `PipeMessageType` enum covers: `AppBRunning`, `CheckStatus`, `ClientRunning`, `GetClientInfo`, `ShutdownRequest`, `ShutdownConfirmed`, `SendFamilyInfo`, `SendCoAPic`
- `PipeClientService.cs` in the main project handles client-side logic

### Event Aggregation

ViewModels communicate via Prism's `IEventAggregator`. Look for `_eventAggregator.GetEvent<SomeEvent>().Publish(...)` / `.Subscribe(...)` patterns for cross-ViewModel communication without tight coupling.

### Database Connection

MySQL connection string is named `"MySqlConnection"` in `App.config`. SQLite is embedded (no server required). The active database backend can be switched at runtime.

### CK3 Export Format

`People` and `Family` models have string generation methods that output CK3 scripting format. `PersonToStringConverter` and `FamilyToStringConverter` are the corresponding WPF value converters.
