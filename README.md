# Chitti: Always-On-Top AI Assistant with Smart Automation

## Overview
Chitti is a Windows WPF application that provides an always-visible horizontal overlay at the top of your screen. It combines system monitoring with instant AI-powered assistance and intelligent automation capabilities using multiple LLM providers.

## Features

### AI Assistant
- **Instant Query**: Type any question in the left chat bubble
- **Single-Line Response**: AI responses are constrained to one line for quick reading
- **Multi-LLM Support**: Gemini, OpenAI GPT, Anthropic Claude, and Ollama integration
- **Smart Personality**: Chitti has a distinct personality defined in JSON configuration

### Smart Automation
- **Natural Language Commands**: Just type what you want - "play despacito", "open notepad", "take screenshot"
- **Heuristic Detection**: Fast offline command recognition for common actions
- **LLM Planning**: AI generates action plans for complex requests
- **Safety Controls**: Risky actions require confirmation, with per-action toggles

### System Monitoring
- **Real-time Clock**: Current time display
- **Battery Status**: Battery percentage and charging status (laptops)
- **Network Status**: WiFi/Ethernet connectivity indicator
- **Dynamic Messages**: Weather, crypto, news, quotes, and system stats

### Window Management
- **Always On Top**: Uses Windows AppBar API to reserve screen space
- **Non-intrusive**: Maximized windows adjust automatically to avoid overlap
- **Minimize to Tray**: Hide the bar without closing the application

## Setup Instructions

### Prerequisites
- Windows 10/11
- .NET 8.0 or later
- At least one LLM API key (Gemini, OpenAI, Claude, or Ollama)

### Installation
1. Clone or download this repository
2. Open the project in Visual Studio or VS Code
3. Restore NuGet packages: `dotnet restore`
4. **Configure API Keys** (at least one required):
   - Open Settings → LLM Settings tab
   - Add your API keys for desired providers:
     - **Gemini**: Get from https://makersuite.google.com/app/apikey
     - **OpenAI**: Get from https://platform.openai.com/api-keys
     - **Anthropic Claude**: Get from https://console.anthropic.com/
     - **Ollama**: Install locally from https://ollama.ai/

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

### Smart Tags

Chitti includes 61 built-in smart tags for instant text processing. Just type your text with a smart tag:

#### Productivity & Writing
- **`/grammar`** - Fix grammar and spelling errors
- **`/tldr`** - Ultra-short summary (one sentence max)
- **`/eli5`** - Explain like I'm 5 (simple explanations)
- **`/proofread`** - Deep proofreading with suggestions
- **`/keywords`** - Extract key terms and phrases
- **`/outline`** - Convert to structured outline
- **`/action`** - Extract action items and tasks
- **`/meeting`** - Format as meeting notes with agenda
- **`/seo`** - Optimize text for SEO with keywords
- **`/summary`** - Create a concise summary
- **`/expand`** - Expand and elaborate on text

#### Communication & Tone
- **`/polite`** - Make text more polite and professional
- **`/formal`** - Convert to formal language
- **`/casual`** - Convert to casual language
- **`/confident`** - Make text more assertive and confident
- **`/empathy`** - Add empathetic and understanding tone
- **`/persuasive`** - Make text more convincing
- **`/apology`** - Format as sincere apology
- **`/congratulate`** - Format as congratulations message
- **`/decline`** - Politely decline or say no

#### Coding & Technical
- **`/code`** - Review and improve code
- **`/debug`** - Analyze code for bugs and issues
- **`/comment`** - Add comprehensive code comments
- **`/optimize`** - Suggest performance optimizations
- **`/test`** - Generate unit tests for code
- **`/explain`** - Explain code in plain English

#### Creative & Fun
- **`/creative`** - Add creativity and flair
- **`/joke`** - Make text humorous and funny
- **`/poem`** - Convert to poem or verse
- **`/story`** - Expand into short story
- **`/emoji`** - Add relevant emojis throughout
- **`/gen-z`** - Rewrite in Gen-Z slang
- **`/pirate`** - Rewrite in pirate speak (fun!)
- **`/shakespeare`** - Rewrite in Shakespearean English

#### Data & Analysis
- **`/compare`** - Compare and contrast items
- **`/pros-cons`** - List pros and cons
- **`/fact-check`** - Verify claims and provide sources
- **`/stats`** - Extract numbers and statistics

