# DryIoc Quick Reference for Claude Code

> **See Full Documentation**: `DRYIOC_CLAUDE_EXPLOITABLE_PATTERNS.md`

## Installation

```xml
<PackageReference Include="DryIoc.dll" Version="5.4.3" />
<PackageReference Include="DryIoc.Microsoft.DependencyInjection" Version="6.2.0" />
<PackageReference Include="Castle.Core" Version="5.1.1" /> <!-- For AOP -->
```

---

## Basic Container Setup

```csharp
var container = new Container(rules => rules
    .WithoutThrowOnRegisteringDisposableTransient()
    .WithTrackingDisposableTransients()
    .WithFactorySelector(Rules.SelectLastRegisteredFactory())
    .WithConstructorWithResolvableArguments()
    .WithAutoConcreteTypeResolution()
);
```

---

## Registration Patterns

### Lifetime Scopes

```csharp
// Singleton - one instance per container
container.Register<IService, Service>(Reuse.Singleton);

// Transient - new instance every resolve
container.Register<IService, Service>(Reuse.Transient);

// Scoped - one instance per scope
container.Register<IService, Service>(Reuse.Scoped);
```

### Multiple Implementations (Keyed)

```csharp
container.Register<ITheme, LightTheme>(serviceKey: "Light");
container.Register<ITheme, DarkTheme>(serviceKey: "Dark");

var theme = container.Resolve<ITheme>(serviceKey: "Dark");
var all = container.Resolve<IEnumerable<ITheme>>();
```

### Open Generics

```csharp
container.Register(typeof(IRepository<>), typeof(Repository<>), Reuse.Scoped);
// Auto-resolves: IRepository<Customer>, IRepository<Order>, etc.
```

### Convention-Based (Assembly Scanning)

```csharp
// Register all services in namespace
container.RegisterMany(
    typeof(App).Assembly.GetTypes()
        .Where(t => t.Namespace?.Contains("Services") == true),
    serviceTypeCondition: type => type.IsInterface,
    reuse: Reuse.Singleton
);

// Register all ViewModels
container.RegisterMany(
    typeof(App).Assembly.GetTypes()
        .Where(t => t.Name.EndsWith("ViewModel")),
    reuse: Reuse.Transient
);
```

---

## Decorator Patterns

### Basic Decorator

```csharp
public class LoggingDecorator : IService
{
    private readonly IService _inner;
    private readonly ILogger _logger;

    public LoggingDecorator(IService inner, ILogger logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task DoWorkAsync()
    {
        _logger.LogInformation("Starting work");
        await _inner.DoWorkAsync();
        _logger.LogInformation("Work completed");
    }
}

// Registration
container.Register<IService, Service>(Reuse.Singleton);
container.Register<IService, LoggingDecorator>(
    Reuse.Singleton,
    setup: Setup.Decorator
);
```

### Multiple Decorators (Order: innermost first)

```csharp
container.Register<IService, Service>(Reuse.Singleton);
container.Register<IService, ValidationDecorator>(setup: Setup.Decorator);  // 1st wrap
container.Register<IService, LoggingDecorator>(setup: Setup.Decorator);     // 2nd wrap
container.Register<IService, CachingDecorator>(setup: Setup.Decorator);     // 3rd wrap (outer)
// Execution: Cache -> Log -> Validate -> Service
```

### Conditional Decorator

```csharp
container.Register<IService, Service>(Reuse.Singleton);
container.Register<IService, CachingDecorator>(
    setup: Setup.DecoratorWith(r => r.ServiceKey?.Equals("cached") == true)
);
```

---

## AOP Interceptors (Castle DynamicProxy)

### Setup Extension Method

```csharp
using Castle.DynamicProxy;

public static class InterceptorExtensions
{
    private static readonly ProxyGenerator ProxyGenerator = new();

    public static void Intercept<TService, TInterceptor>(this IContainer container)
        where TService : class
        where TInterceptor : class, IInterceptor
    {
        container.Register<TService>(
            made: Made.Of(
                r => ServiceInfo.Of<TService>(),
                f => ProxyGenerator.CreateInterfaceProxyWithTarget<TService>(
                    f.Arg<TService>(),
                    f.Arg<TInterceptor>())
            ),
            setup: Setup.DecoratorWith(useDecorateeReuse: false));
    }
}
```

### Logging Interceptor

