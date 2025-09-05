# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MultiRecord is a C# WinForms application for interfacing with SIGLENT SDM3055 digital multimeters over TCP/IP. It provides real-time measurement display, data recording, and export functionality.

## Build and Development Commands

### Building the Project
- **Visual Studio**: Open `MultiRecord.sln` and build using Ctrl+Shift+B or Build menu
- **MSBuild**: `msbuild MultiRecord.sln /p:Configuration=Release`
- **Command Line**: `dotnet build MultiRecord.sln` (targets .NET Framework 4.8)

### Running the Application  
- **Debug**: Press F5 in Visual Studio or use Debug > Start Debugging
- **Release**: Run the compiled executable from `bin/Release/`

### Package Management
- **NuGet Restore**: `nuget restore MultiRecord.sln` or use Visual Studio Package Manager
- **Key Dependencies**: NAudio (2.2.1) for audio, EPPlus (4.5.3.3) for Excel export, Microsoft.Win32.Registry (4.7.0)

### Testing
- **No automated tests**: Project currently relies on manual testing with physical SIGLENT SDM3055 hardware
- **Hardware Required**: SIGLENT SDM3055 digital multimeter connected via Ethernet/TCP

## Architecture Overview

### Core Components

**Main.cs** - Main application window and UI controller (renamed from Form1.cs)
- Handles DMM connection management and measurement display
- Manages measurement recording and data export with tolerance checking
- Implements keyboard shortcuts (Ctrl+Q for record/delete)
- Uses async/await pattern for all DMM communications
- Integrates settings management and audio feedback systems

**SiglentHelper.cs (SDM3055 class)** - SIGLENT DMM communication layer
- TCP/IP socket communication with the multimeter
- Implements SCPI command protocol
- Provides async methods for measurement configuration and data retrieval
- Handles connection management, timeouts, and error recovery
- Supports continuous reading with cancellation tokens

**SoundUtil.cs** - Audio feedback utility
- Uses NAudio library for playing notification sounds
- Provides beep functionality for user feedback during record/delete operations

**SettingsManager.cs** - Settings persistence and management
- Handles INI-style configuration storage in AppData folder
- Manages custom sound file paths and application preferences
- Provides static access to application settings

**SettingsForm.cs** - Settings configuration dialog
- UI for configuring custom sound files and application preferences
- File browser integration for selecting custom audio files
- Sound testing functionality for immediate feedback

### Data Management
- CSV-based data storage in AppData folder (`%AppData%/MultiRecordApp/`)
- Real-time DataTable binding to DataGridView with auto-sizing columns
- Automatic data persistence and recovery on application restart
- Export functionality with timestamp formatting
- Tolerance checking system with configurable limits and target values

### Measurement Functions
The application supports multiple measurement types via `MeasurementFunction` enum:
- Voltage (DC/AC), Current (DC/AC), Resistance (2W/4W)
- Capacitance, Frequency, Temperature, Diode testing
- Each function has specific parameter controls (range, speed/NPLC, relative mode)

### Communication Protocol
- Uses SCPI commands over TCP port 5025 (default)
- Implements command queuing with semaphore for thread safety
- Automatic connection recovery and error handling
- Configurable timeouts for connection and read operations

## Key Design Patterns

- **Async/Await**: All DMM operations are asynchronous to prevent UI blocking
- **Event-driven**: Uses events for connection status and measurement data updates  
- **MVP Pattern**: Main form acts as presenter, SDM3055 as model, WinForms as view layer
- **Resource management**: Proper disposal patterns for network and audio resources using IDisposable
- **Observer Pattern**: DMM publishes measurement events, UI subscribes and reacts
- **Command Queuing**: Thread-safe SCPI command execution using SemaphoreSlim
- **Static Factory**: SettingsManager provides centralized configuration management
- **Separation of Concerns**: UI, business logic, hardware communication, and settings are clearly separated

## File Structure and Key Locations

### Project Files
- `MultiRecord.sln` - Visual Studio solution file
- `MultiRecord/MultiRecord.csproj` - Main project file (.NET Framework 4.8)
- `MultiRecord/packages.config` - NuGet package dependencies

### Source Code
- `MultiRecord/Program.cs` - Application entry point and Windows Forms initialization
- `MultiRecord/Main.cs` - Main UI controller and business logic (previously Form1.cs)
- `MultiRecord/Main.Designer.cs` - Auto-generated UI layout code (previously Form1.Designer.cs)
- `MultiRecord/SiglentHelper.cs` - DMM communication library (SDM3055 class)
- `MultiRecord/SoundUtil.cs` - Audio feedback utility using NAudio
- `MultiRecord/SettingsManager.cs` - Settings persistence and configuration management
- `MultiRecord/SettingsForm.cs` - Settings dialog UI and functionality

### Data Storage
- Application data stored in `%AppData%/MultiRecordApp/`
- `measurement_data.csv` - Persistent measurement records
- `settings.ini` - Application settings and preferences
- CSV export files with timestamp-based naming
- Custom sound files (beep.mp3, delete.mp3, over.mp3) in `sound/` directory