#### Advanced Daily Use
- **`/schedule`** - Extract dates, times, and create schedule format
- **`/price`** - Extract pricing info and calculate totals/comparisons
- **`/recipe`** - Format as step-by-step recipe with ingredients
- **`/directions`** - Convert to clear step-by-step directions
- **`/simplify`** - Simplify complex text to basic language
- **`/technical`** - Convert to technical/expert language
- **`/linkedin`** - Optimize for LinkedIn post format
- **`/tweet`** - Condense to Twitter/X format (280 chars)
- **`/hashtags`** - Generate relevant hashtags for social media
- **`/reply`** - Generate appropriate reply to message/email
- **`/bullets`** - Convert to bullet points
- **`/email`** - Format as professional email

#### Smart Processing
- **`/continue`** - Continue/complete the text naturally
- **`/rephrase`** - Rephrase without changing meaning
- **`/questions`** - Generate relevant questions about the topic
- **`/title`** - Generate catchy title/headline
- **`/tags`** - Generate categorization tags
- **`/sentiment`** - Analyze sentiment (positive/negative/neutral)
- **`/readability`** - Improve readability and clarity
- **`/legal`** - Convert to legal/formal document language
- **`/translate`** - Translate to English
- **`/solve`** - Provide solution or answer
- **`/screen`** - Process with screen context

### Smart Automation Commands

Chitti understands natural language commands. Just type what you want:

#### Media & Entertainment
- **"play despacito"** → Auto-plays first YouTube result
- **"play lo-fi beats on spotify"** → Opens Spotify search
- **"play local music"** → Searches your Music folder
- **"search youtube for cooking videos"** → Opens YouTube search

#### Browsing & Search
- **"open google.com"** → Opens URL directly
- **"search for machine learning"** → Google search
- **"search images for cats"** → Google Images search
- **"google weather today"** → Google search

#### System Utilities
- **"take screenshot"** → Captures and opens screenshot
- **"open notepad"** → Launches Notepad
- **"take a note: buy groceries"** → Opens Notepad with text
- **"set timer for 10 minutes"** → Sets system timer
- **"remind me in 30 seconds"** → Quick timer
- **"volume up"** / **"mute volume"** → Volume control
- **"start focus timer for 25 minutes"** → Focus session
- **"take a 5 minute break"** → Break timer

#### File & Folder Management
- **"open downloads"** → Opens Downloads folder
- **"show me my documents"** → Opens Documents folder
- **"reveal file budget.xlsx"** → Finds and shows file in Explorer
- **"launch calculator"** → Opens Calculator app
- **"open paint"** → Launches Paint

#### Advanced Commands
- **"chitti play despacito"** → Forces action mode (bypasses AI chat)
- **"chitti set alarm for 8am"** → Direct command execution

#### Safety Features
- **Risky actions** (lock computer, sleep PC) require confirmation
- **Per-action toggles** in Settings → Automation & Safety
- **Max tabs limit** to prevent browser overload

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
- **Service Layer**: Modular design for AI, automation, and system monitoring
- **Action Registry**: Pluggable action handlers for extensible automation

### Key Components
- `MainWindow`: Primary UI and window management
- `LLMService`: Multi-provider AI communication (Gemini, OpenAI, Claude, Ollama)
- `CommandOrchestrator`: Hybrid command processing (heuristics + LLM planning)
- `ActionService`: Core action implementations
- `SystemMonitorService`: Real-time system information
- `DynamicMessageService`: Live data feeds (weather, crypto, news)
- `Win32`: Native Windows API interop for AppBar functionality

### Automation System
- **Heuristic Detection**: Fast offline pattern matching for common commands
- **LLM Planning**: AI generates structured action plans for complex requests
- **Action Handlers**: Modular `IActionHandler` interface for easy extension
- **Safety Gates**: Per-action toggles and confirmation for risky operations
- **Plan Caching**: Reduces LLM calls with intelligent caching

### API Configuration
- **Multi-LLM Support**: Switch between providers in settings
- **Personality System**: JSON-defined character traits for consistent responses
- **Response Constraints**: Single-line format with character limits
- **Rate Limiting**: Built-in protection against API abuse

## Troubleshooting

### Common Issues
1. **"API Error - Check your API key"**: Verify your API keys in Settings → LLM Settings
2. **Window not staying on top**: Restart the application as administrator
3. **System info not updating**: Check Windows permissions for WMI access
4. **Actions not working**: Check Settings → Automation & Safety for enabled actions
5. **"Action disabled in settings"**: Enable the specific action in settings

### Development
- Build configuration: Release for optimal performance
- Target framework: .NET 8.0 with Windows-specific features
- Dependencies: Newtonsoft.Json, System.Management
- Action development: Implement `IActionHandler` interface for new actions

## Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License
This project is provided as-is for educational and personal use.