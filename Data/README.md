# Data

This folder contains data access layer components and repository patterns.

## Purpose
- Abstract data access logic
- Repository and Unit of Work patterns
- Database interaction interfaces

## Files
- `IRepository.cs` - Generic repository interface
- `IUnitOfWork.cs` - Unit of Work pattern interface
- `RepositoryBase.cs` - Base repository implementation

## Architecture
Following Repository and Unit of Work patterns for clean data access:

```
ViewModel → Service → Repository → Database
```

## Usage Example
```csharp
public class CustomerService
{
    private readonly IRepository<Customer> _customerRepo;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Customer> GetCustomerAsync(int id)
    {
        return await _customerRepo.GetByIdAsync(id);
    }

    public async Task SaveCustomerAsync(Customer customer)
    {
        _customerRepo.Add(customer);
        await _unitOfWork.CommitAsync();
    }
}
```

## Benefits
- Testable data access (easy to mock)
- Separation of concerns
- Consistent data patterns