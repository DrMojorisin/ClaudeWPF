# WPFBase Template Usage Guide

## Quick Start - Using WPFBase as a Template

This guide helps you use WPFBase as a foundation for new WPF applications with Claude Code.

## Step 1: Copy WPFBase to Your Project

### Option A: Fresh Project
1. Copy entire WPFBase folder to your new project location
2. Rename the folder to your project name (e.g., `MyApp`)
3. Update namespace and project references:
   - Rename `WPFBase.csproj` to `MyApp.csproj`
   - Update `<StartupObject>` in .csproj from `WPFBase.App` to `MyApp.App`
   - Use Find & Replace: `WPFBase` → `MyApp` across all `.cs` and `.xaml` files

### Option B: Existing Project
1. Copy all folders EXCEPT `bin`, `obj`, and `.vs` to your existing project
2. Merge the PackageReference entries from `WPFBase.csproj` into your `.csproj`
3. Copy `App.xaml` and `App.xaml.cs` (or merge dependency injection setup)
4. Update namespaces as needed

## Step 2: Initial Setup Checklist

### ✅ Required Actions
- [ ] Update project namespace from `WPFBase` to your namespace
- [ ] Update `AssemblyName` and `RootNamespace` in .csproj
- [ ] Update window title in `MainWindow.xaml`
- [ ] Update application name in `App.xaml` resources
- [ ] Clear sample ViewModels/Views (keep base classes)
- [ ] Update `README.md` with your project information
- [ ] Initialize git repository if needed

### 🎯 Optional Customization
- [ ] Modify theme colors in `/Themes/`
- [ ] Update application icon in `/Resources/`
- [ ] Configure default layout in `MainViewModel`
- [ ] Adjust logging configuration in `App.xaml.cs`
- [ ] Set up application settings structure

## Step 3: Project Structure Overview

```
YourProject/
├── Commands/          → Custom ICommand implementations
├── Controls/          → Reusable user controls
├── Converters/        → XAML value converters
├── Data/              → Repository pattern, data access
├── Extensions/        → Extension methods
├── Interfaces/        → Service contracts
├── Models/            → Business models, DTOs
├── Resources/         → Images, icons, assets
├── Services/          → Service implementations
├── Themes/            → Application themes
├── Validators/        → FluentValidation rules
├── ViewModels/        → MVVM ViewModels
│   ├── Documents/     → Document-based VMs
│   └── Tools/         → Tool window VMs
├── Views/             → XAML views
│   ├── Documents/     → Document views
│   └── Tools/         → Tool windows
└── Tests/             → Unit tests
```

## Step 4: Working with Claude Code

### Tell Claude Code About Your Project
When starting work with Claude Code, inform it:
```
I'm using the WPFBase framework. Please check CLAUDE.md for framework documentation.
```

### Common Claude Code Commands

#### Create New Feature
```
Create a customer management feature with:
- Customer list view with search
- Customer detail form with validation
- CRUD operations using repository pattern
```

#### Add New Service
```
Add an email notification service that:
- Sends emails using SMTP
- Supports templates
- Logs all email activities
```

#### Create Document Type
```
Create a markdown editor document type that:
- Supports syntax highlighting
- Has preview pane
- Can save/load .md files
```

## Step 5: Essential Configuration

### Database Connection (if needed)
1. Add connection string to `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyApp;Trusted_Connection=true;"
  }
}
```

2. Register DbContext in `App.xaml.cs`:
```csharp
services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
```

### API Integration (if needed)
1. Add base URL to configuration
2. Register HttpClient in DI:
```csharp
services.AddHttpClient<IApiService, ApiService>(client =>
{
    client.BaseAddress = new Uri(configuration["Api:BaseUrl"]);
});
```

## Step 6: Development Workflow

### 1. Planning Phase
- Use TodoWrite tool for task management
- Break features into small, testable components
- Follow MVVM pattern strictly

### 2. Implementation Phase
- Create ViewModels with `[ObservableProperty]` attributes
- Use `[RelayCommand]` for command generation
- Implement services with interfaces
- Add validation rules as needed

### 3. Testing Phase
- Write unit tests for ViewModels
- Test services with mocked dependencies
- Verify navigation flows
- Test validation rules

### 4. Integration Phase
- Wire up views to ViewModels
- Configure dependency injection
- Set up data bindings
- Test end-to-end scenarios

## Common Tasks Quick Reference

### Add Menu Item
```csharp
// In MainViewModel
MenuItems.Add(new MenuItem 
{ 
    Header = "New Feature",
    Command = NewFeatureCommand 
});
```

### Add Tool Window
```csharp
await _dockingService.ShowToolAsync(
    new MyToolViewModel(),
    "Tool Title",
    AnchorableShowStrategy.Left);
```

### Add Keyboard Shortcut
```csharp
_keyboardShortcutService.RegisterShortcut(
    "MyAction",
    Key.N,
    ModifierKeys.Control | ModifierKeys.Shift,
    async () => await ExecuteActionAsync());
```

### Show Dialog
```csharp
var result = await _dialogService.ShowDialogAsync<MyDialogViewModel, MyResult>(param);
```

### Navigate to View
```csharp
await _navigationService.NavigateToAsync<MyViewModel>(parameter);
```

## Troubleshooting

### Issue: Build fails after renaming
**Solution**: Clean solution, delete `bin`/`obj` folders, rebuild

### Issue: Commands not generating
**Solution**: Ensure partial class, build to trigger source generators

### Issue: Services not injecting
**Solution**: Check registration in `App.xaml.cs` ConfigureServices

### Issue: Binding not working
**Solution**: Check DataContext, use [ObservableProperty] attribute

### Issue: Theme not applying
**Solution**: Verify theme resource dictionary merged in App.xaml

## Best Practices

1. **Always use ViewModelBase** - Don't create ViewModels from scratch
2. **Register all services** - Add to DI container in App.xaml.cs
3. **Use async/await** - For all I/O operations
4. **Implement IDisposable** - When holding event subscriptions
5. **Write tests first** - Especially for complex ViewModels
6. **Use MessageBus** - For decoupled component communication
7. **Validate user input** - Use DataAnnotations or FluentValidation
8. **Log important operations** - Use ILoggingService
9. **Handle exceptions** - Don't let exceptions bubble to UI
10. **Follow MVVM** - No code-behind except for pure UI logic

## Next Steps

1. Review `CLAUDE.md` for detailed framework documentation
2. Explore example implementations in ViewModels folder
3. Run the application to see the framework in action
4. Start building your features on top of this foundation

## Support

- Framework issues: Check CLAUDE.md documentation
- Claude Code usage: Refer to Claude Code documentation
- Pattern guidance: Follow examples in the framework

Remember: This framework is designed for rapid development. Let Claude Code handle the boilerplate while you focus on business logic!