# Architecture & Design Patterns

## Overview

The Obriri USSD service follows a modular, layered architecture with clear separation of concerns. It leverages .NET 5.0 features and implements several design patterns for maintainability and scalability.

## Architectural Layers

### 1. Presentation Layer
- **UssdController**: ASP.NET Core MVC controller handling HTTP requests
- **API Models**: UssdRequest, UssdResponse for data transfer

### 2. Business Logic Layer
- **IUssdService/UssdService**: Core business logic and USSD flow management
- **MessageService**: SMS notification handling
- **Validation Logic**: Input validation and business rules

### 3. Data Access Layer
- **UssdDataContext**: EF Core DbContext for database operations
- **Repository Pattern**: Interface-based data access (IUssdDataContext)

### 4. Infrastructure Layer
- **Background Services**: Asynchronous processing
- **Session Management**: In-memory state storage
- **Payment Channel**: Producer-consumer pattern for payment processing

## Design Patterns Used

### Singleton Pattern
- **UssdSessionManager**: Single instance managing all user sessions
- **PaymentChannel**: Single channel instance for payment queuing

### Factory Pattern
- **UssdResponse.CreateResponse()**: Static factory method for response creation

### Repository Pattern
- **IUssdDataContext**: Interface abstraction over data access
- **UssdDataContext**: Concrete implementation

### Producer-Consumer Pattern
- **PaymentChannel**: Channel-based asynchronous processing
- **PaymentBackgroundService**: Consumer processing payment requests

### State Pattern
- **UserState**: Encapsulates user session state
- String-based state machine for USSD menu navigation

### Background Service Pattern
- **PaymentBackgroundService**: IHostedService for long-running tasks
- **UssdSessionBackgroundService**: Session cleanup and maintenance

## Key Components

### UssdService
Core service implementing the USSD menu flow:

```csharp
public class UssdService : IUssdService
{
    // Dependencies injected via constructor
    private readonly UssdDataContext _context;
    private readonly IPaymentChannel _channel;

    public async Task<UssdResponse> ProcessRequest(UssdRequest request, CancellationToken token)
    {
        // State-based menu processing logic
    }
}
```

**Responsibilities:**
- Parse incoming USSD requests
- Manage user state transitions
- Validate user input
- Generate appropriate menu responses
- Queue payment requests

### PaymentChannel
Implements the producer-consumer pattern using System.Threading.Channels:

```csharp
public class PaymentChannel : IPaymentChannel
{
    private readonly Channel<PaymentChannelMessage> Requests = Channel.CreateUnbounded<PaymentChannelMessage>();

    public async Task WriteAsync(PaymentChannelMessage request)
    {
        await Requests.Writer.WriteAsync(request);
    }

    public IAsyncEnumerable<PaymentChannelMessage> ReadAllAsync()
    {
        return Requests.Reader.ReadAllAsync();
    }
}
```

**Benefits:**
- Decouples request processing from payment processing
- Enables asynchronous payment handling
- Provides backpressure handling
- Thread-safe by design

### Session Management
Uses ConcurrentDictionary for thread-safe session storage:

```csharp
public static ConcurrentDictionary<string, UserState> PreviousState = new();
```

**Features:**
- Thread-safe operations
- Automatic expiration (30 seconds)
- Memory-efficient for high concurrency

## Dependency Injection

The application uses ASP.NET Core's built-in DI container:

```csharp
// Startup.cs
services.AddTransient<IUssdService, UssdService>();
services.AddSingleton<IPaymentChannel, PaymentChannel>();
services.AddHostedService<PaymentBackgroundService>();
```

**Lifetime Management:**
- **Transient**: UssdService (new instance per request)
- **Singleton**: PaymentChannel (shared across app)
- **Scoped**: DbContext (per request scope)

## Asynchronous Programming

Extensive use of async/await for I/O operations:

- HTTP requests to payment gateway
- Database operations via EF Core
- SMS API calls
- Channel-based communication

## Error Handling

- Try-catch blocks in critical paths
- CancellationToken support for cooperative cancellation
- Graceful degradation on payment failures
- Logging of exceptions for monitoring

## Configuration Management

- **appsettings.json**: Environment-specific configuration
- **Connection strings**: Database connectivity
- **External API keys**: SMS service configuration

## Testing Strategy

### Unit Tests
- Service layer testing with mocked dependencies
- Validation logic testing
- State transition testing

### Integration Tests
- API endpoint testing
- Database integration testing
- End-to-end USSD flow testing

## Performance Considerations

### Memory Management
- Session cleanup to prevent memory leaks
- Efficient use of ConcurrentDictionary
- Background service for maintenance tasks

### Database Optimization
- EF Core query optimization
- Connection pooling
- Indexing strategy

### Concurrency
- Thread-safe session management
- Asynchronous payment processing
- Channel-based queuing

## Scalability

### Horizontal Scaling
- Stateless design (sessions in memory)
- Database as single source of truth
- External service dependencies

### Vertical Scaling
- Background services for CPU-intensive tasks
- Efficient memory usage
- Connection pooling

## Security

### Input Validation
- Comprehensive input sanitization
- Range and format checking
- SQL injection prevention via EF Core

### API Security
- HTTPS enforcement
- API key management
- Request rate limiting (to be implemented)

## Monitoring & Observability

### Logging
- Structured logging with Microsoft.Extensions.Logging
- Transaction auditing
- Error tracking

### Metrics
- Request/response times
- Payment success rates
- Session counts

## Future Improvements

### Microservices Architecture
- Extract payment processing to separate service
- SMS service as independent microservice
- Session management as Redis-backed service

### Event-Driven Architecture
- Event sourcing for transaction history
- Message queues for inter-service communication
- Event-driven payment processing

### CQRS Pattern
- Separate read/write models
- Optimized queries for reporting
- Eventual consistency where appropriate</content>
<filePath">/Users/rehotech/DotnetApps2026/ObririUssd/ObririUssd/docs/architecture.md