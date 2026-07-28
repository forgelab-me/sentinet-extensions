# Sentinet Extensions

A collection of custom extensions for [Nevatech Sentinet](https://www.nevatech.com/sentinet) API Management Platform built with .NET 10.

## Overview

This repository provides open-source extensions to enhance Sentinet's monitoring, alerting, and data handling capabilities. Each extension is independently deployable and follows Sentinet's extension model.

## Projects

### CustomMonitoringFilter

A monitoring filter that provides configurable obfuscation of sensitive data in HTTP traffic monitoring.

**Key Features:**
- JWT token signature hashing with SHA256
- Authorization header obfuscation (Bearer, Basic, NTLM, Negotiate)
- Configurable field-level obfuscation for JSON payloads
- Regex-based pattern matching
- Recursive processing of nested structures

📖 [Full Documentation](src/CustomMonitoringFilter/README.md)

### CustomAlert *(Coming Soon)*

Custom alerting extensions for Sentinet monitoring events.

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- Visual Studio 2022/2026 or compatible IDE
- Nevatech Sentinet 7.0.1905 or later
- Valid Sentinet license

### Building the Solution

```bash
git clone https://github.com/forgelab-me/sentinet-extensions.git
cd sentinet-extensions
dotnet build
```

### Running Tests

```bash
dotnet test
```

## Deployment

Each extension includes specific deployment instructions in its project README. General steps:

1. Build the solution in Release mode
2. Locate the compiled DLL in `src/[ProjectName]/bin/Release/net10.0/`
3. Copy to your Sentinet Node installation directory under `extensions/`
4. Restart the Sentinet Node service
5. Configure the extension via Sentinet Console

## Project Structure

```
sentinet-extensions/
├── sentinet-extensions.slnx          # Visual Studio solution
├── .gitignore
├── LICENSE
├── README.md                         # This file
└── src/
	├── CustomMonitoringFilter/       # Monitoring filter extension
	│   ├── README.md                 # Project-specific docs
	│   ├── CustomMonitoringFilter.csproj
	│   └── ...
	└── CustomMonitoringFilter.Tests/ # xUnit test project
		└── ...
```

## Contributing

Contributions are welcome. Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/your-feature`)
3. Follow existing code style and patterns
4. Add tests for new functionality
5. Ensure all tests pass (`dotnet test`)
6. Submit a Pull Request

### Code Standards

- Target .NET 10.0
- Enable nullable reference types
- Follow C# coding conventions
- Add XML documentation comments for public APIs
- Maintain test coverage for new features

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

### Important Notice

These extensions are built for **Nevatech Sentinet**, which is a proprietary product. While this code is open source under MIT, **you must have a valid Sentinet license** to use these extensions.

For Sentinet licensing: https://www.nevatech.com/sentinet

## Acknowledgments

- Built for [Nevatech Sentinet](https://www.nevatech.com/sentinet) API Management Platform
- Uses [Newtonsoft.Json](https://www.newtonsoft.com/json) for JSON processing

## Support

- **Issues**: Open an issue in this repository
- **Sentinet Support**: Contact Nevatech for platform-specific issues
- **Documentation**: Check individual project README files

## Roadmap

- [x] CustomMonitoringFilter - Data obfuscation
- [ ] CustomAlert - Advanced alerting
- [ ] Additional monitoring filters
- [ ] Performance optimizations
- [ ] Extended MIME type support

---

**Note**: These are community extensions and are not officially supported by Nevatech.