```csharp
public class LoggingInterceptor : IInterceptor
{
    private readonly ILogger _logger;

    public LoggingInterceptor(ILogger logger) => _logger = logger;

    public void Intercept(IInvocation invocation)
    {
        var method = $"{invocation.TargetType?.Name}.{invocation.Method.Name}";
        _logger.LogDebug("Calling {Method}", method);

        var sw = Stopwatch.StartNew();
        invocation.Proceed();
        sw.Stop();

        _logger.LogDebug("Completed {Method} in {Ms}ms", method, sw.ElapsedMilliseconds);
    }
}

// Registration
container.Register<IUserService, UserService>(Reuse.Singleton);
container.Register<LoggingInterceptor>(Reuse.Singleton);
container.Intercept<IUserService, LoggingInterceptor>();
```

---

## Child Containers

### Basic Child Container

```csharp
var childContainer = container.CreateChild(
    RegistrySharing.Share  // Share parent registrations
);

// Override in child
childContainer.Register<IService, ChildService>(Reuse.Singleton);
```

### Multi-Tenancy

```csharp
public class TenantContainerFactory
{
    private readonly IContainer _root;
    private readonly ConcurrentDictionary<string, IContainer> _tenants = new();

    public TenantContainerFactory(IContainer root) => _root = root;

    public IContainer GetTenantContainer(string tenantId)
    {
        return _tenants.GetOrAdd(tenantId, id =>
        {
            var tenant = _root.CreateChild(RegistrySharing.Share, defaultServiceKey: id);
            tenant.RegisterInstance(new TenantContext { Id = id }, serviceKey: id);
            return tenant;
        });
    }
}
```

---

## Testing Patterns

### Basic Test Container

```csharp
[TestClass]
public class ServiceTests
{
    private IContainer _container;
    private Mock<IDependency> _depMock;

    [TestInitialize]
    public void Setup()
    {
        _container = new Container();
        _depMock = new Mock<IDependency>();

        _container.RegisterInstance<IDependency>(_depMock.Object);
        _container.Register<IService, Service>(Reuse.Transient);
    }

    [TestCleanup]
    public void Cleanup() => _container.Dispose();

    [TestMethod]
    public async Task Test_ServiceMethod()
    {
        _depMock.Setup(d => d.GetDataAsync()).ReturnsAsync("test");

        var service = _container.Resolve<IService>();
        var result = await service.ProcessAsync();

        Assert.AreEqual("test", result);
    }
}
```

---

## Claude Code Prompts

### Add Service with Decorator

```
Add IOrderService with OrderService implementation.
Apply logging and telemetry decorators.
Register as singleton.
```

### Convention Registration

```
Register all services in Services.Payment namespace.
Match by interface (I prefix), singleton lifetime.
```

### Add Caching Decorator

```
Create caching decorator for IProductService.
Cache GetProductAsync for 5 minutes.
Invalidate on UpdateProductAsync.
```

### Add AOP Interceptor

```
Add Castle DynamicProxy logging to IPaymentService.
Log method entry/exit, duration, exceptions.
```

### Multi-Tenant Setup

```
Create multi-tenant IDataService using child containers.
Each tenant gets isolated registration.
Provide factory to manage tenant containers.
```

### Open Generic Handler

```
Register ICommandHandler<TCommand, TResult> as open generic.
Create handlers for CreateUserCommand and UpdateUserCommand.
```

---

## Migration from Microsoft.Extensions.DI

### Registration Equivalents

| Microsoft.Extensions.DI | DryIoc |
|-------------------------|--------|
| `services.AddSingleton<I, C>()` | `container.Register<I, C>(Reuse.Singleton)` |
| `services.AddTransient<I, C>()` | `container.Register<I, C>(Reuse.Transient)` |
| `services.AddScoped<I, C>()` | `container.Register<I, C>(Reuse.Scoped)` |
| `serviceProvider.GetRequiredService<T>()` | `container.Resolve<T>()` |
| `serviceProvider.GetService<T>()` | `container.Resolve<T>(IfUnresolved.ReturnDefault)` |

### Container Setup Replacement

**Before (MS.DI):**
```csharp
var services = new ServiceCollection();
ConfigureServices(services);
var serviceProvider = services.BuildServiceProvider();
```

**After (DryIoc):**
```csharp
var container = new Container(rules => rules
    .WithoutThrowOnRegisteringDisposableTransient()
    .WithTrackingDisposableTransients()
);
ConfigureServices(container);
// Use container directly
```

---

## Common Decorators Library

### 1. Logging

```csharp
public class LoggingDecorator<T> where T : class
{
    private readonly T _inner;
    private readonly ILogger _logger;

    public LoggingDecorator(T inner, ILogger logger)
    {
        _inner = inner;
        _logger = logger;
    }
}
```

