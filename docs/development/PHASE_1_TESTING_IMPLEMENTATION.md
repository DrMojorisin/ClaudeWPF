# Phase 1: Testing Infrastructure Implementation Guide

## Overview
Complete implementation guide for expanding WPFBase testing infrastructure from minimal coverage to comprehensive 80%+ coverage with AI-optimized patterns.

## Current State
- ✅ Test project created (`WPFBase.Tests.csproj`)
- ✅ Example test patterns implemented
- ✅ Test data builders created
- 📊 Current coverage: ~5%
- 🎯 Target coverage: 80%+

## Step-by-Step Implementation

### Step 1: Expand ViewModel Test Coverage (Week 1, Days 1-3)

#### 1.1 Create Base Test Class for ViewModels
```csharp
// File: WPFBase.Tests/ViewModels/ViewModelTestBase.cs
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentAssertions;
using WPFBase.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace WPFBase.Tests.ViewModels;

public abstract class ViewModelTestBase<TViewModel> where TViewModel : class
{
    protected Mock<IDialogService> MockDialogService { get; private set; }
    protected Mock<INavigationService> MockNavigationService { get; private set; }
    protected Mock<IMessageBus> MockMessageBus { get; private set; }
    protected Mock<ILoggingService> MockLoggingService { get; private set; }
    protected IServiceProvider ServiceProvider { get; private set; }
    protected TViewModel ViewModel { get; set; }

    [TestInitialize]
    public virtual void TestInitialize()
    {
        // Setup common mocks
        MockDialogService = new Mock<IDialogService>();
        MockNavigationService = new Mock<INavigationService>();
        MockMessageBus = new Mock<IMessageBus>();
        MockLoggingService = new Mock<ILoggingService>();

        // Build service provider
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        // Create view model
        ViewModel = CreateViewModel();
    }

    protected virtual void ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton(MockDialogService.Object);
        services.AddSingleton(MockNavigationService.Object);
        services.AddSingleton(MockMessageBus.Object);
        services.AddSingleton(MockLoggingService.Object);
    }

    protected abstract TViewModel CreateViewModel();

    [TestCleanup]
    public virtual void TestCleanup()
    {
        (ViewModel as IDisposable)?.Dispose();
    }

    // Common test helpers
    protected void VerifyPropertyChanged(string propertyName, Action action)
    {
        var fired = false;
        ((System.ComponentModel.INotifyPropertyChanged)ViewModel).PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == propertyName)
                fired = true;
        };

        action();

        fired.Should().BeTrue($"Property {propertyName} should have raised PropertyChanged");
    }

    protected async Task VerifyCommandExecutionAsync<T>(IAsyncRelayCommand<T> command, T parameter,
        Func<Task> verification = null)
    {
        command.CanExecute(parameter).Should().BeTrue();
        await command.ExecuteAsync(parameter);

        if (verification != null)
            await verification();
    }
}
```

#### 1.2 Implement Tests for Each ViewModel
```csharp
// Template for Claude Code to replicate across all ViewModels
// File: WPFBase.Tests/ViewModels/[ViewModelName]Tests.cs

[TestClass]
public class [ViewModelName]Tests : ViewModelTestBase<[ViewModelName]>
{
    protected override [ViewModelName] CreateViewModel()
    {
        return new [ViewModelName](
            MockDialogService.Object,
            // Add other required services
        );
    }

    [TestMethod]
    public void Constructor_InitializesProperties()
    {
        // Assert
        ViewModel.Should().NotBeNull();
        ViewModel.Title.Should().NotBeNullOrEmpty();
        // Add other property assertions
    }

    [TestMethod]
    [DataRow("", false)]
    [DataRow("valid input", true)]
    public void [PropertyName]_Validation_WorksCorrectly(string value, bool expectedValid)
    {
        // Act
        ViewModel.[PropertyName] = value;

        // Assert
        if (expectedValid)
            ViewModel.HasErrors.Should().BeFalse();
        else
            ViewModel.HasErrors.Should().BeTrue();
    }

    [TestMethod]
    public async Task [CommandName]Command_ExecutesSuccessfully()
    {
        // Arrange
        // Setup any required state

        // Act
        await ViewModel.[CommandName]Command.ExecuteAsync(null);

        // Assert
        // Verify expected behavior
    }

    [TestMethod]
    public void [CommandName]Command_CanExecute_DependsOn[Condition]()
    {
        // Arrange & Act - Test different states
        ViewModel.[Property] = invalidValue;
        var canExecuteInvalid = ViewModel.[CommandName]Command.CanExecute(null);

        ViewModel.[Property] = validValue;
        var canExecuteValid = ViewModel.[CommandName]Command.CanExecute(null);

        // Assert
        canExecuteInvalid.Should().BeFalse();
        canExecuteValid.Should().BeTrue();
    }
}
```

