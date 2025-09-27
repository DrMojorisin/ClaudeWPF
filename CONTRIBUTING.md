# Contributing to WPFBase

First off, thank you for considering contributing to WPFBase! It's people like you that make WPFBase such a great framework.

## Code of Conduct

By participating in this project, you are expected to uphold our simple code of conduct:
- Be respectful and inclusive
- Welcome newcomers and help them get started
- Focus on constructive criticism
- Accept feedback gracefully

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check existing issues as you might find out that you don't need to create one. When you are creating a bug report, please include as many details as possible using our bug report template.

**Great Bug Reports** tend to have:
- A quick summary and/or background
- Steps to reproduce
  - Be specific!
  - Give sample code if you can
- What you expected would happen
- What actually happens
- Notes (possibly including why you think this might be happening, or stuff you tried that didn't work)

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion, please use the feature request template and include:
- A clear and descriptive title
- A detailed description of the proposed feature
- Code examples of how the feature would be used
- Explain why this enhancement would be useful

### Pull Requests

1. **Fork the repo** and create your branch from `main`
2. **Follow the existing code style** - We use modern C# with CommunityToolkit.Mvvm patterns
3. **Add tests** if you've added code that should be tested
4. **Ensure the test suite passes** - Run `dotnet test`
5. **Update documentation** - Make sure the README.md and relevant folder READMEs are updated
6. **Write a good commit message** - Follow conventional commits if possible

## Development Setup

1. **Prerequisites**
   - .NET 9.0 SDK
   - Visual Studio 2022 / VS Code / Rider
   - Git

2. **Getting Started**
   ```bash
   git clone https://github.com/DrMojorisin/ClaudeWPF.git
   cd ClaudeWPF
   dotnet restore
   dotnet build
   dotnet test
   ```

3. **Project Structure**
   - Follow the existing folder structure
   - Each folder has a README explaining its purpose
   - Keep related code together

## Coding Guidelines

### C# Style Guide

1. **Use Modern Patterns**
   ```csharp
   // ✅ Good - Modern pattern
   [ObservableProperty]
   private string name = string.Empty;

   // ❌ Avoid - Old pattern
   private string _name;
   public string Name
   {
       get => _name;
       set => SetProperty(ref _name, value);
   }
   ```

2. **Dependency Injection**
   - Define interfaces in `/Interfaces`
   - Implement in `/Services`
   - Register in `App.xaml.cs`

3. **MVVM Pattern**
   - ViewModels inherit from `ViewModelBase`
   - Use `[RelayCommand]` for commands
   - Keep Views simple with minimal code-behind

4. **Async/Await**
   ```csharp
   // Always use async/await for I/O operations
   [RelayCommand]
   private async Task LoadDataAsync()
   {
       IsBusy = true;
       try
       {
           await DataService.LoadAsync();
       }
       finally
       {
           IsBusy = false;
       }
   }
   ```

5. **Error Handling**
   - Use try-catch in commands
   - Log errors with ILoggingService
   - Show user-friendly messages with IDialogService

### Documentation

- Add XML comments to public APIs
- Update folder README files when adding new components
- Include usage examples in documentation
- Keep the GETTING_STARTED.md up to date

### Testing

1. **Unit Tests Required For:**
   - All services
   - ViewModels with business logic
   - Converters and validators
   - Extension methods

2. **Test Naming Convention:**
   ```csharp
   [Fact]
   public async Task MethodName_StateUnderTest_ExpectedBehavior()
   {
       // Arrange
       // Act
       // Assert
   }
   ```

3. **Use Mocking:**
   ```csharp
   var mockDialog = new Mock<IDialogService>();
   mockDialog.Setup(x => x.ShowInformationAsync(It.IsAny<string>(), It.IsAny<string>()))
             .ReturnsAsync(true);
   ```

## Commit Messages

We prefer conventional commits but it's not required:
- `feat:` New feature
- `fix:` Bug fix
- `docs:` Documentation changes
- `style:` Code style changes (formatting, etc)
- `refactor:` Code refactoring
- `test:` Test additions or changes
- `chore:` Maintenance tasks

Example:
```
feat: add dark mode support to theme service

- Implemented dark theme resources
- Added theme toggle command
- Updated documentation
```

## Review Process

1. **Automated checks** must pass (build, tests)
2. **Code review** by maintainers
3. **Documentation** must be updated
4. **Tests** must be included for new features

## Where to Get Help

- **Documentation**: Start with README.md and GETTING_STARTED.md
- **Issues**: Check existing issues or create a new one
- **Discussions**: Use GitHub Discussions for questions
- **Code Examples**: Look at existing implementations in the codebase

## Recognition

Contributors will be recognized in:
- GitHub contributors page
- Release notes for significant contributions
- Special mentions for exceptional contributions

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing to WPFBase! 🎉