### 2. Caching

```csharp
public class CachingDecorator<T> where T : class
{
    private readonly T _inner;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _expiration;

    public CachingDecorator(T inner, IMemoryCache cache, TimeSpan? expiration = null)
    {
        _inner = inner;
        _cache = cache;
        _expiration = expiration ?? TimeSpan.FromMinutes(5);
    }
}
```

### 3. Validation

```csharp
public class ValidationDecorator<T> where T : class
{
    private readonly T _inner;
    private readonly IValidator _validator;

    public ValidationDecorator(T inner, IValidator validator)
    {
        _inner = inner;
        _validator = validator;
    }
}
```

### 4. Telemetry

```csharp
public class TelemetryDecorator<T> where T : class
{
    private readonly T _inner;
    private readonly ITelemetryService _telemetry;

    public TelemetryDecorator(T inner, ITelemetryService telemetry)
    {
        _inner = inner;
        _telemetry = telemetry;
    }
}
```

### 5. Retry/Resilience

```csharp
public class ResilientDecorator<T> where T : class
{
    private readonly T _inner;
    private readonly ResiliencePipeline _pipeline;

    public ResilientDecorator(T inner, IResilienceService resilience)
    {
        _inner = inner;
        _pipeline = resilience.CreatePipeline(retryCount: 3);
    }
}
```

### 6. Authorization

```csharp
public class AuthorizationDecorator<T> where T : class
{
    private readonly T _inner;
    private readonly IAuthorizationService _auth;

    public AuthorizationDecorator(T inner, IAuthorizationService auth)
    {
        _inner = inner;
        _auth = auth;
    }
}
```

### 7. Transaction

```csharp
public class TransactionalDecorator<T> where T : class
{
    private readonly T _inner;
    private readonly IUnitOfWork _unitOfWork;

    public TransactionalDecorator(T inner, IUnitOfWork unitOfWork)
    {
        _inner = inner;
        _unitOfWork = unitOfWork;
    }
}
```

### 8. Performance Monitoring

```csharp
public class PerformanceDecorator<T> where T : class
{
    private readonly T _inner;
    private readonly IPerformanceOptimizationService _perf;

    public PerformanceDecorator(T inner, IPerformanceOptimizationService perf)
    {
        _inner = inner;
        _perf = perf;
    }
}
```

---

## Performance Tips

### 1. Pre-Compile Frequently Used Services

```csharp
container.Register<IUserService, UserService>(Reuse.Singleton);
container.Resolve<IUserService>();  // Force compilation at startup
```

### 2. Use Lazy<T> for Expensive Services

```csharp
public MyViewModel(Lazy<IExpensiveService> expensive)
{
    _expensive = expensive;
}

// Service created only when accessed
_expensive.Value.DoWork();
```

### 3. Batch Registration

```csharp
// Faster than individual Register calls
container.RegisterMany(
    typeof(App).Assembly.GetTypes().Where(t => t.Namespace == "Services"),
    reuse: Reuse.Singleton
);
```

---

## Troubleshooting

### Circular Dependencies

```csharp
// Use Lazy<T>
public ServiceA(Lazy<ServiceB> serviceB) { }
public ServiceB(Lazy<ServiceA> serviceA) { }
```

### Decorator Order Issues

```csharp
// Order: innermost first, last registered = outermost
container.Register<I, Impl>(Reuse.Singleton);
container.Register<I, Decorator1>(setup: Setup.Decorator);  // Inner
container.Register<I, Decorator2>(setup: Setup.Decorator);  // Outer
```

### Scoped in Singleton

```csharp
// Use Func<IContainer> for scope creation
public SingletonService(Func<IContainer> scopeFactory)
{
    using var scope = scopeFactory().OpenScope();
    var scoped = scope.Resolve<IScopedService>();
}
```

---

## Key Benefits

| Feature | Benefit |
|---------|---------|
| **Performance** | 20-30% faster than MS.DI |
| **Memory** | 25% less allocation |
| **Decorators** | Native support, no manual wrappers |
| **AOP** | Built-in with Castle.DynamicProxy |
| **Multi-Tenancy** | Child containers with O(1) snapshots |
| **Open Generics** | Full support with variance |
| **Convention** | Powerful RegisterMany |

---

## Additional Resources

- **Full Documentation**: `DRYIOC_CLAUDE_EXPLOITABLE_PATTERNS.md`
- **Official Docs**: https://github.com/dadhi/DryIoc
- **Performance Benchmarks**: https://danielpalme.github.io/IocPerformance/
- **WPFBase Implementation**: `App.xaml.cs`