### Step 2: Service Layer Testing (Week 1, Days 3-5)

#### 2.1 Create Service Test Base
```csharp
// File: WPFBase.Tests/Services/ServiceTestBase.cs
public abstract class ServiceTestBase<TService> where TService : class
{
    protected Mock<ILoggingService> MockLoggingService { get; private set; }
    protected TService Service { get; set; }

    [TestInitialize]
    public virtual void TestInitialize()
    {
        MockLoggingService = new Mock<ILoggingService>();
        Service = CreateService();
    }

    protected abstract TService CreateService();

    // Helper methods for common service testing patterns
    protected void VerifyLogging(LogLevel level, string messageContains, Times times)
    {
        MockLoggingService.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == level),
                It.Is<string>(m => m.Contains(messageContains))),
            times);
    }
}
```

#### 2.2 Service Implementation Tests
```csharp
// File: WPFBase.Tests/Services/NavigationServiceTests.cs
[TestClass]
public class NavigationServiceTests : ServiceTestBase<NavigationService>
{
    private Mock<IServiceProvider> _mockServiceProvider;

    protected override NavigationService CreateService()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        return new NavigationService(_mockServiceProvider.Object, MockLoggingService.Object);
    }

    [TestMethod]
    public async Task NavigateToAsync_ValidViewModel_NavigatesSuccessfully()
    {
        // Arrange
        var mockViewModel = new Mock<TestViewModel>();
        _mockServiceProvider.Setup(x => x.GetService(typeof(TestViewModel)))
            .Returns(mockViewModel.Object);

        // Act
        var result = await Service.NavigateToAsync<TestViewModel>();

        // Assert
        result.Should().BeTrue();
        Service.CurrentViewModel.Should().Be(mockViewModel.Object);
    }

    [TestMethod]
    public async Task NavigateToAsync_ServiceNotRegistered_ReturnsFalse()
    {
        // Arrange
        _mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>()))
            .Returns(null);

        // Act
        var result = await Service.NavigateToAsync<TestViewModel>();

        // Assert
        result.Should().BeFalse();
        VerifyLogging(LogLevel.Error, "Failed to resolve", Times.Once());
    }

    [TestMethod]
    public async Task GoBackAsync_WithHistory_NavigatesToPrevious()
    {
        // Arrange
        var vm1 = new TestViewModel();
        var vm2 = new TestViewModel();
        _mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>()))
            .Returns((Type t) => Activator.CreateInstance(t));

        await Service.NavigateToAsync<TestViewModel>();
        await Service.NavigateToAsync<TestViewModel>();

        // Act
        var result = await Service.GoBackAsync();

        // Assert
        result.Should().BeTrue();
        Service.CanGoBack.Should().BeFalse();
    }
}
```

### Step 3: Integration Testing (Week 2, Days 1-2)

#### 3.1 Create Integration Test Base
```csharp
// File: WPFBase.Tests/Integration/IntegrationTestBase.cs
public abstract class IntegrationTestBase
{
    protected IServiceProvider ServiceProvider { get; private set; }
    protected IServiceCollection Services { get; private set; }

    [TestInitialize]
    public virtual void TestInitialize()
    {
        Services = new ServiceCollection();
        ConfigureServices(Services);
        ServiceProvider = Services.BuildServiceProvider();
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Add real services for integration testing
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IMessageBus, MessageBus>();
        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton<IUserSettingsService, UserSettingsService>();

        // Add test implementations where needed
        services.AddSingleton<IDialogService, TestDialogService>();
    }

    protected T GetService<T>() => ServiceProvider.GetRequiredService<T>();

    [TestCleanup]
    public virtual void TestCleanup()
    {
        (ServiceProvider as IDisposable)?.Dispose();
    }
}
```

