# Changelog

All notable changes to the Unity Volume Rendering package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2026-11-04

First release distributed as a Unity Package Manager (UPM) package, and first
release under the MingRay name. This release contains breaking changes; see the
migration guide below.

### Added
- Unity Package Manager (UPM) support. Install via git URL:
  `https://github.com/mlavik1/UnityVolumeRendering.git`
- `package.json` with full UPM metadata and samples registration.
- Importable samples: "Basic Demo" (scene + camera controller) and "Sample Data"
  (VisMale raw dataset, PARCHG VASP dataset).
- URP and HDRP support (in addition to the Built-in render pipeline).
- Segmentation support, including multi-segmentation datasets, automatic
  detection of segmentation labels (incl. TotalSegmentator `total_mr` labels)
  and transparent segmentations.
- PET overlay support.
- MingRay panel (`Tools/MingRay/MingRay Panel`) and a welcome window.
- Option for clamping Hounsfield scale values.
- Editor button for normalising datasets.

### Changed
- **Breaking:** Namespace renamed from `UnityVolumeRendering` to `MingRay`.
- **Breaking:** Menu items moved from the top-level `Volume Rendering` menu to
  `Tools/MingRay/` (and `Assets/MingRay/` for asset import menu items).
- **Breaking:** Repository structure reorganized from a Unity project to a UPM
  package layout:
  - `Assets/Scripts/` -> `Runtime/`
  - `Assets/Editor/` -> `Editor/`
  - `Assets/3rdparty/` -> `ThirdParty/`
  - `Assets/Shaders/`, `Assets/Materials/`, `Assets/Textures/` -> `Runtime/`
  - `Assets/Resources/` -> `Runtime/Resources/`
  - `Assets/Scenes/` and sample scripts -> `Samples~/BasicDemo/`
  - `DataFiles/` -> `Samples~/SampleData/`
  - `Documentation/` -> `Documentation~/`
  - Development Unity project -> `SampleProject~/`
- Code is now split into assembly definitions: `UnityVolumeRendering.Runtime`,
  `UnityVolumeRendering.Editor` and `UnityVolumeRendering.3rdParty`.
- Hounsfield scale clamping is now OFF by default.
- Automatic dataset normalisation is disabled (use the editor button instead).
- DICOM orientation is now handled by `SimpleITK.DICOMOrient`, replacing
  `ImportUtilsInternal.ConvertLPSToUnityCoordinateSpace`. Imported datasets may
  be oriented differently than before.
- Gradients are now generated using a Sobel operator, and lighting calculations
  were improved. The default gradient lighting threshold was changed.
  Rendered results will look different from previous versions.
- `TransferFunction`'s texture field is now `protected` instead of `private`.

### Fixed
- Raymarching ray start position (ensure the ray never starts behind the camera).
- Early return on zero alpha no longer skips segmentations.
- Removed unused `using UnityEditor;` directives from `DatasetImporterUtility.cs`
  and `ImporterUtilsInternal.cs`.

### Migration Guide
- Remove the old plugin folder from your project before installing this version,
  to avoid duplicate types.
- Replace `using UnityVolumeRendering;` with `using MingRay;` in your scripts.
- If your own assembly definitions reference this plugin, add references to
  `UnityVolumeRendering.Runtime` (and `UnityVolumeRendering.Editor` for editor code).
- If you were previously cloning the repository into your project, either:
  1. **Recommended**: Install via UPM git URL instead (see README).
  2. Clone (or add as a git submodule) into your project's `Packages/` folder,
     e.g. `Packages/com.mlavik1.mingray`. Unity will pick it up as an embedded,
     editable package.
