# DryIoc Prompt Templates for Claude Code

This document contains ready-to-use prompt templates for common DryIoc operations. Copy and paste these prompts to have Claude Code generate DI code following WPFBase patterns.

---

## Service Registration Prompts

### 1. Add Basic Service

```
Add a new service IOrderService with OrderService implementation.
Methods: GetOrderAsync(string orderId), CreateOrderAsync(Order order).
Register as singleton in DryIoc container in App.xaml.cs.
```

### 2. Add Service with Multiple Implementations

```
Add INotificationService with three implementations:
- EmailNotificationService (key: "email")
- SmsNotificationService (key: "sms")
- PushNotificationService (key: "push")
Register all as singletons with service keys.
```

### 3. Add Scoped Service

```
Add IOrderProcessor with OrderProcessor implementation.
Register as scoped (per request/operation).
Include proper scope management in consuming ViewModels.
```

### 4. Add Service with Factory Method

```
Add IConnectionFactory with SqlConnectionFactory implementation.
Use factory registration with connection string from configuration.
Register as singleton.
```

---

## Decorator Prompts

### 5. Add Logging Decorator

```
Add logging decorator to IPaymentService.
Log method entry, exit, duration, and exceptions.
Inject ILoggingService for structured logging.
Register decorator after base PaymentService.
```

### 6. Add Caching Decorator

```
Create caching decorator for IProductService.
Cache GetProductAsync results for 10 minutes.
Cache GetProductsByCategoryAsync results for 5 minutes.
Invalidate cache on UpdateProductAsync and DeleteProductAsync.
Use IMemoryCache for storage.
```

### 7. Add Validation Decorator

```
Create validation decorator for IUserService.
Validate SaveUserAsync input using FluentValidation.
Throw ValidationException with detailed errors.
Log validation failures.
Apply decorator before SaveUserAsync is called.
```

### 8. Add Telemetry Decorator

```
Add telemetry decorator to IOrderService.
Track operation duration, success/failure, and exceptions.
Use ITelemetryService.StartActivity for distributed tracing.
Record custom metrics for order value and item count.
```

### 9. Add Retry/Resilience Decorator

```
Create resilience decorator for IExternalApiService.
Use Polly with 3 retries and exponential backoff.
Add circuit breaker (opens after 5 failures, 1-minute timeout).
Log retry attempts and circuit breaker state changes.
Integrate with IResilienceService.
```

### 10. Add Authorization Decorator

```
Add authorization decorator to IAdminService.
Check user permissions before method execution.
Inject IAuthorizationService and IUserContext.
Throw UnauthorizedAccessException with descriptive message.
Log unauthorized access attempts.
```

### 11. Add Transaction Decorator

```
Create transactional decorator for IOrderProcessingService.
Wrap ProcessOrderAsync in database transaction.
Use IUnitOfWork for transaction management.
Commit on success, rollback on exception.
Log transaction begin/commit/rollback.
```

### 12. Add Performance Monitoring Decorator

```
Add performance monitoring decorator to IReportService.
Track execution time and memory allocation.
Log warnings for operations over 1 second.
Log warnings for memory allocation over 10MB.
Use IPerformanceOptimizationService.
Record metrics to ITelemetryService.
```

### 13. Add Multiple Decorators

```
Add three decorators to IPaymentService in this order:
1. ValidationDecorator (innermost) - validate input
2. LoggingDecorator (middle) - log operations
3. TelemetryDecorator (outermost) - track metrics
Register all decorators properly ordered in DryIoc.
```

---

## Convention-Based Registration Prompts

### 14. Register Services by Namespace

```
Add convention-based registration for all services in Services.Payment namespace.
Match by interface name (I prefix).
Register as singleton.
Use RegisterMany with assembly scanning.
```

### 15. Register All ViewModels

```
Add convention-based registration for all ViewModels.
Match types ending with "ViewModel".
Register as transient (new instance per navigation).
Exclude MainViewModel (already registered as singleton).
```

### 16. Register All Repositories

```
Register all repository implementations in Data.Repositories namespace.
Match IRepository<T> interface pattern.
Register as scoped (per unit of work).
Use RegisterMany with type filtering.
```

### 17. Register All Validators

```
Register all FluentValidation validators in Validators namespace.
Match IValidator<T> interface.
Register as singleton (validators are stateless).
```

---

## Open Generic Prompts

### 18. Add Generic Repository

```
Register open generic IRepository<TEntity> with Repository<TEntity> implementation.
Support GetByIdAsync, AddAsync, UpdateAsync, DeleteAsync, GetAllAsync.
Register as scoped lifetime.
Ensure automatic resolution for IRepository<Customer>, IRepository<Order>, etc.
```

