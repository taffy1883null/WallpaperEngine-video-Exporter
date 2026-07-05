[English](README.md) | [简体中文](README-CN.md)
# Wallpaper Engine Exporter CLI

A lightweight, minimalist, and efficient C# command-line tool for interactively scanning and exporting video files from Steam Wallpaper Engine.

Say goodbye to the confusing, hard-to-remember "numeric-only" folder names found in the Workshop. This tool automatically parses `project.json` to extract actual wallpaper titles and provides an interactive interface allowing you to select and batch-export the source `.mp4` video files.

---

## Quick Start
- Simply download and install the ZIP file from the Releases section.

---

## Features
- **Fully Interactive CLI Interface**: Supports navigation (up/down), single selection, and "select all" functionality.
- **Smart Name Resolution**: Automatically reads `project.json` to map folder IDs to the actual wallpaper titles assigned by the original creators.
- **Safe Export & Renaming**:
  - Automatically filters out characters that are invalid for filenames (e.g., `<>|?*`).
  - Smart handling of duplicate filenames by automatically appending sequence numbers (e.g., `Wallpaper(1).mp4`) to avoid overwriting existing files.
  - 100% read-only scanning; it does not modify, delete, or corrupt your Steam library assets.
- **Zero External Dependencies**: Built using pure C# and native console APIs; no third-party libraries required.

---

## Requirements

- **.NET 8.0 SDK** or later
- Operating System: Windows (relies on default local Steam paths)

---

## Configuration
- The program comes with the following default paths configured:
  - Steam Workshop wallpaper directory: `D:\SteamLibrary\steamapps\workshop\content\431960`
  - Export directory: `D:\WallpaperEngineExport`
