# Contributing to Chitti

Thank you for your interest in contributing to Chitti! This guide will help you get started with development and contributing to the project.

## Development Setup

### Prerequisites
- Windows 10/11
- .NET 8.0 SDK or later
- Visual Studio 2022 or VS Code with C# extension
- Git

### Getting Started
1. **Fork the repository** on GitHub
2. **Clone your fork**:
   ```bash
   git clone https://github.com/yourusername/Chitti_2.0.git
   cd Chitti_2.0
   ```
3. **Restore dependencies**:
   ```bash
   dotnet restore
   ```
4. **Build the project**:
   ```bash
   dotnet build
   ```
5. **Run the application**:
   ```bash
   dotnet run
   ```

## Project Structure

```
Chitti_2.0/
├── Services/                    # Core services
│   ├── Actions/                # Action handlers (IActionHandler implementations)
│   │   ├── IActionHandler.cs   # Action interface
│   │   ├── OpenUrlAction.cs    # URL opening
│   │   ├── SearchWebAction.cs  # Web search
│   │   └── ...                 # Other action handlers
│   ├── CommandOrchestrator.cs  # Command processing and routing
│   ├── LLMService.cs          # Multi-provider AI integration
│   ├── ActionService.cs       # Core action implementations
│   └── ...                    # Other services
├── MainWindow.xaml             # Main UI
├── SettingsWindow.xaml         # Settings UI
├── AppSettings.cs             # Configuration management
└── personality.json           # Chitti's personality definition
```

## Architecture Overview

### Action System
Chitti uses a modular action system where each action is implemented as an `IActionHandler`:

```csharp
public interface IActionHandler
{
    string Name { get; }           // Unique action identifier
    bool IsRisky { get; }          // Whether action requires confirmation
    Task<string> ExecuteAsync(JObject args);  // Execute the action
}
```

### Command Processing Flow
1. **Heuristic Detection**: Fast offline pattern matching
2. **LLM Planning**: AI generates structured action plans
3. **Action Execution**: Routes to appropriate `IActionHandler`
4. **Safety Gates**: Confirmation for risky actions

## Adding New Actions

### Step 1: Create Action Handler
Create a new file in `Services/Actions/`:

```csharp
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Chitti.Services.Actions
{
    public class MyNewAction : IActionHandler
    {
        private readonly ActionService _actionService;
        
        public string Name => "myNewAction";
        public bool IsRisky => false;  // Set to true if risky
        
        public MyNewAction(ActionService actionService)
        {
            _actionService = actionService;
        }
        
        public async Task<string> ExecuteAsync(JObject args)
        {
            var param = args.Value<string>("param");
            // Implement your action logic here
            return "Action completed successfully!";
        }
    }
}
```

### Step 2: Register Handler
Add to `CommandOrchestrator.RegisterHandlers()`:

```csharp
RegisterHandler(new MyNewAction(_actions));
```

### Step 3: Add Heuristics (Optional)
Add pattern matching in `CommandOrchestrator.HandleAsync()`:

```csharp
if (Regex.IsMatch(input, @"my new action", RegexOptions.IgnoreCase))
{
    var args = new JObject { ["param"] = "value" };
    var msg = await ExecuteAsync("myNewAction", args);
    return (true, msg, false, null);
}
```

### Step 4: Update Settings UI
Add toggles in `SettingsWindow.xaml` Automation & Safety tab:

```xml
<CheckBox Content="My New Action: description" 
          IsChecked="{Binding EnableMyNewAction}" 
          Foreground="LightGray"/>
```

### Step 5: Add Settings Property
Add to `AppSettings.cs`:

```csharp
public bool EnableMyNewAction { get; set; } = true;
```

### Step 6: Update Settings Gating
Add to `CommandOrchestrator.IsActionEnabledBySettings()`:

```csharp
case "mynewaction":
    return _settings.EnableMyNewAction;
```

## Code Style Guidelines

### C# Conventions
- Use PascalCase for public members
- Use camelCase for private fields
- Use async/await for asynchronous operations
- Use meaningful variable names
- Add XML documentation for public methods

### XAML Conventions
- Use consistent indentation (4 spaces)
- Group related properties together
- Use descriptive element names
- Follow WPF data binding best practices

### File Organization
- One class per file
- Namespace matches folder structure
- Use regions for large classes
- Keep methods focused and small

## Testing Your Changes

### Build Verification
Always build after changes:
```bash
dotnet build
```

### Manual Testing
1. **Test the action** with various inputs
2. **Verify settings** work correctly
3. **Check error handling** with invalid inputs
4. **Test safety features** for risky actions

### Common Test Cases
- Valid command execution
- Invalid parameter handling
- Settings enable/disable
- Error message clarity
- Confirmation prompts (for risky actions)

## Pull Request Guidelines

### Before Submitting
1. **Build successfully** with no errors
2. **Test thoroughly** with various scenarios
3. **Update documentation** if needed
4. **Follow code style** guidelines
5. **Add comments** for complex logic

### PR Description Template
```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Documentation update
- [ ] Code refactoring

## Testing
- [ ] Built successfully
- [ ] Tested manually
- [ ] No breaking changes

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-review completed
- [ ] Documentation updated
- [ ] No console warnings
```

## Areas for Contribution

### High Priority
- **New Action Handlers**: Expand automation capabilities
- **UI Improvements**: Better settings, error messages
- **Performance**: Optimize command processing
- **Error Handling**: Better error messages and recovery

### Medium Priority
- **Documentation**: Improve guides and examples
- **Testing**: Add unit tests for services
- **Accessibility**: Improve UI accessibility
- **Localization**: Multi-language support

### Low Priority
- **Themes**: Dark/light mode options
- **Plugins**: External action plugin system
- **Analytics**: Usage statistics (privacy-focused)
- **Advanced Features**: Complex automation workflows

## Development Workflow

### Feature Development
1. **Create feature branch**: `git checkout -b feature/my-new-action`
2. **Implement changes**: Follow coding guidelines
3. **Test thoroughly**: Manual testing and build verification
4. **Commit changes**: Use descriptive commit messages
5. **Push and create PR**: Submit for review

### Bug Fixes
1. **Create bug branch**: `git checkout -b fix/issue-description`
2. **Reproduce issue**: Understand the problem
3. **Implement fix**: Minimal changes to fix the issue
4. **Test fix**: Verify the issue is resolved
5. **Submit PR**: Include issue number in description

## Code Review Process

### For Contributors
- **Respond to feedback** promptly
- **Make requested changes** clearly
- **Ask questions** if feedback is unclear
- **Test changes** after addressing feedback

### For Reviewers
- **Be constructive** in feedback
- **Test the changes** if possible
- **Explain reasoning** for suggestions
- **Approve when ready** for merge

## Getting Help

### Documentation
- **README.md**: Project overview and setup
- **USER_GUIDE.md**: User documentation
- **Code comments**: Inline documentation

### Community
- **GitHub Issues**: Bug reports and feature requests
- **Discussions**: Questions and ideas
- **Pull Requests**: Code contributions

### Development Questions
- **Architecture**: How the action system works
- **API Integration**: Adding new LLM providers
- **UI Development**: WPF and XAML best practices
- **Windows APIs**: Win32 interop and AppBar functionality

## License

By contributing to Chitti, you agree that your contributions will be licensed under the same terms as the project.

---

**Thank you for contributing to Chitti!** 🤖✨

Your contributions help make Chitti more powerful and useful for everyone.