#### 3.2 End-to-End Scenario Tests
```csharp
// File: WPFBase.Tests/Integration/NavigationScenarioTests.cs
[TestClass]
public class NavigationScenarioTests : IntegrationTestBase
{
    [TestMethod]
    public async Task CompleteNavigationFlow_WorksEndToEnd()
    {
        // Arrange
        var navigationService = GetService<INavigationService>();
        var messageBus = GetService<IMessageBus>();
        var messagesReceived = new List<NavigationMessage>();

        messageBus.Subscribe<NavigationMessage>(msg => messagesReceived.Add(msg));

        // Act - Navigate through multiple views
        await navigationService.NavigateToAsync<HomeViewModel>();
        await navigationService.NavigateToAsync<SettingsViewModel>(new { tab = "general" });
        await navigationService.GoBackAsync();

        // Assert
        messagesReceived.Should().HaveCount(3);
        navigationService.CurrentViewModel.Should().BeOfType<HomeViewModel>();
        navigationService.CanGoBack.Should().BeFalse();
    }
}
```

### Step 4: Performance Benchmarks (Week 2, Day 3)

#### 4.1 Create Benchmark Suite
```csharp
// File: WPFBase.Tests/Benchmarks/ViewModelBenchmarks.cs
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, targetCount: 5)]
public class ViewModelBenchmarks
{
    private PartialPropertiesExampleViewModel _viewModel;
    private List<string> _testData;

    [GlobalSetup]
    public void Setup()
    {
        var mockDialog = new Mock<IDialogService>().Object;
        var mockPerf = new Mock<IPerformanceOptimizationService>().Object;
        _viewModel = new PartialPropertiesExampleViewModel(mockDialog, mockPerf, "test");
        _testData = Enumerable.Range(0, 1000).Select(i => $"Item {i}").ToList();
    }

    [Benchmark]
    public void PropertyChange_SingleUpdate()
    {
        _viewModel.Email = "test@example.com";
    }

    [Benchmark]
    public void PropertyChange_WithValidation()
    {
        _viewModel.Email = "test@example.com";
        _viewModel.ValidateProperty(nameof(_viewModel.Email));
    }

    [Benchmark]
    public async Task Command_AsyncExecution()
    {
        await _viewModel.SaveCommand.ExecuteAsync(null);
    }

    [Benchmark]
    public void Collection_BulkAdd()
    {
        _viewModel.Items.Clear();
        foreach (var item in _testData)
        {
            _viewModel.Items.Add(item);
        }
    }
}

// Run benchmarks in test
[TestClass]
public class PerformanceTests
{
    [TestMethod]
    [TestCategory("Performance")]
    public void RunBenchmarks()
    {
        var summary = BenchmarkRunner.Run<ViewModelBenchmarks>();

        // Assert performance thresholds
        var propertyChangeBenchmark = summary.Reports
            .First(r => r.BenchmarkCase.Descriptor.WorkloadMethodDisplayInfo.Contains("PropertyChange"));

        propertyChangeBenchmark.ResultStatistics.Mean
            .Should().BeLessThan(1000); // nanoseconds
    }
}
```

### Step 5: Test Coverage and Reporting (Week 2, Days 4-5)

#### 5.1 Configure Code Coverage
```xml
<!-- Add to WPFBase.Tests.csproj -->
<PropertyGroup>
  <CollectCoverage>true</CollectCoverage>
  <CoverletOutputFormat>cobertura</CoverletOutputFormat>
  <CoverletOutput>./TestResults/</CoverletOutput>
  <ExcludeFromCodeCoverage>
    WPFBase.Tests.*;
    *.xaml.cs;
    *Program.cs;
  </ExcludeFromCodeCoverage>
</PropertyGroup>
```