### 19. Add Generic Command Handler

```
Register open generic ICommandHandler<TCommand, TResult>.
Create CommandHandler<TCommand, TResult> base implementation.
Register as transient.
Add example handlers: CreateUserCommandHandler, UpdateUserCommandHandler.
```

### 20. Add Generic Validator

```
Register open generic IValidator<T> pattern.
Create base Validator<T> implementation using FluentValidation.
Register as singleton.
Add example validators for User and Order entities.
```

---

## AOP Interceptor Prompts

### 21. Add Logging Interceptor with Castle DynamicProxy

```
Create Castle DynamicProxy logging interceptor for IUserService.
Log method name, parameters, duration, and exceptions.
Support both sync and async methods.
Use ILoggingService for structured logging.
Register interceptor using extension method pattern.
```

### 22. Add Performance Monitoring Interceptor

```
Create Castle DynamicProxy performance interceptor.
Track method execution time and memory allocation.
Log warnings for slow operations (>1 second).
Log warnings for high memory allocation (>10MB).
Record metrics to ITelemetryService.
Apply to all services in Services.Core namespace.
```

### 23. Add Exception Handling Interceptor

```
Create Castle DynamicProxy exception handling interceptor.
Catch and log all exceptions with context.
Convert specific exceptions to user-friendly messages.
Record exceptions to ITelemetryService.
Re-throw original exception after logging.
```

---

## Multi-Tenancy Prompts

### 24. Add Tenant Container Factory

```
Create TenantContainerFactory for multi-tenant IDataService.
Each tenant gets isolated child container with shared core services.
Store tenant containers in ConcurrentDictionary.
Include GetOrCreateTenantContainer and RemoveTenantContainer methods.
Register ITenantContext per tenant with tenant ID.
```

### 25. Add Tenant-Scoped Service

```
Add ITenantDataService that uses tenant-specific connection strings.
Create TenantDataService that reads connection string from ITenantContext.
Register service with tenant scope using child containers.
Include example of switching tenants in ViewModel.
```

---

## Plugin Architecture Prompts

### 26. Add Plugin Manager with Child Containers

```
Create PluginManager using DryIoc child containers.
LoadPlugin method creates isolated child container per plugin.
Share core services from root container.
Register plugin services from plugin assembly.
Include UnloadPlugin to dispose plugin container.
Provide GetPlugins to enumerate all loaded plugins.
```

### 27. Add Plugin Interface and Sample Plugin

```
Create IPlugin interface with Initialize and Execute methods.
Create SamplePlugin implementation.
Register plugin using child container with plugin name as service key.
Include plugin metadata (Name, Version, Author).
```

---

## Testing Prompts

### 28. Create Test Container Setup

```
Create test container setup for unit testing IUserService.
Mock dependencies: IUserRepository, ILoggingService.
Register mocks in test container.
Register service under test as transient.
Include [TestInitialize] and [TestCleanup] methods.
```

### 29. Create Auto-Mocking Container

```
Create AutoMockingContainer class for testing.
Automatically create Moq mocks for unregistered interfaces.
Provide GetMock<T> method to access and configure mocks.
Register concrete services normally.
Include Dispose for cleanup.
Add example test using auto-mocking.
```

### 30. Create Integration Test Container

```
Create integration test container that uses real implementations.
Replace only external dependencies (database, HTTP) with mocks.
Use child container to override specific services.
Include setup for in-memory database or test database.
Add cleanup to reset state between tests.
```

---

## Migration Prompts

### 31. Migrate Single Service from MS.DI to DryIoc

```
Migrate this Microsoft.Extensions.DependencyInjection registration to DryIoc:
services.AddSingleton<IEmailService, EmailService>();

Provide equivalent DryIoc registration with explanation.
```

### 32. Migrate Multiple Services

```
Migrate these MS.DI registrations to DryIoc:
services.AddSingleton<IEmailService, EmailService>();
services.AddTransient<IReportGenerator, ReportGenerator>();
services.AddScoped<IOrderProcessor, OrderProcessor>();
services.AddSingleton<ILoggingService>(sp => new LoggingService(...));

Include factory registration pattern for LoggingService.
```

### 33. Migrate Entire ConfigureServices Method

```
Migrate this entire ConfigureServices method from MS.DI to DryIoc:
[paste ConfigureServices code]

Convert all service registrations.
Handle factory registrations.
Convert scoped services with proper lifetime.
Update resolution calls.
```

---

## Advanced Pattern Prompts

### 34. Add Conditional Registration

```
Register IDataService with two implementations:
- MockDataService in Development environment
- ProductionDataService in Production environment
Read environment from IConfigurationService.
Use conditional registration in DryIoc.
```

### 35. Add Named Service Resolution

