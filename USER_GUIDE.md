# Chitti User Guide

## Getting Started

### First Launch
1. **Install Chitti** following the setup instructions in README.md
2. **Configure API Keys** in Settings → LLM Settings (at least one required)
3. **Customize Actions** in Settings → Automation & Safety
4. **Start using** by typing in the text box!

### Understanding the Interface
- **Left Side**: Text input for queries and commands
- **Center**: Chitti's logo and status
- **Right Side**: System information (time, battery, network)

## Basic Usage

### AI Chat
Simply type any question and press Enter:
- "What's the weather like?"
- "Explain quantum computing"
- "How do I fix a slow computer?"

### Smart Commands
Chitti understands natural language commands. No need to memorize syntax!

## Command Categories

### 🎵 Media & Entertainment

#### Playing Music/Videos
- **"play despacito"** → Auto-plays first YouTube result
- **"play lo-fi beats"** → Searches and plays on YouTube
- **"play despacito on spotify"** → Opens Spotify search
- **"play local music"** → Searches your Music folder
- **"search youtube for cooking videos"** → Opens YouTube search

#### Tips for Media Commands
- Be specific about the platform: "on spotify", "on youtube"
- For local music, Chitti will fuzzy-match filenames
- YouTube auto-play works without any API keys

### 🌐 Browsing & Search

#### Web Navigation
- **"open google.com"** → Opens URL directly
- **"open github.com"** → Direct URL opening
- **"search for machine learning"** → Google search
- **"google weather today"** → Google search
- **"search images for cats"** → Google Images search
- **"bing artificial intelligence"** → Bing search

#### Search Tips
- Chitti automatically detects search vs. direct URL
- Use "search images for..." for image searches
- Specify search engine: "google", "bing", "duckduckgo"

### 💻 System Utilities

#### Screenshots & Notes
- **"take screenshot"** → Captures and opens screenshot
- **"open notepad"** → Launches Notepad
- **"take a note: buy groceries"** → Opens Notepad with text
- **"write down: meeting at 3pm"** → Creates note

#### Timers & Reminders
- **"set timer for 10 minutes"** → Sets system timer
- **"remind me in 30 seconds"** → Quick timer
- **"timer 5m"** → Short form timer
- **"alarm in 1 hour"** → Hourly reminder

#### Volume Control
- **"volume up"** → Increases system volume
- **"volume down"** → Decreases system volume
- **"mute volume"** → Mutes system
- **"unmute volume"** → Unmutes system

#### Focus & Productivity
- **"start focus timer for 25 minutes"** → Focus session
- **"take a 5 minute break"** → Break timer
- **"focus for 1 hour"** → Long focus session
- **"break time"** → Quick break

### 📁 File & Folder Management

#### Opening Folders
- **"open downloads"** → Opens Downloads folder
- **"show me my documents"** → Opens Documents folder
- **"open music folder"** → Opens Music folder
- **"open pictures"** → Opens Pictures folder

#### Finding Files
- **"reveal file budget.xlsx"** → Finds and shows file in Explorer
- **"show me report.pdf"** → Locates and reveals file
- **"find presentation.pptx"** → Searches and opens location

#### Launching Apps
- **"launch calculator"** → Opens Calculator app
- **"open paint"** → Launches Paint
- **"start notepad"** → Opens Notepad
- **"open calculator"** → Calculator app

### ⚠️ Advanced Commands

#### Force Action Mode
- **"chitti play despacito"** → Forces action mode (bypasses AI chat)
- **"chitti set alarm for 8am"** → Direct command execution
- **"chitti take screenshot"** → Immediate action

#### Risky Actions (Require Confirmation)
- **"lock computer"** → Locks the screen (confirmation required)
- **"sleep pc"** → Puts computer to sleep (confirmation required)
- **"shutdown computer"** → Shuts down (confirmation required)

## Settings & Customization

### Automation & Safety Tab
- **Enable/Disable Actions**: Toggle specific action categories
- **Max Open Tabs**: Limit browser tabs to prevent overload
- **Risky Actions**: Allow/block dangerous operations
- **Action Categories**:
  - Media: YouTube, Spotify, local music
  - Search: Web, images, various engines
  - System: Screenshots, notes, timers, volume
  - Files: Folders, file search, app launch

### LLM Settings Tab
- **API Keys**: Configure your preferred AI providers
- **Model Selection**: Choose specific models for each provider
- **Response Settings**: Customize AI behavior

### General Tab
- **Window Height**: Adjust bar height (8% of screen by default)
- **Typing Animation**: Enable/disable text animation
- **Message Types**: Choose dynamic message categories

## Tips & Best Practices

### Command Efficiency
1. **Be Natural**: "play despacito" works better than "execute play command for despacito"
2. **Use Specifics**: "play lo-fi beats on spotify" is clearer than "play music"
3. **Try Variations**: "take screenshot", "capture screen", "screenshot" all work

### Troubleshooting Commands
- **Not Working?** Check Settings → Automation & Safety for enabled actions
- **Wrong Action?** Try being more specific or use "chitti" prefix
- **Confirmation Needed?** Risky actions require explicit confirmation

### Performance Tips
- **Heuristic Commands**: Work instantly (no AI processing)
- **LLM Commands**: May take 1-2 seconds for complex requests
- **Caching**: Similar commands are cached for faster response

## Common Use Cases

### Daily Workflow
1. **Morning**: "open downloads" → check new files
2. **Work**: "start focus timer for 25 minutes" → productivity session
3. **Break**: "play lo-fi beats" → background music
4. **Notes**: "take a note: call client at 2pm" → quick reminder
5. **End Day**: "take screenshot" → save work progress

### Entertainment
1. **Music**: "play [song name]" → instant YouTube playback
2. **Videos**: "search youtube for [topic]" → discover content
3. **Local Media**: "play local [artist]" → your music collection

### Productivity
1. **Research**: "search for [topic]" → quick web search
2. **Screenshots**: "take screenshot" → capture important info
3. **Timers**: "set timer for [duration]" → time management
4. **Notes**: "take a note: [text]" → quick documentation

## Safety & Privacy

### Safe Actions (Auto-execute)
- Media playback
- Web searches
- File/folder operations
- Screenshots
- Notes and timers
- Volume control

### Risky Actions (Require Confirmation)
- Lock computer
- Sleep/shutdown
- System modifications

### Privacy
- No data is stored permanently
- API keys are encrypted in settings
- Screenshots saved locally only
- No telemetry or tracking

## Getting Help

### Built-in Help
- Type "help" for command suggestions
- Check Settings → Automation & Safety for available actions
- Review this guide for command examples

### Common Issues
- **"Action disabled in settings"** → Enable the action in settings
- **"API Error"** → Check your API keys in LLM Settings
- **Commands not working** → Restart Chitti and try again

### Support
- Check the README.md for technical details
- Review troubleshooting section
- Ensure you have at least one API key configured

---

**Happy automating with Chitti!** 🤖✨
