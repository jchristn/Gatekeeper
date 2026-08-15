<img src="https://github.com/jchristn/Gatekeeper/raw/master/assets/icon.png" width="100" height="100">

# GateKeeper

[![NuGet Version](https://img.shields.io/nuget/v/GateKeeper.svg?style=flat)](https://www.nuget.org/packages/GateKeeper/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/GateKeeper.svg)](https://www.nuget.org/packages/GateKeeper)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)

A lightweight, easy-to-use roles-based access control (RBAC) library for .NET applications.

## Overview

GateKeeper provides a simple yet powerful way to implement authorization in your .NET applications. Define users, roles, resources, and permissions, then authorize access attempts with a single method call.

## Features

- Simple, intuitive API for managing users, roles, resources, and permissions
- SQLite-based persistence (no external database required)
- Event-driven authorization with detailed matching information
- Automatic cleanup of related records when deleting entities
- Input sanitization for security
- Cross-platform support

## Supported Frameworks

- .NET Standard 2.0
- .NET Standard 2.1
- .NET 8.0
- .NET 10.0

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package GateKeeper
```

Or via the Package Manager Console:

```powershell
Install-Package GateKeeper
```

## Quick Start

```csharp
using GateKeeper;

// 1. Create the RBAC server (uses SQLite database)
RbacServer server = new RbacServer();
// Or specify a custom database file:
// RbacServer server = new RbacServer("myapp.db");

// 2. Create a user
User user = server.Users.Add(new User("alice"));

// 3. Create a resource
Resource resource = server.Resources.Add(new Resource("documents"));

// 4. Create a role
Role role = server.Roles.Add(new Role("editor"));

// 5. Create a permission (role + resource + operation + allow/deny)
Permission permission = server.Permissions.Add(
    new Permission("editor-can-edit-documents", role, resource, "edit", true)
);

// 6. Assign the user to the role
UserRole userRole = server.UserRoles.Add(user, role);

// 7. Authorize a request
bool authorized = server.Authorize("alice", "edit", "documents");
Console.WriteLine($"Authorized: {authorized}"); // Output: Authorized: True
```

## Core Concepts

### Users
Entities that attempt to access resources. Users are assigned to one or more roles.

### Roles
Groups that define a set of permissions. Users inherit permissions from their assigned roles.

### Resources
Protected entities that users attempt to access (e.g., files, APIs, features).

### Permissions
Rules that grant or deny a specific operation on a resource to a role.

### Operations
Actions that can be performed on resources (e.g., "create", "read", "update", "delete").

## API Reference

### RbacServer

The main entry point for the GateKeeper library.

```csharp
// Create with default database file (gatekeeper.db)
RbacServer server = new RbacServer();

// Create with custom database file
RbacServer server = new RbacServer("custom.db");

// Set default behavior when no matching permission is found
server.DefaultPermit = false; // Default: deny

// Authorize a request
bool result = server.Authorize("username", "operation", "resource");

// Authorize with metadata (passed to events)
bool result = server.Authorize("username", "operation", "resource", myMetadata);
```

### Manager APIs

Each manager (`Users`, `Roles`, `Resources`, `Permissions`, `UserRoles`) provides:

| Method | Description |
|--------|-------------|
| `Add(entity)` | Add a new entity |
| `Remove(entity)` | Remove an entity |
| `RemoveByName(name)` | Remove an entity by name |
| `All()` | Retrieve all entities |
| `GetFirstByName(name)` | Get an entity by name |
| `ExistsByName(name)` | Check if an entity exists |

### Authorization Events

Subscribe to authorization events for logging, auditing, or custom logic:

```csharp
server.AuthorizationEvent += (sender, args) =>
{
    Console.WriteLine($"User: {args.Username}");
    Console.WriteLine($"Operation: {args.Operation}");
    Console.WriteLine($"Resource: {args.Resource}");
    Console.WriteLine($"Authorized: {args.Authorized}");
    Console.WriteLine($"Matching Entries: {args.MatchingEntries?.Count ?? 0}");

    if (args.Metadata != null)
        Console.WriteLine($"Metadata: {args.Metadata}");
};
```

## Sample Application

The `GateKeeperConsole` project provides an interactive console application for testing GateKeeper functionality. Run it to:

- Create and manage users, roles, resources, and permissions
- Test authorization scenarios
- Explore the API interactively

```bash
cd src/GateKeeperConsole
dotnet run
```

## Automated Tests

The `Test.Automated` project contains comprehensive tests covering all library functionality:

```bash
cd src/Test.Automated
dotnet run
```

The test suite validates:
- User, role, resource, and permission management
- User-role mappings
- Authorization logic
- Default permit behavior
- Authorization events
- Cascade deletes
- Input validation

## Project Structure

```
GateKeeper/
├── src/
│   ├── GateKeeper/           # Main library
│   ├── GateKeeperConsole/    # Interactive console demo
│   └── Test.Automated/       # Automated test suite
├── assets/                   # Icons and images
├── README.md
└── LICENSE.md
```

## Building from Source

```bash
cd src
dotnet restore
dotnet build
```

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## Support

- **Issues**: [GitHub Issues](https://github.com/jchristn/Gatekeeper/issues)
- **Discussions**: [GitHub Discussions](https://github.com/jchristn/Gatekeeper/discussions)

## Version History

See [CHANGELOG.md](CHANGELOG.md) for a detailed version history.

### v2.1.0

- Retargeted to .NET Standard 2.0, .NET Standard 2.1, .NET 8.0, and .NET 10.0
- Updated WatsonORM.Sqlite to v3.0.14
- Migrated to System.Text.Json (removed Newtonsoft.Json dependency from console projects)
- Reorganized project structure (source moved to `src/` directory)
- Added comprehensive automated test suite

### v2.0.0

- Breaking changes and major refactor
- Content sanitization on insert and authorization evaluation
- Event handler for authorization decisions including evaluation metadata
- Automatic cleanup of subordinate objects
