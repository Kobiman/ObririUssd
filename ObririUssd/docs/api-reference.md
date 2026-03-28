# API Reference

## USSD Controller

### POST /index

Processes incoming USSD requests and returns appropriate menu responses.

#### Request Format
```json
{
  "USERID": "string",     // Service provider ID
  "MSISDN": "string",     // User's phone number (e.g., "233XXXXXXXXX")
  "USERDATA": "string",   // User's input (numbers, menu selections)
  "MSGTYPE": boolean,     // true for continuation, false for end
  "NETWORK": "string"     // Mobile network (MTN, Vodafone, etc.)
}
```

#### Response Format
```json
{
  "USERID": "string",     // Echoes request USERID
  "MSISDN": "string",     // Echoes request MSISDN
  "MSG": "string",        // Menu text or message to display
  "MSGTYPE": boolean      // true for more input expected, false for session end
}
```

#### Error Responses
- Invalid input format: "Input value is not in the right format"
- Invalid range: "Enter value between X - Y"
- Draw closed: "Sorry Draw is closed for [GameType]"
- Draw ended: "Sorry Draw Has Ended for [GameType]"

## USSD Flow States

The application uses a state machine approach with string-based states:

- **"" (Empty)**: Initial state, show main menu
- **"1"**: Game type selected, show sub-options
- **"11"**: Direct-1 selected
- **"12"**: Direct-2 selected
- **"2"**: Perm-2 selected
- **"21"**: Perm-2 with numbers
- **"3"**: Amount input state
- **"31"**: Payment processing

## Validation Rules

### Number Input
- Range: 1-90 inclusive
- Format: Space-separated integers
- No duplicates allowed
- Minimum count varies by game type:
  - Direct-1: exactly 1 number
  - Direct-2: exactly 2 numbers
  - Perm-2: minimum 3 numbers
  - Perm-3: minimum 4 numbers

### Amount Input
- Must be valid integer
- Minimum amount validation (configurable)
- Multiplied by number of lines for Perm games

## Game Types

### Direct Games
- **Direct-1**: Single number selection
- **Direct-2**: Two number selection
- **Direct-3**: Three number selection
- **Direct-4**: Four number selection
- **Direct-5**: Five number selection

### Permutation Games
- **Perm-2**: Select 3+ numbers, all 2-number combinations played
- **Perm-3**: Select 4+ numbers, all 3-number combinations played

## Daily Game Availability

| Day | Option 1 | Option 2 |
|-----|----------|----------|
| Monday | PIONEER | MONDAY SPECIAL |
| Tuesday | VAG EAST | LUCKY TUESDAY |
| Wednesday | VAG WEST | MID-WEEK |
| Thursday | AFRICAN LOTTO | FORTUNE THURSDAY |
| Friday | OBIRI SPECIAL | FRIDAY BONANZA |
| Saturday | OLD SOLDIER | NATIONAL |
| Sunday | SUNDAY SPECIAL | N/A |

## Session Management

- Sessions stored in memory with ConcurrentDictionary
- Key: MSISDN (phone number)
- Timeout: 30 seconds from last activity
- Automatic cleanup by background service</content>
<filePath">/Users/rehotech/DotnetApps2026/ObririUssd/ObririUssd/docs/api-reference.md