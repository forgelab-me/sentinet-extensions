# CustomMonitoringFilter for Sentinet

A .NET monitoring filter extension for Nevatech Sentinet that provides configurable obfuscation of sensitive data in monitored HTTP traffic.

## Overview

This library implements `IMonitoringFilter` to intercept and sanitize sensitive information in API monitoring records before persistence. It handles authorization headers, JWT tokens, and configurable payload fields across JSON, XML, and plaintext formats.

## Features

- JWT token obfuscation preserving header and payload while hashing signatures with SHA256
- Multi-scheme authorization header support (Bearer, Basic, NTLM, Negotiate)
- Configurable field-level obfuscation via exact match or regex patterns
- Recursive processing of nested JSON structures and arrays
- MIME type detection and format-specific handling
- HashSet-based lookups (O(1)) and compiled regex for performance
- Graceful error handling with fallback to unmodified data

## Table of Contents

- [Installation](#installation)
- [Usage](#usage)
- [Configuration](#configuration)
- [Examples](#examples)
- [Architecture](#architecture)
- [Performance](#performance)
- [Contributing](#contributing)

## Installation

### Requirements

- .NET 10.0 or later
- Nevatech Sentinet 7.0.1905 or compatible

### Dependencies

```xml
<PackageReference Include="Nevatech.Sentinet" Version="7.0.1905" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
```

### Build

```bash
git clone https://github.com/forgelab-me/sentinet-extensions.git
cd sentinet-extensions
dotnet build --project src/CustomMonitoringFilter/CustomMonitoringFilter.csproj
```

## Usage

Deploy the compiled assembly to your Sentinet installation and configure the monitoring filter on the desired Virtual Service.

### Configuration via Design Mode

1. Navigate to the Virtual Service
2. Go to **Monitoring → Control → Filters**
3. Click **Add Custom Filter**
4. Enter the following details:
   - **Name**: `Obfuscation`
   - **Assembly**: `CustomMonitoringFilter`
   - **Type**: `CustomMonitoringFilter.Obfuscation`
5. Save the configuration

### Configuration via Source Mode

Add the following XML to the Virtual Service configuration:

```xml
<MONITORING-FILTERS>
    <CUSTOM-FILTER name="Obfuscation" type="CustomMonitoringFilter.Obfuscation, CustomMonitoringFilter" />
</MONITORING-FILTERS>
```

### Deployment

1. Locate your Sentinet Node installation directory
2. Create an `extensions` folder if it doesn't already exist
3. Copy `CustomMonitoringFilter.dll` into the `extensions` folder
4. Restart the Sentinet Node service to load the custom filter

## Configuration

### Field-Level Obfuscation

Configure which fields to obfuscate by modifying the `FieldsNameList` array in `Obfuscation.cs`:

```csharp
private static readonly string[] FieldsNameList =
[
	"Client_secret",
	"Password",
	"Image",
	"File",
	"FileBody"
];
```

### Pattern-Based Obfuscation

Define regex patterns for dynamic field matching:

```csharp
private static readonly Regex[] FieldsNameRegexList =
[
	new(@"(?s)(?<=BASE64"">).*?(?=<)", RegexOptions.Compiled)
];
```

### Field Removal

Specify fields to remove entirely from payloads:

```csharp
private static readonly string[] FieldNameToRemoveList = 
[
	"TemporaryToken",
	"SessionId"
];
```

## Examples

### JWT Token Obfuscation

**Before:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNryP4J3jVmNHl0w5N_XgL0n3I9PlFUP0THsR8U
```

**After:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.<SHA256>8f923d838b71f8a2778d598d1e5d30e4dfd7b05f27dc8f539459ffd43184fe44</SHA256>
```

### JSON Payload Obfuscation

**Before:**
```json
{
  "username": "john.doe",
  "password": "MySecretP@ss123",
  "client_secret": "abc123xyz"
}
```

**After:**
```json
{
  "username": "john.doe",
  "password": "***OBFUSCATED***",
  "client_secret": "***OBFUSCATED***"
}
```

### Basic Auth Obfuscation

**Before:**
```
Authorization: Basic dXNlcjpwYXNzd29yZA==
```

**After:**
```
Authorization: Basic *******
```

## Architecture

### Component Structure

```
CustomMonitoringFilter
├─ Obfuscation.cs              # Main filter implementation (IMonitoringFilter)
└─ Obfuscator/
   ├─ HeaderObfuscator.cs      # Authorization header processing
   ├─ PayloadObfuscator.cs     # Payload routing and orchestration
   ├─ JsonObfuscator.cs        # JSON-specific obfuscation logic
   ├─ PlaintextObfuscator.cs   # Regex-based plaintext obfuscation
   └─ MimeTypes.cs             # MIME type detection utilities
```

### Processing Flow

```
MonitoringRecord
	↓
Obfuscation.WriteRecordAsync()
	├─→ HeaderObfuscator.Obfuscate()  → Authorization headers
	└─→ PayloadObfuscator.Obfuscate()
			├─→ PlaintextObfuscator   → Regex patterns
			├─→ JsonObfuscator        → JSON fields
			└─→ (XML support future)
```

## Performance

- HashSet lookups provide O(1) field matching versus O(n) array scanning
- Compiled regex patterns for faster matching
- Single-pass processing minimizes iterations over large payloads
- Lazy evaluation performs MIME type detection only when needed
- Exception handling preserves original data on failure

## Security

- JWT signatures are hashed with SHA256 to prevent replay while maintaining debuggability
- Case-insensitive matching prevents bypass through casing variations
- Recursive obfuscation handles deeply nested JSON structures
- Graceful failure handling ensures original data is never exposed due to processing errors

## Testing

Run the test suite:

```bash
dotnet test
```

Test coverage includes:
- JWT token parsing and obfuscation
- JSON nested object obfuscation
- Authorization header schemes (Bearer, Basic, NTLM, Negotiate)
- MIME type detection
- Regex pattern matching
- Edge cases and error handling

## Contributing

Contributions are welcome. Please see the [main repository contributing guidelines](../../README.md#contributing).

### Code Standards

- Follow existing code style
- Add XML documentation comments
- Include unit tests for new features
- Ensure all tests pass before submitting

## License

This project is part of the Sentinet Extensions repository and is licensed under the MIT License.

See the [LICENSE](../../LICENSE) file in the repository root for full details.

## Support

For issues and questions:
- Open an [Issue](https://github.com/forgelab-me/sentinet-extensions/issues) in the main repository
- Check the [main documentation](../../README.md)
