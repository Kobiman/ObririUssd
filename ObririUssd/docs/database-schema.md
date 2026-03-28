# Database Schema

## Overview

The application uses Entity Framework Core with SQL Server. The database contains three main tables for transaction management, game locking, and logging.

## Tables

### UssdTransaction

Stores all betting transactions and their outcomes.

```sql
CREATE TABLE [dbo].[Trans] (
    [Id] INT IDENTITY (1, 1) NOT NULL,
    [TSN] INT NULL,
    [PhoneNumber] NVARCHAR(MAX) NULL,
    [OptionName] NVARCHAR(MAX) NULL,
    [OptionValue] NVARCHAR(MAX) NULL,
    [Amount] REAL NULL,
    [WinningAmount] REAL NULL,
    [Win] BIT NULL,
    [PaymentStatus] BIT NULL,
    [ApprovedBy] NVARCHAR(MAX) NULL,
    [Proccessed] BIT NULL,
    [TransactionDate] DATETIME2 NULL,
    [Message] NVARCHAR(MAX) NULL,
    [Status] BIT NULL,
    [GameType] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_Trans] PRIMARY KEY CLUSTERED ([Id] ASC)
);
```

**Fields:**
- `Id`: Auto-incrementing primary key
- `TSN`: Transaction sequence number (for external systems)
- `PhoneNumber`: Customer's mobile number
- `OptionName`: Game option selected (e.g., "PIONEER", "MONDAY SPECIAL")
- `OptionValue`: Numbers selected (space-separated string)
- `Amount`: Bet amount in GHS
- `WinningAmount`: Payout amount (0 if no win)
- `Win`: Boolean indicating if ticket won
- `PaymentStatus`: Payment processing status
- `ApprovedBy`: User/system that approved payment
- `Proccessed`: Whether transaction has been fully processed
- `TransactionDate`: Timestamp of transaction
- `Message`: Human-readable transaction description
- `Status`: General transaction status
- `GameType`: Type of game (Direct, Perm-2, Perm-3)

### UssdLock

Controls when specific games are available for betting.

```sql
CREATE TABLE [dbo].[UssdLock] (
    [Id] INT IDENTITY (1, 1) NOT NULL,
    [StartTime] INT NULL,
    [EndTime] INT NULL,
    [GameType] NVARCHAR(MAX) NULL,
    [Disabled] BIT NULL,
    CONSTRAINT [PK_UssdLock] PRIMARY KEY CLUSTERED ([Id] ASC)
);
```

**Fields:**
- `Id`: Auto-incrementing primary key
- `StartTime`: Hour when betting opens (0-23)
- `EndTime`: Hour when betting closes (0-23)
- `GameType`: Game type to lock/unlock
- `Disabled`: Whether this lock rule is active

**Logic:**
- Draw is closed if `Disabled = true`
- Draw has ended if current hour < `StartTime` OR current hour >= `EndTime`

### TransactionLog

Audit log for transaction-related events.

```sql
CREATE TABLE [dbo].[TransactionLogs] (
    [Id] INT IDENTITY (1, 1) NOT NULL,
    [Timestamp] DATETIME2 NULL,
    [EventType] NVARCHAR(MAX) NULL,
    [Details] NVARCHAR(MAX) NULL,
    [TransactionId] INT NULL,
    CONSTRAINT [PK_TransactionLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
);
```

**Fields:**
- `Id`: Auto-incrementing primary key
- `Timestamp`: When the log entry was created
- `EventType`: Type of event (Payment, Validation, Error, etc.)
- `Details`: Detailed event information
- `TransactionId`: Reference to UssdTransaction.Id

## Relationships

- TransactionLog.TransactionId → UssdTransaction.Id (optional foreign key)

## Indexes

Recommended indexes for performance:

```sql
-- Transaction queries by phone
CREATE INDEX IX_Trans_PhoneNumber ON Trans (PhoneNumber);

-- Transaction queries by date
CREATE INDEX IX_Trans_TransactionDate ON Trans (TransactionDate);

-- Lock queries by game type
CREATE INDEX IX_UssdLock_GameType ON UssdLock (GameType);

-- Log queries by transaction
CREATE INDEX IX_TransactionLogs_TransactionId ON TransactionLogs (TransactionId);
```

## Migrations

The application uses EF Core migrations. To create a new migration:

```bash
dotnet ef migrations add MigrationName
```

To apply migrations:

```bash
dotnet ef database update
```

## Seed Data

Example seed data for UssdLock:

```sql
INSERT INTO UssdLock (StartTime, EndTime, GameType, Disabled) VALUES
(9, 18, 'PIONEER', 0),
(9, 18, 'MONDAY SPECIAL', 0),
-- ... other game types
```

## Backup Strategy

- Daily full backups during off-peak hours
- Transaction log backups every 15 minutes
- Retention: 30 days for full backups, 7 days for logs
- Encrypted backups stored off-site

## Performance Considerations

- Partition UssdTransaction by month for large datasets
- Archive old transactions (> 1 year) to separate database
- Monitor query performance and add indexes as needed
- Consider read replicas for reporting queries</content>
<filePath">/Users/rehotech/DotnetApps2026/ObririUssd/ObririUssd/docs/database-schema.md