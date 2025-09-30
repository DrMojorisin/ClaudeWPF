# Dependency Injection Container Architecture

## Current Implementation: Microsoft.Extensions.DependencyInjection (Primary)

### Executive Summary

WPFBase uses **Microsoft.Extensions.DependencyInjection** as the primary DI container throughout the codebase (see `App.xaml.cs`). **DryIoc** is included as an optional package for developers who need advanced features, but it is **not used by default**.

---

## Why Microsoft.Extensions.DI is Primary

### 1. Industry Standard Across .NET Ecosystem
- **Same DI system** used in ASP.NET Core, MAUI, Worker Services, Azure Functions
- **Zero learning curve** for .NET developers - if you know .NET, you know this container
- **Consistent patterns** across all .NET workloads (web, desktop, mobile, cloud)

### 2. Excellent Tooling Support
- **Full IntelliSense** in Visual Studio and VS Code
- **Analyzer support** for detecting DI registration issues
- **Debugging support** with service provider inspection
- **Official Microsoft documentation** and support

### 3. Claude Code Compatibility
- **AI understands** Microsoft.Extensions.DI patterns perfectly
- **Training data** includes extensive Microsoft.Extensions.DI examples
- **Consistent generation** of registration patterns
- **No hallucinations** - Claude generates correct patterns first try

### 4. Simplicity and Clarity
```csharp
// Clear, simple, standard .NET pattern
services.AddSingleton<IService, Service>();
services.AddTransient<IViewModel, ViewModel>();
services.AddScoped<IRepository, Repository>();
```

---

## Why DryIoc is Included (But Optional)

### Package Reference
```xml
<!-- WPFBase.csproj line 74 -->
<PackageReference Include="DryIoc.dll" Version="5.4.3" />
```

### Advanced Scenarios Where DryIoc Excels

#### 1. Decorator Pattern
```csharp
// DryIoc has native decorator support
container.Register<IOrderService, OrderService>(Reuse.Singleton);
container.Register<IOrderService, LoggingOrderServiceDecorator>(setup: Setup.Decorator);
container.Register<IOrderService, CachingOrderServiceDecorator>(setup: Setup.Decorator);
// Automatically chains: CachingOrderServiceDecorator -> LoggingOrderServiceDecorator -> OrderService
```

**Microsoft.Extensions.DI equivalent** (requires manual setup):
```csharp
services.AddSingleton<OrderService>();
services.AddSingleton<IOrderService>(sp =>
{
    var orderService = sp.GetRequiredService<OrderService>();
    var loggingDecorator = new LoggingOrderServiceDecorator(orderService);
    return new CachingOrderServiceDecorator(loggingDecorator);
});
```

#### 2. Interceptors and AOP (Aspect-Oriented Programming)
```csharp
// DryIoc + Castle.DynamicProxy for cross-cutting concerns
container.Register<IUserService, UserService>(
    made: Made.Of(
        () => Castle.DynamicProxy.Create.InterfaceProxy<IUserService>(
            Arg.Of<IUserService>(),
            Arg.Of<LoggingInterceptor>())));
```

**Microsoft.Extensions.DI**: Not supported - requires external library like Castle.Windsor or manual proxy creation

#### 3. Performance Critical Applications
**Benchmark Results** (from DryIoc documentation):
- DryIoc: 294ms (single-threaded) / 219ms (multi-threaded)
- Microsoft.Extensions.DI: 363ms (single-threaded) / 242ms (multi-threaded)
- **DryIoc is ~20% faster** in resolution performance

For applications resolving services millions of times per second, this matters. For typical WPF applications resolving services on startup and navigation, the difference is negligible (< 100ms total).

#### 4. Child Containers and Multi-Tenancy
```csharp
// DryIoc supports child containers with fast O(1) snapshots
var tenantContainer = container.CreateChild();
tenantContainer.Register<ITenantData>(() => new TenantData(tenantId));
```

**Microsoft.Extensions.DI**: Requires creating entirely new service providers (slower, more memory)

#### 5. Convention-Based Registration
```csharp
// DryIoc can scan assemblies and register by convention
container.RegisterMany(
    new[] { typeof(IService).Assembly },
    serviceTypeCondition: type => type.IsInterface && type.Name.StartsWith("I"),
    reuse: Reuse.Transient);
```

