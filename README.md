# CK3 Mod Studio

A WPF desktop tool for **Crusader Kings 3** mod creation, focused on managing families, characters, dynasties, and landed titles — with dedicated support for **AGOT (A Game of Thrones)** submod creation.

---

## Features

### Family & Character Editor
- Create and manage fictional dynasties and family trees
- Define characters with name, gender, religion, culture, and life events (birth, death, marriage)
- Realistic random date generation using statistical distributions
- Export characters and dynasties directly in CK3 scripting format
- Visualize family trees in a dedicated diagram view
- Import/export family data as JSON or CK3 format files

### Landed Titles Editor
- Parse and edit CK3 `landed_titles.txt` files with full hierarchy support (Empire → Kingdom → Duchy → County → Barony)
- Move titles between parents via drag or dialog
- Add/remove titles at any rank
- Save and reload: last opened file is remembered across sessions

### AGOT Submod Generator
- For any selected county, automatically generate all files required by an **AGOT More Counts** submod:
  - Province history (`history/provinces/`)
  - Development levels (`xsy_z_development_levels.txt`)
  - Title/ruler history (`xsy_titles_{region}.txt`)
  - English and Chinese localization YAML files
- Culture and religion dropdowns populated directly from the AGOT main mod's province files
- Auto-fills reference county, culture, and religion based on the duchy capital

### Statistics & Timeline
- LiveCharts-based timeline visualization for county data
- Family statistics view

### Database Support
- MySQL, SQLite, and SQL Server backends
- Async load/save of family data

### Inter-Process Communication
- Named pipe protocol for communication between two running instances
- Supports family data exchange and coordinated shutdown

---

## Requirements

- Windows 10 / 11
- .NET Framework 4.8
- Visual Studio 2022 (for building)
- Crusader Kings 3 + AGOT mod (for submod generation features)

---

## Build

```bash
msbuild CK3ModStudio.sln /p:Configuration=Release
```

Output: `CK3ModStudio/bin/Release/CK3ModStudio.exe`

---

## Project Structure

```
CK3ModStudio.sln
├── CK3ModStudio/              # Main WPF application (.NET 4.8)
│   ├── Views/                 # XAML views
│   ├── ViewModels/            # Prism MVVM view models
│   ├── Model/                 # Domain models (People, Family, LandedTitle, ...)
│   ├── Helper/                # Parsers, generators, database helpers
│   └── Converter/             # WPF value converters
└── PipeCommunicationLibrary/  # Named pipe IPC (.NET Standard 2.0)
```

---

## Settings

User settings (last opened file, mod paths) are persisted to:
```
%AppData%\CK3ModStudio\settings.json
```

---

## Version History

| Version | Notes |
|---------|-------|
| v6.0-beta | Renamed to CK3ModStudio; Landed Titles Editor; AGOT submod generator; persistent settings |
| v5.x | Named pipe IPC; pipe communication between instances |
| v4.x | Database integration (MySQL/SQLite); statistics view |

---

## License

See [LICENSE.txt](LICENSE.txt)