```
Create IReportExporter with three implementations:
- PdfReportExporter (key: "pdf")
- ExcelReportExporter (key: "excel")
- CsvReportExporter (key: "csv")
Create ReportExporterFactory that resolves by format string.
Register all exporters with service keys.
```

### 36. Add Lazy Service Resolution

```
Register IExpensiveService with ExpensiveService implementation.
Inject as Lazy<IExpensiveService> in MyViewModel.
Service should only be created when Value property is accessed.
Show example of deferred initialization in ViewModel.
```

### 37. Add Func Factory Pattern

```
Create IDocumentProcessor factory using Func<string, IDocumentProcessor>.
DocumentProcessorFactory resolves processor by document type.
Register processors with keys: "pdf", "word", "excel".
Show example of factory usage in ViewModel.
```

### 38. Add Property Injection

```
Configure DryIoc to support property injection for Controllers.
Use rules.With(propertiesAndFields: ...) pattern.
Only inject properties marked with [Inject] attribute.
Provide example Controller with property injection.
```

---

## Real-World Scenario Prompts

### 39. Complete CRUD Service with All Decorators

```
Create complete ICustomerService with CRUD operations.
Add CustomerService implementation using IRepository<Customer>.
Apply decorators in this order:
1. Validation (validate input)
2. Authorization (check permissions)
3. Transaction (wrap in transaction)
4. Logging (log operations)
5. Telemetry (track metrics)
6. Caching (cache reads)
Register all with proper ordering in DryIoc.
```

### 40. External API Service with Resilience

```
Create IWeatherApiService for external weather API.
Add WeatherApiService implementation using HttpClient.
Apply decorators:
1. Retry with exponential backoff (3 attempts)
2. Circuit breaker (5 failures, 1-minute timeout)
3. Logging (log requests/responses)
4. Telemetry (track API performance)
5. Caching (cache for 10 minutes)
Register all in DryIoc.
```

### 41. Multi-Tenant Database Service

```
Create ITenantDatabaseService for multi-tenant application.
Each tenant has separate connection string.
Use child containers for tenant isolation.
Include TenantContainerFactory for tenant management.
Add middleware to switch tenant context per request.
Register all components in DryIoc.
```

### 42. Plugin-Based Report Generator

```
Create plugin-based report generation system.
IReportGenerator interface with multiple implementations.
Use child containers to load plugins from assemblies.
Include PluginManager to discover and load plugins.
Provide ReportService that aggregates all generators.
Register using DryIoc plugin pattern.
```

---

## Optimization Prompts

### 43. Pre-Compile Services for Fast Startup

```
Configure DryIoc for fastest startup time.
Pre-compile all singleton services at application start.
Use compilation instead of interpretation.
Add performance logging to measure improvement.
Show before/after startup time comparison.
```

### 44. Optimize Memory Usage

```
Configure DryIoc for minimal memory allocation.
Use appropriate lifetimes (avoid over-use of singleton).
Enable disposal tracking for transient disposables.
Add memory profiling to measure improvement.
```

### 45. Add Resolution Diagnostics

```
Add DryIoc resolution diagnostics logging.
Log all service resolutions with timing.
Track slow resolutions (>100ms).
Identify circular dependencies.
Create diagnostic report at application start.
```

---

## Documentation Prompts

### 46. Document DryIoc Container Setup

```
Create XML documentation for ConfigureServices method in App.xaml.cs.
Explain each rule configuration.
Document service registration patterns.
Include examples of resolving services.
Add troubleshooting section for common issues.
```

### 47. Create DryIoc Migration Guide

```
Create step-by-step migration guide from MS.DI to DryIoc.
Include checklist of tasks.
Provide before/after code examples.
Document breaking changes.
Add testing recommendations.
Include rollback plan.
```

---

## Usage Examples

### Example 1: Basic Service

**Prompt:**
```
Add IOrderService with OrderService implementation.
Methods: GetOrderAsync(string orderId), CreateOrderAsync(Order order).
Register as singleton in DryIoc container in App.xaml.cs.
```

**Claude generates:**
```csharp
// IOrderService.cs
public interface IOrderService
{
    Task<Order> GetOrderAsync(string orderId);
    Task CreateOrderAsync(Order order);
}

// OrderService.cs
public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly ILoggingService _logger;

    public OrderService(IOrderRepository repository, ILoggingService logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Order> GetOrderAsync(string orderId)
    {
        _logger.LogDebug("Getting order {OrderId}", orderId);
        return await _repository.GetByIdAsync(orderId);
    }

    public async Task CreateOrderAsync(Order order)
    {
        _logger.LogInformation("Creating order");
        await _repository.AddAsync(order);
    }
}

// App.xaml.cs - Add to ConfigureServices
container.Register<IOrderService, OrderService>(Reuse.Singleton);
```

