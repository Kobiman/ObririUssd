# Obriri USSD Service

A .NET 5.0 ASP.NET Core web API service for handling USSD (Unstructured Supplementary Service Data) requests for a lottery/betting system. The service manages user sessions, processes payments, and sends SMS confirmations.

## Overview

This application provides a USSD-based interface for users to participate in various lottery games. It supports different game types (Direct, Perm-2, Perm-3) with daily options that vary by day of the week. The system handles user state management, payment processing, and transaction logging.

## Architecture

The application follows a layered architecture with the following key components:

### Controllers
- **UssdController**: Handles incoming USSD requests via the `/index` endpoint

### Services
- **UssdService**: Core business logic for processing USSD requests and managing user flows
- **MessageService**: Handles SMS notifications via mNotify API

### Background Services
- **PaymentBackgroundService**: Processes payments asynchronously using a channel-based approach
- **UssdSessionBackgroundService**: Manages USSD session cleanup (implementation details in source)

### Data Layer
- **UssdDataContext**: Entity Framework Core DbContext for database operations
- **Models**: Data transfer objects and entities (UssdRequest, UssdResponse, UssdTransaction, etc.)

### Session Management
- **UssdSessionManager**: Static concurrent dictionary for managing user states across requests
- **UserState**: Record containing current state, selected values, and session data

## Prerequisites

- .NET 5.0 SDK
- SQL Server (Express or full version)
- Visual Studio 2019+ or VS Code with C# extensions

## Installation

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd ObririUssd
   ```

2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

3. Update the connection string in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=Ussd;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```

4. Run database migrations:
   ```bash
   dotnet ef database update
   ```

5. Build and run the application:
   ```bash
   dotnet build
   dotnet run
   ```

## Configuration

### appsettings.json
- **ConnectionStrings.DefaultConnection**: SQL Server connection string
- **Logging**: Log levels for different components

### Environment Variables
The application supports environment-specific configurations via `appsettings.Development.json` and `appsettings.Production.json`.

## API Documentation

### Endpoints

#### POST /index
Processes USSD requests.

**Request Body:**
```json
{
  "USERID": "WEB_MATE",
  "MSISDN": "233XXXXXXXXX",
  "USERDATA": "1",
  "MSGTYPE": true,
  "NETWORK": "MTN"
}
```

**Response:**
```json
{
  "USERID": "WEB_MATE",
  "MSISDN": "233XXXXXXXXX",
  "MSG": "Menu options...",
  "MSGTYPE": true
}
```

### Swagger Documentation
When running in development mode, access Swagger UI at `https://localhost:5001/swagger`

## Database Schema

### UssdTransaction
- `Id`: Primary key
- `TSN`: Transaction sequence number
- `PhoneNumber`: User's phone number
- `OptionName`: Selected game option
- `OptionValue`: Selected numbers/values
- `Amount`: Transaction amount
- `WinningAmount`: Winning amount (if applicable)
- `Win`: Win status
- `PaymentStatus`: Payment processing status
- `ApprovedBy`: Approver identifier
- `Proccessed`: Processing status
- `TransactionDate`: Transaction timestamp
- `Message`: Transaction message
- `Status`: General status
- `GameType`: Type of game

### UssdLock
- `Id`: Primary key
- `StartTime`: Draw start hour
- `EndTime`: Draw end hour
- `GameType`: Game type to lock
- `Disabled`: Lock status

### TransactionLog
- Logs for transaction auditing

## USSD Flow

The USSD service implements a state-based menu system:

1. **Initial Menu**: Displays daily options based on current day
2. **Game Selection**: User selects a game type (Direct-1 to Direct-5, Perm-2, Perm-3)
3. **Number Input**: User enters numbers (1-90 range)
4. **Amount Input**: User enters bet amount
5. **Payment Processing**: Asynchronous payment via background service
6. **Confirmation**: SMS sent with transaction details

### State Management
- States are tracked using a string-based system (e.g., "11", "2", "31")
- User sessions stored in memory with 30-second timeout
- Concurrent dictionary ensures thread safety

### Game Types
- **Direct-X**: Select exactly X numbers
- **Perm-2**: Select minimum 3 numbers for 2-number permutations
- **Perm-3**: Select minimum 4 numbers for 3-number permutations

### Daily Options
Different games available each day of the week:
- Monday: PIONEER, MONDAY SPECIAL
- Tuesday: VAG EAST, LUCKY TUESDAY
- Wednesday: VAG WEST, MID-WEEK
- Thursday: AFRICAN LOTTO, FORTUNE THURSDAY
- Friday: OBIRI SPECIAL, FRIDAY BONANZA
- Saturday: OLD SOLDIER, NATIONAL
- Sunday: SUNDAY SPECIAL

## Payment Processing

Payments are handled asynchronously through a channel-based system:

1. USSD request triggers payment initiation
2. Request queued in PaymentChannel
3. PaymentBackgroundService processes queue
4. External payment API called
5. On approval: transaction saved, SMS sent
6. On failure: appropriate error handling

## SMS Notifications

SMS messages sent via mNotify API for:
- Transaction confirmations
- Ticket details
- Win notifications (future feature)

## Background Services

### PaymentBackgroundService
- Consumes payment requests from channel
- Processes payments with external provider
- Saves successful transactions
- Sends confirmation SMS

### UssdSessionBackgroundService
- Cleans up expired user sessions
- Manages memory usage

## Error Handling

- Input validation for number ranges and formats
- Duplicate number detection
- Session timeout handling
- Payment failure scenarios
- Database transaction safety

## Security Considerations

- Input sanitization and validation
- SQL injection prevention via EF Core
- HTTPS enforcement in production
- API key management for external services

## Deployment

### Development
```bash
dotnet run --environment Development
```

### Production
1. Publish the application:
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. Configure production appsettings.json
3. Set up IIS or reverse proxy
4. Ensure database connectivity
5. Configure SSL certificates

### Docker (Optional)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:5.0
COPY ./publish /app
WORKDIR /app
ENTRYPOINT ["dotnet", "ObririUssd.dll"]
```

## Monitoring and Logging

- Structured logging via Microsoft.Extensions.Logging
- Transaction logging in database
- Error tracking and alerting (to be implemented)
- Performance monitoring (to be implemented)

## Testing

### Unit Tests
- Service layer testing
- Validation logic testing
- State management testing

### Integration Tests
- API endpoint testing
- Database integration testing
- Payment flow testing

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make changes with tests
4. Submit pull request

### Code Standards
- Follow C# coding conventions
- Use async/await for I/O operations
- Implement proper error handling
- Add XML documentation comments

## Future Enhancements

- Win checking and payout processing
- Admin panel for game management
- Advanced analytics and reporting
- Multi-language support
- Mobile app integration
- Real-time notifications

## Support

For support or questions, please contact the development team.

## Documentation

For detailed technical documentation, see the [docs](./docs/) directory:

- [API Reference](./docs/api-reference.md) - Detailed API specifications and USSD flow documentation
- [Database Schema](./docs/database-schema.md) - Database design, tables, and relationships
- [Architecture](./docs/architecture.md) - System design patterns and architectural decisions</content>
<filePath="filePath">/Users/rehotech/DotnetApps2026/ObririUssd/ObririUssd/README.md