# Changelog

All notable changes to the Unity Volume Rendering package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-02-12

### Added
- Unity Package Manager (UPM) support. Install via git URL:
  `https://github.com/mlavik1/UnityVolumeRendering.git`
- `package.json` with full UPM metadata and samples registration.
- Importable samples: "Basic Demo" (scene + camera controller) and "Sample Data"
  (VisMale raw dataset, PARCHG VASP dataset).

### Changed
- Repository structure reorganized from a Unity project to a UPM package layout:
  - `Assets/Scripts/` -> `Runtime/`
  - `Assets/Editor/` -> `Editor/`
  - `Assets/3rdparty/` -> `ThirdParty/`
  - `Assets/Shaders/`, `Assets/Materials/`, `Assets/Textures/` -> `Runtime/`
  - `Assets/Resources/` -> `Runtime/Resources/`
  - `Assets/Scenes/` and sample scripts -> `Samples~/BasicDemo/`
  - `DataFiles/` -> `Samples~/SampleData/`
  - `Documentation/` -> `Documentation~/`
  - Development Unity project -> `DevProject~/`

### Fixed
- Removed unused `using UnityEditor;` directives from `DatasetImporterUtility.cs`
  and `ImporterUtilsInternal.cs`.

### Migration Guide
If you were previously using this project by cloning the repository and opening
it as a Unity project, you now have two options:
1. **Recommended**: Install via UPM git URL in your own project.
2. **For development**: Open `DevProject~/` as your Unity project.