### Example 2: With Decorators

**Prompt:**
```
Add logging and telemetry decorators to IPaymentService.
```

**Claude generates:**
```csharp
// LoggingPaymentServiceDecorator.cs
public class LoggingPaymentServiceDecorator : IPaymentService
{
    private readonly IPaymentService _inner;
    private readonly ILoggingService _logger;

    public LoggingPaymentServiceDecorator(IPaymentService inner, ILoggingService logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(Payment payment)
    {
        _logger.LogInformation("Processing payment: {Amount}", payment.Amount);
        var sw = Stopwatch.StartNew();

        try
        {
            var result = await _inner.ProcessPaymentAsync(payment);
            sw.Stop();
            _logger.LogInformation("Payment processed in {Ms}ms", sw.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Payment failed after {Ms}ms", sw.ElapsedMilliseconds);
            throw;
        }
    }
}

// TelemetryPaymentServiceDecorator.cs
public class TelemetryPaymentServiceDecorator : IPaymentService
{
    private readonly IPaymentService _inner;
    private readonly ITelemetryService _telemetry;

    public TelemetryPaymentServiceDecorator(IPaymentService inner, ITelemetryService telemetry)
    {
        _inner = inner;
        _telemetry = telemetry;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(Payment payment)
    {
        using var activity = _telemetry.StartActivity("Payment.Process");
        activity?.SetTag("payment.amount", payment.Amount);
        activity?.SetTag("payment.currency", payment.Currency);

        var sw = Stopwatch.StartNew();

        try
        {
            var result = await _inner.ProcessPaymentAsync(payment);
            sw.Stop();

            _telemetry.RecordMetric("payment.duration", sw.ElapsedMilliseconds);
            _telemetry.IncrementCounter("payment.success");

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _telemetry.RecordException(ex, activity);
            _telemetry.IncrementCounter("payment.failure");
            throw;
        }
    }
}

// App.xaml.cs - Registration
container.Register<IPaymentService, PaymentService>(Reuse.Singleton);
container.Register<IPaymentService, LoggingPaymentServiceDecorator>(
    Reuse.Singleton,
    setup: Setup.Decorator
);
container.Register<IPaymentService, TelemetryPaymentServiceDecorator>(
    Reuse.Singleton,
    setup: Setup.Decorator
);
```

---

## Tips for Effective Prompts

### 1. Be Specific About Lifetimes
✅ "Register as singleton"
✅ "Register as transient"
✅ "Register as scoped"
❌ "Register the service"

### 2. Specify Decorator Order
✅ "Apply decorators in this order: validation, logging, caching"
❌ "Add decorators"

### 3. Include Dependencies
✅ "Inject ILoggingService and ITelemetryService"
❌ "Add logging"

### 4. Specify Integration Points
✅ "Add to ConfigureServices method in App.xaml.cs"
❌ "Register in container"

### 5. Request Complete Examples
✅ "Include example usage in ViewModel"
✅ "Show registration code"
✅ "Provide test example"
❌ "Create the service"

### 6. Reference Existing Patterns
✅ "Follow the pattern used in LoggingService"
✅ "Similar to TelemetryService decorator"
❌ "Create a service"

---

## Combining Multiple Prompts

You can chain prompts together for complex scenarios:

```
1. Add IInvoiceService with InvoiceService implementation
2. Add validation decorator (check invoice totals match line items)
3. Add authorization decorator (require AccountsPayable role)
4. Add transaction decorator (wrap in database transaction)
5. Add logging decorator (log all operations)
6. Add telemetry decorator (track invoice processing metrics)
7. Register all in proper order in DryIoc
8. Create unit tests with mocked dependencies
```

Claude will generate a complete, integrated solution following all patterns.

---

## Template Customization

Copy any prompt and customize with your specific requirements:

**Template:**
```
Add I{ServiceName} with {ServiceName} implementation.
Methods: {method1}, {method2}, {method3}.
Inject {dependency1}, {dependency2}.
Register as {lifetime} in DryIoc container.
Apply {decorator1} and {decorator2} decorators.
```

**Example:**
```
Add ICustomerService with CustomerService implementation.
Methods: GetCustomerAsync, CreateCustomerAsync, UpdateCustomerAsync.
Inject ICustomerRepository, ILoggingService, IValidator<Customer>.
Register as singleton in DryIoc container.
Apply logging and validation decorators.
```

---

## See Also

- **Full Patterns**: `DRYIOC_CLAUDE_EXPLOITABLE_PATTERNS.md`
- **Quick Reference**: `DRYIOC_QUICK_REFERENCE.md`
- **Working Example**: `App.xaml.cs`