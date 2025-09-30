# Claude Code Integration Guide for WPFBase

## 🚀 Advanced Claude Code Integration Patterns

This guide provides specific strategies for maximizing productivity with Claude Code in VS Code when working with WPFBase.

## Selection-Based Prompting Patterns

### 1. **Context-Aware Refactoring**
Select code sections and use targeted prompts for intelligent refactoring:

```csharp
// Select a ViewModel and prompt:
"Refactor this to use CommunityToolkit.Mvvm 8.4 partial properties"
"Add resilience patterns using Polly to all async operations"
"Convert to use IAsyncRelayCommand with cancellation support"
```

### 2. **Multi-File Context Loading**
Open related files before prompting for comprehensive context:

```
1. Open: ViewModel, View, Interface, Service
2. Prompt: "Generate comprehensive unit tests covering all interactions"
3. Claude uses all open files for context-aware test generation
```

### 3. **Scaffolding from Selection**
Select an interface or base class, then prompt for implementations:

```csharp
// Select ICustomerService interface
"Generate complete implementation with:
- Polly resilience patterns
- OpenTelemetry instrumentation
- Comprehensive error handling
- Mock service for testing"
```

## Workspace Preparation Strategies

### Optimal File Organization for Claude
```
Open in VS Code (in order):
1. /Interfaces/IYourService.cs      - Contract definition
2. /Services/YourService.cs         - Implementation
3. /ViewModels/YourViewModel.cs     - Consumer
4. /Views/YourView.xaml            - UI binding
5. appsettings.json                - Configuration
```

This order provides Claude with:
- Contract-first understanding
- Implementation details
- Usage patterns
- UI requirements
- Configuration context

## Advanced Prompting Patterns

### 1. **DI Container Registration**
```
"Generate DryIoc registrations for all services in /Services folder"
"Convert Microsoft.Extensions.DI to DryIoc with performance optimizations"
"Add Polly policies to all HTTP client registrations"
```

### 2. **Theme and Style Generation**
```
// Select XAML resource dictionary
"Convert this to WPF UI 4.0.3 Fluent Design"
"Generate dark mode variant maintaining all bindings"
"Create HandyControl alternative styles"
```

### 3. **Chart Logic Synthesis**
```
// Select data model
"Generate ScottPlot 5.0 real-time chart with:
- 60 FPS performance target
- Million point handling
- Pan/zoom with mouse
- Export to PNG/SVG"
```

### 4. **Authentication Scaffolding**
```
"Implement Microsoft.Identity.Web authentication with:
- Azure AD B2C support
- Token refresh handling
- Secure credential storage
- Integration with IAuthenticationService"
```

### 5. **FlaUI Test Generation**
```
// Select a View
"Generate FlaUI automation tests for:
- All button clicks
- Text input validation
- Navigation flows
- Error scenarios"
```

## Terminal Integration Workflows

### Automated Build and Test Cycles
```powershell
# Use Claude to generate and execute:
"Create PowerShell script that:
1. Builds the solution
2. Runs all tests
3. Generates code coverage report
4. Opens report in browser"
```

### Package Management Automation
```
"Update all NuGet packages to latest stable versions and test compatibility"
"Add package with all necessary dependencies and update App.xaml.cs registration"
```

## Multi-Agent Workflows

### Claude Code + GitHub Copilot Strategy
```
1. Use Claude Code for:
   - Architecture decisions
   - Complex refactoring
   - Test generation
   - Documentation

2. Use Copilot for:
   - Line completions
   - Simple patterns
   - Boilerplate code
   - IntelliSense enhancement
```

### Parallel Development Pattern
```
Window 1: Claude Code
- High-level design
- Service implementation
- Complex algorithms

Window 2: Copilot
- UI bindings
- Simple properties
- Event handlers
```

## Integration Examples by Component

### MVVM Toolkit Integration
```
Select: Legacy ViewModel
Prompt: "Modernize using CommunityToolkit.Mvvm 8.4 patterns:
- [ObservableProperty] with notifications
- [RelayCommand] with cancellation
- [NotifyCanExecuteChangedFor]
- Validation attributes"
```

### Polly Resilience Integration
```
Select: Service method
Prompt: "Add Polly resilience:
- Retry with exponential backoff
- Circuit breaker for external calls
- Timeout policies
- Fallback values"
```

### OpenTelemetry Integration
```
Select: Service class
Prompt: "Add comprehensive telemetry:
- Activity tracking with spans
- Custom metrics
- Performance counters
- Distributed tracing"
```

### ScottPlot 5.0 Integration
```
Select: Data collection
Prompt: "Create high-performance chart:
- Real-time updates at 60 FPS
- Million point optimization
- Interactive tooltips
- Custom styling"
```

## Best Practices for Claude Code

### 1. **Prepare Context**
- Open all related files
- Include configuration files
- Show example patterns

### 2. **Be Specific**
- Reference exact versions (ScottPlot 5.0, Polly 8.5)
- Specify performance targets
- Include error handling requirements

### 3. **Iterative Refinement**
- Start with basic implementation
- Add features incrementally
- Request optimizations separately

### 4. **Leverage Templates**
- Create .claude/templates/ folder
- Store common patterns
- Reference in prompts

### 5. **Documentation Integration**
- Request inline documentation
- Generate README sections
- Create API documentation

## Quick Command Reference

### ViewModels
```
"Generate ViewModel with CRUD operations using modern patterns"
"Add batch processing with progress reporting and cancellation"
"Implement infinite scroll with virtualization"
```

### Services
```
"Create service with Polly resilience and OpenTelemetry"
"Add caching layer with memory and distributed cache"
"Implement background service with hosted service pattern"
```

### Testing
```
"Generate comprehensive unit tests with Moq"
"Create FlaUI UI automation tests"
"Add integration tests with WebApplicationFactory"
```

### UI Components
```
"Create responsive grid layout with WPF UI 4.0.3"
"Generate data template selector for polymorphic collections"
"Add drag-drop support with visual feedback"
```

## Performance Optimization Prompts

```
"Optimize for 10,000+ items with virtualization"
"Add lazy loading with pagination"
"Implement memory-efficient data structures"
"Profile and optimize binding performance"
```

## Security Enhancement Prompts

```
"Add input validation and sanitization"
"Implement secure credential storage"
"Add audit logging for sensitive operations"
"Encrypt configuration values"
```

## Troubleshooting with Claude

### Build Errors
```
Select: Error message
Prompt: "Fix this build error maintaining project patterns"
```

### Performance Issues
```
Select: Slow code
Prompt: "Optimize this for better performance using:
- Async/await patterns
- Caching strategies
- Lazy evaluation
- Parallel processing"
```

### Memory Leaks
```
Select: Class
Prompt: "Identify and fix potential memory leaks:
- Event handler cleanup
- Dispose patterns
- Weak references
- Collection clearing"
```

## Measuring Claude Code Impact

Track these metrics to optimize your workflow:

1. **Code Generation Speed**
   - Lines per minute
   - Features per day
   - Bug reduction rate

2. **Pattern Consistency**
   - Style adherence
   - Pattern reuse
   - Documentation completeness

3. **Test Coverage**
   - Auto-generated test percentage
   - Edge case coverage
   - Assertion quality

4. **Refactoring Efficiency**
   - Time saved
   - Code quality improvements
   - Technical debt reduction

---

## 🎯 Result

With these integration patterns, WPFBase + Claude Code enables:
- **10x faster feature development**
- **Consistent code quality**
- **Comprehensive test coverage**
- **Modern pattern adoption**
- **Reduced technical debt**

Use this guide to transform your WPF development workflow with AI-powered productivity.