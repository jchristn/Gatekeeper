<img src="https://github.com/jchristn/Gatekeeper/raw/master/assets/icon.png" width="100" height="100">

# GateKeeper Roles-Based Access Control

[![NuGet Version](https://img.shields.io/nuget/v/GateKeeper.svg?style=flat)](https://www.nuget.org/packages/GateKeeper/) [![NuGet](https://img.shields.io/nuget/dt/GateKeeper.svg)](https://www.nuget.org/packages/GateKeeper) 

Roles-Based Access Control Library in C#

GateKeeper is a simple library for implementing roles-based access control to control access to resources by users given a specified operation type. 

With GateKeeper, you can define users, roles, and permissions, then authorize access attempts to resources (by resource name and operation).

## New in v2.0.0

- Breaking changes and major refactor
- Content sanitization on insert and authorization evaluation
- Event handler for authorization decisions including evaluation metadata
- Automatic cleanup of subordinate objects (for instance, deleting a user deletes any associated role maps)

## Help, Feedback, and Disclaimer

First things first - do you need help or have feedback?  File an issue or start a discussion!  We would love to get your feedback to help make our software better.  Also, there may be bugs or issues that we have yet to encounter!  

## Sample App

Refer to the ```GateKeeperConsole``` project for a working example.  This project will initialize a database, and optionally, prepopulate it with a series of records allowing you to test functionality.

## Sqlite and .NET Framework

You'll need to copy the ```runtimes``` directory into your application directory.  Please refer to WatsonORM (see https://github.com/jchristn/watsonorm) Test.Sqlite project.

## Enterprise Editions

If you wish to use GateKeeper in an enterprise application using your own database application, email me at joel dot christner at gmail dot com.

## Getting Started

To get up and running with GateKeeper:

1) Install the NuGet package
```
> Install-Package GateKeeper
```
2) Add the appropriate using statements
```csharp
using GateKeeper;
```
3) Instantiate
```csharp
RbacServer server = new RbacServer(); 
// or
RbacServer server = new RbacServer("MyDatabaseFilename.db");
```
4) Create users
```csharp
User user = new User("My first user");
server.Users.Add(user);
// users are entities that attempt to consume resources
```
5) Create resources
```csharp
Resource resource = new Resource("My first resource");
server.Resources.Add(resource);
// resources are entities that users attempt to consume
```
6) Create roles
```csharp
Role role = new Role("My first role");
server.Roles.Add(role);
// roles are entities to which permissions are mapped
```
7) Create permissions
```csharp
Permission perm = new Permission("My first permission", role, resource, "create", true);
// first parameter is the name of the permission
// second parameter is the role to which the permission should be assigned
// third parameter is the resource allowed or disallowed by the permission
// fourth parameter is the type of operation permitted or denied by this permission
// fifth parameter is whether or not the operation should be permitted
server.Permissions.Add(perm);
```
8) Map users to roles
```csharp
UserRole userRole = server.UserRoles.Add(user, role);
// this maps the user to the role defined in step 7
```
9) Attempt an authorization!
```csharp
bool authorized;

authorized = server.Authorize("My first user", "create", "My first resource");
// optionally, add metadata, which propagates to events
authorized = server.Authorize("My first user", "create", "My first resource", 42);
```
10) Attach authorization event handler (optional)
```csharp
server.AuthorizationEvent += MyEventHandler;

private static void MyEventHandler(object sender, AuthorizationEventArgs e)
{
  Console.WriteLine(e.Username + " attempted to " + e.Operation + " against " + e.Resource + ": " + e.Authorized);
}
```
## Additional APIs

Each of the manager instances on ```RbacServer``` (```Permissions```, ```Resources```, ```Roles```, ```Users```, ```UserRoles```) have a series of APIs for managing the underlying data.  These APIs include (not all are applicable to every manager):

- ```Add```
- ```Remove```
- ```RemoveByName```
- ```All```
- ```GetFirstByName```
- ```ExistsByName```

