---
layout: default
title: WPFBase - Modern WPF MVVM Framework
description: Production-ready WPF MVVM framework with modern .NET practices and comprehensive architecture patterns
---

# WPFBase - Modern WPF MVVM Framework

<div class="quick-links">
  <h3>🚀 Quick Start</h3>
  <div style="text-align: center; margin: 20px 0;">
    <a href="{{ '/getting-started' | relative_url }}" class="cta-button">Get Started</a>
    <a href="{{ site.repository }}" class="cta-button secondary" target="_blank">View on GitHub</a>
  </div>
</div>

A **production-ready WPF MVVM framework** built with modern .NET practices and comprehensive architecture patterns.

## ✨ Key Features

<div class="feature-grid">
  <div class="feature-card">
    <h3><span class="feature-icon">⚡</span> Modern MVVM</h3>
    <p>Built with <strong>CommunityToolkit.Mvvm 8.4.0</strong> source generators for automatic property and command generation. No more boilerplate code!</p>
    <div>
      <span class="badge">Source Generators</span>
      <span class="badge">ObservableProperty</span>
      <span class="badge">RelayCommand</span>
    </div>
  </div>

  <div class="feature-card">
    <h3><span class="feature-icon">🏗️</span> Rich Architecture</h3>
    <p>Comprehensive service layer with dependency injection, navigation, dialogs, themes, and docking windows.</p>
    <div>
      <span class="badge">Dependency Injection</span>
      <span class="badge">AvalonDock</span>
      <span class="badge">Service Layer</span>
    </div>
  </div>

  <div class="feature-card">
    <h3><span class="feature-icon">🎨</span> Professional UI</h3>
    <p>Visual Studio-style docking, light/dark themes, custom controls, and modern design patterns.</p>
    <div>
      <span class="badge">Dark Mode</span>
      <span class="badge">Docking Windows</span>
      <span class="badge">Custom Controls</span>
    </div>
  </div>

  <div class="feature-card">
    <h3><span class="feature-icon">✅</span> Validation Ready</h3>
    <p>Built-in support for both DataAnnotations and FluentValidation with automatic error handling.</p>
    <div>
      <span class="badge">DataAnnotations</span>
      <span class="badge">FluentValidation</span>
      <span class="badge">Error Handling</span>
    </div>
  </div>

  <div class="feature-card">
    <h3><span class="feature-icon">🧪</span> Test Coverage</h3>
    <p>Comprehensive unit tests with xUnit and Moq. CI/CD pipeline with automated testing.</p>
    <div>
      <span class="badge">xUnit</span>
      <span class="badge">Moq</span>
      <span class="badge">GitHub Actions</span>
    </div>
  </div>

  <div class="feature-card">
    <h3><span class="feature-icon">📚</span> Documentation</h3>
    <p>Complete documentation with step-by-step guides, examples, and best practices.</p>
    <div>
      <span class="badge">Getting Started</span>
      <span class="badge">API Reference</span>
      <span class="badge">Examples</span>
    </div>
  </div>
</div>

## 🏗️ Core Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| **.NET** | 9.0 | Modern C# with latest features |
| **CommunityToolkit.Mvvm** | 8.4.0 | Source generators for MVVM |
| **AvalonDock** | 4.72.1 | VS-style docking windows |
| **Serilog** | 4.1.0 | Structured logging |
| **FluentValidation** | 11.10.0 | Business rule validation |
| **Microsoft.Extensions.DI** | 9.0.0 | Dependency injection |

## 🎯 Perfect For

- **Enterprise Desktop Applications** - Professional business apps
- **Document-Based Applications** - With docking and tabbed interfaces
- **Configuration Tools** - Settings and admin utilities
- **Data Management Apps** - CRUD operations with validation
- **Rapid Prototyping** - Production-ready code from day one

## 💻 Quick Example

Here's how easy it is to create a feature with WPFBase:

```csharp
public partial class CustomerViewModel : ViewModelBase
{
    private readonly ICustomerService _customerService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Customer> customers = new();

    public CustomerViewModel(ICustomerService customerService, IDialogService dialogService)
    {
        _customerService = customerService;
        _dialogService = dialogService;
        Title = "Customer Management";
    }

    [RelayCommand]
    private async Task LoadCustomersAsync()
    {
        IsBusy = true;
        try
        {
            var data = await _customerService.GetAllAsync();
            Customers.Clear();
            foreach (var item in data) Customers.Add(item);
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Failed to load customers", "Error", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

## 🚀 Next Steps

<div style="text-align: center; margin: 40px 0;">
  <a href="{{ '/getting-started' | relative_url }}" class="cta-button">📖 Read Getting Started Guide</a>
  <a href="{{ '/architecture' | relative_url }}" class="cta-button">🏗️ Learn Architecture</a>
  <a href="{{ '/examples' | relative_url }}" class="cta-button">💡 See Examples</a>
</div>

## 🤝 Community

- **[GitHub Repository]({{ site.repository }})** - Source code, issues, and releases
- **[Discussions]({{ site.repository }}/discussions)** - Community Q&A and ideas
- **[Contributing Guide]({{ site.repository }}/blob/main/CONTRIBUTING.md)** - How to contribute
- **[Issues]({{ site.repository }}/issues)** - Bug reports and feature requests

---

**Built with ❤️ for the WPF community** | **[MIT License]({{ site.repository }}/blob/main/LICENSE)** | **[Version {{ site.wpfbase.version }}]({{ site.repository }}/releases)**