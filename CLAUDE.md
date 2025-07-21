# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MultiRecord is a C# WinForms application for interfacing with SIGLENT SDM3055 digital multimeters over TCP/IP. It provides real-time measurement display, data recording, and export functionality.

## Build and Development Commands

### Building the Project
- **Visual Studio**: Open `MultiRecord.sln` and build using Ctrl+Shift+B or Build menu
- **MSBuild**: `msbuild MultiRecord.sln /p:Configuration=Release`
- **Command Line**: `dotnet build` (if .NET Core compatible)

### Running the Application  
- **Debug**: Press F5 in Visual Studio or use Debug > Start Debugging
- **Release**: Run the compiled executable from `bin/Release/`

### Package Management
- **NuGet Restore**: `nuget restore MultiRecord.sln` or use Visual Studio Package Manager
- **Key Dependencies**: NAudio (2.2.1), Microsoft.Win32.Registry (4.7.0)

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
- **MVVM-like separation**: Form1 acts as controller, SDM3055 as model, with clear data binding
- **Resource management**: Proper disposal patterns for network and audio resources