**Microsoft.Extensions.DI**: Requires explicit registration or third-party library like Scrutor

---

## Migration Path: Microsoft.Extensions.DI → DryIoc

If you need DryIoc's advanced features, see **`DRYIOC_CLAUDE_EXPLOITABLE_PATTERNS.md`** for complete migration guide.

### Quick Migration Example

**Before (Microsoft.Extensions.DI):**
```csharp
// App.xaml.cs
protected override void OnStartup(StartupEventArgs e)
{
    var serviceCollection = new ServiceCollection();
    ConfigureServices(serviceCollection);
    _serviceProvider = serviceCollection.BuildServiceProvider();
}

private void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IDialogService, DialogService>();
    services.AddTransient<MainViewModel>();
}
```

**After (DryIoc):**
```csharp
// App.xaml.cs
using DryIoc;

protected override void OnStartup(StartupEventArgs e)
{
    var container = new Container(rules => rules
        .WithAutoConcreteTypeResolution()
        .WithDefaultIfAlreadyRegistered(IfAlreadyRegistered.Replace));

    ConfigureServices(container);
    _container = container;
}

private void ConfigureServices(IContainer container)
{
    container.Register<IDialogService, DialogService>(Reuse.Singleton);
    container.Register<MainViewModel>(Reuse.Transient);
}
```

**Hybrid Approach (Best of Both Worlds):**
```csharp
// App.xaml.cs
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;

protected override void OnStartup(StartupEventArgs e)
{
    var container = new Container();
    var serviceCollection = new ServiceCollection();

    ConfigureServices(serviceCollection);

    // Use DryIoc as the backing container for Microsoft.Extensions.DI
    _serviceProvider = container.WithDependencyInjectionAdapter(serviceCollection).BuildServiceProvider();
}
```

---

## Recommendation by Project Type

### Use Microsoft.Extensions.DI (Current Default) For:
✅ **Standard WPF applications** - startup + navigation service resolution
✅ **Teams new to DI** - familiar .NET patterns
✅ **Integration with ASP.NET Core** - shared service registrations
✅ **Maximum tooling support** - VS IntelliSense and analyzers
✅ **Claude Code development** - AI generates perfect patterns

### Migrate to DryIoc For:
🔧 **Decorator-heavy architectures** - logging, caching, validation decorators
🔧 **AOP requirements** - cross-cutting concerns via interceptors
🔧 **Multi-tenant applications** - child containers per tenant
🔧 **Performance-critical resolution** - millions of resolutions per second
🔧 **Convention-based registration** - automatic assembly scanning

---

## Documentation Alignment Status

### ✅ Updated Files
- **MARKETING.md**: Added "Dependency Injection Architecture" section explaining choice
- **modern_wpf_tooling_research.md**: Updated DI container comparisons and tables
- **This document**: Created comprehensive architecture explanation

### 📚 DryIoc Reference Documentation (Preserved)
These documents remain available for developers who choose to migrate:
- **DRYIOC_CLAUDE_EXPLOITABLE_PATTERNS.md**: Complete DryIoc integration patterns
- **DRYIOC_QUICK_REFERENCE.md**: Quick reference for DryIoc syntax
- **DRYIOC_PROMPT_TEMPLATES.md**: Claude Code prompts for DryIoc

### 🎯 Primary Implementation
- **App.xaml.cs**: Uses `Microsoft.Extensions.DependencyInjection` (lines 2-3, 33-35)
- **WPFBase.csproj**: Includes both packages (Microsoft.Extensions.DI line 30, DryIoc.dll line 74)

---

## Conclusion

**Microsoft.Extensions.DependencyInjection is the right choice** for WPFBase's primary implementation:
- Industry standard with excellent tooling
- Perfect Claude Code compatibility
- Zero learning curve for .NET developers
- Sufficient for 95% of WPF application needs

**DryIoc remains available** as an advanced option for the 5% of applications that need decorators, AOP, or maximum performance.

**Documentation now consistently reflects this architecture**, with clear migration paths for developers who need DryIoc's advanced features.