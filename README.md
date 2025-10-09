# Line Buddy: Always-On-Top AI Assistant

## Overview
Line Buddy is a Windows WPF application that provides an always-visible horizontal overlay at the top of your screen. It combines system monitoring with instant AI-powered assistance using the Gemini API.

## Features

### AI Assistant
- **Instant Query**: Type any question in the left chat bubble
- **Single-Line Response**: AI responses are constrained to one line for quick reading
- **Gemini Integration**: Powered by Google's Gemini 2.0 Flash model for fast responses

### System Monitoring
- **Real-time Clock**: Current time display
- **Battery Status**: Battery percentage and charging status (laptops)
- **Network Status**: WiFi/Ethernet connectivity indicator

### Window Management
- **Always On Top**: Uses Windows AppBar API to reserve screen space
- **Non-intrusive**: Maximized windows adjust automatically to avoid overlap
- **Minimize to Tray**: Hide the bar without closing the application

## Setup Instructions

### Prerequisites
- Windows 10/11
- .NET 8.0 or later
- Google Gemini API key

### Installation
1. Clone or download this repository
2. Open the project in Visual Studio or VS Code
3. Restore NuGet packages: `dotnet restore`
4. **Important**: Add your Gemini API key:
   - Open `Services/GeminiService.cs`
   - Replace `YOUR_GEMINI_API_KEY_HERE` with your actual API key
   - Get your API key from: https://makersuite.google.com/app/apikey

### Building
```bash
dotnet build
```

### Running
```bash
dotnet run
```

## Usage

### AI Queries
1. Click in the text input on the left side of the bar
2. Type your question (e.g., "What's the weather like?", "Define machine learning")
3. Press **Enter** or click the arrow button (→)
4. The response will appear in the same text box
5. Press **Escape** or click the X button to clear

### System Information
The right side displays:
- Current time (updates every second)
- Battery status with charging indicator
- Network connectivity status

### Window Controls
- **Minimize button (—)**: Hides the bar to system tray
- **Close**: Right-click the system tray icon to exit completely

## Technical Details

### Architecture
- **WPF Frontend**: Modern, transparent UI with rounded corners
- **Win32 Interop**: AppBar registration for proper window management
- **Service Layer**: Modular design for AI and system monitoring

### Key Components
- `MainWindow`: Primary UI and window management
- `GeminiService`: Handles API communication with Gemini
- `SystemMonitorService`: Collects real-time system information
- `Win32`: Native Windows API interop for AppBar functionality

### API Configuration
The Gemini API is configured with strict constraints:
- Maximum 50 output tokens
- Temperature: 0.1 (focused responses)
- System instruction enforces single-line format
- Client-side truncation to 120 characters maximum

## Troubleshooting

### Common Issues
1. **"API Error - Check your API key"**: Verify your Gemini API key is correctly set
2. **Window not staying on top**: Restart the application as administrator
3. **System info not updating**: Check Windows permissions for WMI access

### Development
- Build configuration: Release for optimal performance
- Target framework: .NET 8.0 with Windows-specific features
- Dependencies: Newtonsoft.Json, System.Management

## License
This project is provided as-is for educational and personal use.