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
- **Key Dependencies**: NAudio (2.2.1), Microsoft.Win32.Registry (4.7.0), EPPlus (4.5.3.3)

### Testing
- **No automated tests**: Project currently relies on manual testing with physical SIGLENT SDM3055 hardware
- **Hardware Required**: SIGLENT SDM3055 digital multimeter connected via Ethernet/TCP

## Architecture Overview

### Core Components

**Form1.cs** - Main application window and UI controller
- Handles DMM connection management and measurement display
- Manages measurement recording and data export
- Implements keyboard shortcuts (Ctrl+Q for record/delete)
- Uses async/await pattern for all DMM communications

**SiglentHelper.cs (SDM3055 class)** - SIGLENT DMM communication layer
- TCP/IP socket communication with the multimeter
- Implements SCPI command protocol
- Provides async methods for measurement configuration and data retrieval
- Handles connection management, timeouts, and error recovery
- Supports continuous reading with cancellation tokens

**SoundUtil.cs** - Audio feedback utility
- Uses NAudio library for playing notification sounds
- Provides beep functionality for user feedback during record/delete operations

### Data Management
- CSV-based data storage in AppData folder
- Real-time DataTable binding to DataGridView
- Automatic data persistence and recovery
- Export functionality with timestamp formatting

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
- **MVP Pattern**: Form1 acts as presenter, SDM3055 as model, WinForms as view layer
- **Resource management**: Proper disposal patterns for network and audio resources
- **Observer Pattern**: DMM publishes measurement events, UI subscribes and reacts
- **Command Queuing**: Thread-safe SCPI command execution using SemaphoreSlim

## File Structure and Key Locations

### Project Files
- `MultiRecord.sln` - Visual Studio solution file
- `MultiRecord/MultiRecord.csproj` - Main project file (.NET Framework 4.8)
- `MultiRecord/packages.config` - NuGet package dependencies

### Source Code
- `MultiRecord/Program.cs` - Application entry point
- `MultiRecord/Form1.cs` - Main UI controller and business logic
- `MultiRecord/Form1.Designer.cs` - Auto-generated UI layout code  
- `MultiRecord/SiglentHelper.cs` - DMM communication library (SDM3055 class)
- `MultiRecord/SoundUtil.cs` - Audio feedback utility using NAudio

### Data Storage
- Application data stored in `%AppData%/MultiRecordApp/`
- CSV export files with timestamp-based naming
- Settings persistence using INI-style configuration