#### 5.2 Create Test Runner Script
```powershell
# File: run-tests.ps1
param(
    [string]$Configuration = "Debug",
    [switch]$Coverage,
    [switch]$Benchmarks
)

Write-Host "Running WPFBase Tests..." -ForegroundColor Green

# Clean previous results
Remove-Item -Path ".\TestResults" -Recurse -ErrorAction SilentlyContinue

# Run tests with coverage
if ($Coverage) {
    dotnet test `
        --configuration $Configuration `
        --collect:"XPlat Code Coverage" `
        --results-directory:".\TestResults" `
        --logger:"html;LogFileName=test-results.html" `
        -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura

    # Generate coverage report
    reportgenerator `
        -reports:".\TestResults\**\coverage.cobertura.xml" `
        -targetdir:".\TestResults\CoverageReport" `
        -reporttypes:"Html;Badges"

    Start-Process ".\TestResults\CoverageReport\index.html"
} else {
    dotnet test --configuration $Configuration
}

# Run benchmarks if requested
if ($Benchmarks) {
    dotnet run -c Release --project WPFBase.Tests --filter "Category=Performance"
}

Write-Host "Tests completed!" -ForegroundColor Green
```

### Step 6: Continuous Testing Integration

#### 6.1 Add Testing to GitHub Actions
```yaml
# File: .github/workflows/test.yml
name: Test

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: windows-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --no-restore

    - name: Test with coverage
      run: |
        dotnet test --no-build --verbosity normal `
          --collect:"XPlat Code Coverage" `
          --results-directory:"./TestResults"

    - name: Upload coverage to Codecov
      uses: codecov/codecov-action@v3
      with:
        file: ./TestResults/**/coverage.cobertura.xml
        fail_ci_if_error: false

    - name: Test Report
      uses: dorny/test-reporter@v1
      if: success() || failure()
      with:
        name: Test Results
        path: 'TestResults/**/*.trx'
        reporter: dotnet-trx
```

## Implementation Checklist

### Week 1 Tasks
- [ ] Create ViewModelTestBase class
- [ ] Implement tests for all existing ViewModels
  - [ ] PartialPropertiesExampleViewModel ✅
  - [ ] MainWindowViewModel
  - [ ] HomeViewModel
  - [ ] SettingsViewModel
  - [ ] TestViewModel
- [ ] Create ServiceTestBase class
- [ ] Implement tests for core services
  - [ ] NavigationService
  - [ ] DialogService
  - [ ] MessageBus
  - [ ] UserSettingsService
  - [ ] ThemeService

### Week 2 Tasks
- [ ] Create IntegrationTestBase class
- [ ] Implement end-to-end scenario tests
- [ ] Setup performance benchmarks
- [ ] Configure code coverage reporting
- [ ] Create test automation scripts
- [ ] Setup CI/CD test pipeline
- [ ] Document testing guidelines

## Success Metrics

### Coverage Goals
- **Unit Tests**: 80% line coverage
- **Integration Tests**: All major user workflows
- **Performance Tests**: Baseline metrics established

### Quality Metrics
- **Test Execution Time**: < 30 seconds for full suite
- **Test Maintainability**: All tests follow consistent patterns
- **False Positives**: < 1% flaky tests

### Documentation
- **Test Patterns**: Documented and templatized
- **Claude Code Templates**: Generated for common test scenarios
- **Coverage Reports**: Automated and accessible

## Claude Code Integration

### Test Generation Commands
```
"Generate unit tests for CustomerViewModel with 80% coverage"
"Create integration tests for the order processing workflow"
"Add performance benchmarks for data loading operations"
"Generate mock data builders for Product entity"
```

### Pattern Templates for Claude
1. **ViewModel Test Template**: Copy from ViewModelTestBase examples
2. **Service Test Template**: Copy from ServiceTestBase examples
3. **Integration Test Template**: Copy from IntegrationTestBase examples
4. **Benchmark Template**: Copy from ViewModelBenchmarks

### Automation Benefits
- Claude can generate tests following established patterns
- Consistent test structure across entire codebase
- Automatic test discovery and organization
- Built-in best practices for async testing

## Next Steps

After completing Phase 1:
1. Review coverage reports to identify gaps
2. Add tests for edge cases and error conditions
3. Establish performance baselines
4. Create testing documentation for team
5. Move to Phase 2: Performance Monitoring Implementation

---

*This implementation guide ensures comprehensive test coverage while maintaining patterns that Claude Code can easily replicate and extend.*