namespace GateKeeper.Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using GateKeeper;
    using Touchstone.Core;

    /// <summary>
    /// Model construction, property, and input-sanitization coverage for the entity
    /// types: User, Role, Resource, Permission, and UserRole.
    /// </summary>
    public static class ModelSuites
    {
        #region User-Model

        /// <summary>
        /// User model coverage.
        /// </summary>
        /// <returns>User model suite.</returns>
        public static TestSuiteDescriptor UserModelSuite()
        {
            const string suiteId = "Model.User";
            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Model - User",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "DefaultConstructor", "Default constructor yields defaults and a 36-char GUID",
                        ct =>
                        {
                            User u = new User();
                            TestAssert.Equal("My User", u.Name, "Default name");
                            TestAssert.Equal(36, u.GUID.Length, "GUID length");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "NameConstructorSetsName", "Name constructor assigns the name",
                        ct =>
                        {
                            User u = new User("alice");
                            TestAssert.Equal("alice", u.Name, "Name set");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "NullNameConstructorThrows", "Constructor throws ArgumentNullException on null name",
                        ct =>
                        {
                            TestAssert.Throws<ArgumentNullException>(() => new User(null!), "Null name ctor");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "EmptyNameConstructorThrows", "Constructor throws ArgumentNullException on empty name",
                        ct =>
                        {
                            TestAssert.Throws<ArgumentNullException>(() => new User(string.Empty), "Empty name ctor");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "NameSetterNullThrows", "Name setter throws ArgumentNullException on null",
                        ct =>
                        {
                            User u = new User();
                            TestAssert.Throws<ArgumentNullException>(() => u.Name = null!, "Name setter null");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "GuidSetterNullThrows", "GUID setter throws ArgumentNullException on null",
                        ct =>
                        {
                            User u = new User();
                            TestAssert.Throws<ArgumentNullException>(() => u.GUID = null!, "GUID setter null");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "GuidSetterAssigns", "GUID setter assigns a valid value",
                        ct =>
                        {
                            User u = new User();
                            string g = Guid.NewGuid().ToString();
                            u.GUID = g;
                            TestAssert.Equal(g, u.GUID, "GUID assigned");
                            return Task.CompletedTask;
                        }),
                });
        }

        #endregion

        #region Role-Model

        /// <summary>
        /// Role model coverage.
        /// </summary>
        /// <returns>Role model suite.</returns>
        public static TestSuiteDescriptor RoleModelSuite()
        {
            const string suiteId = "Model.Role";
            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Model - Role",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "DefaultConstructor", "Default constructor yields defaults and a 36-char GUID",
                        ct =>
                        {
                            Role r = new Role();
                            TestAssert.Equal("My Role", r.Name, "Default name");
                            TestAssert.Equal(36, r.GUID.Length, "GUID length");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "NameConstructorSetsName", "Name constructor assigns the name",
                        ct =>
                        {
                            Role r = new Role("admin");
                            TestAssert.Equal("admin", r.Name, "Name set");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "NullNameConstructorThrows", "Constructor throws ArgumentNullException on null name",
                        ct =>
                        {
                            TestAssert.Throws<ArgumentNullException>(() => new Role(null!), "Null name ctor");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "NameSetterNullThrows", "Name setter throws ArgumentNullException on null",
                        ct =>
                        {
                            Role r = new Role();
                            TestAssert.Throws<ArgumentNullException>(() => r.Name = null!, "Name setter null");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "GuidSetterNullThrows", "GUID setter throws ArgumentNullException on null",
                        ct =>
                        {
                            Role r = new Role();
                            TestAssert.Throws<ArgumentNullException>(() => r.GUID = null!, "GUID setter null");
                            return Task.CompletedTask;
                        }),
                });
        }

        #endregion

        #region Resource-Model

        /// <summary>
        /// Resource model coverage.
        /// </summary>
        /// <returns>Resource model suite.</returns>
        public static TestSuiteDescriptor ResourceModelSuite()
        {
            const string suiteId = "Model.Resource";
            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Model - Resource",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "DefaultConstructor", "Default constructor yields defaults and a 36-char GUID",
                        ct =>
                        {
                            Resource r = new Resource();
                            TestAssert.Equal("My Resource", r.Name, "Default name");
                            TestAssert.Equal(36, r.GUID.Length, "GUID length");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "NameConstructorSetsName", "Name constructor assigns the name",
                        ct =>
                        {
                            Resource r = new Resource("documents");
                            TestAssert.Equal("documents", r.Name, "Name set");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "NullNameConstructorThrows", "Constructor throws ArgumentNullException on null name",
                        ct =>
                        {
                            TestAssert.Throws<ArgumentNullException>(() => new Resource(null!), "Null name ctor");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "NameSetterNullThrows", "Name setter throws ArgumentNullException on null",
                        ct =>
                        {
                            Resource r = new Resource();
                            TestAssert.Throws<ArgumentNullException>(() => r.Name = null!, "Name setter null");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "GuidSetterNullThrows", "GUID setter throws ArgumentNullException on null",
                        ct =>
                        {
                            Resource r = new Resource();
                            TestAssert.Throws<ArgumentNullException>(() => r.GUID = null!, "GUID setter null");
                            return Task.CompletedTask;
                        }),
                });
        }

        #endregion

        #region Permission-Model

        /// <summary>
        /// Permission model coverage.
        /// </summary>
        /// <returns>Permission model suite.</returns>
        public static TestSuiteDescriptor PermissionModelSuite()
        {
            const string suiteId = "Model.Permission";
            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Model - Permission",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "DefaultAllowIsTrue", "Default constructor leaves Allow=true",
                        ct =>
                        {
                            Permission p = new Permission();
                            TestAssert.True(p.Allow, "Default allow true");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "FullConstructorSetsProperties", "Full constructor maps name, operation, allow, and GUIDs",
                        ct =>
                        {
                            Role role = new Role("admin");
                            Resource res = new Resource("documents");
                            Permission p = new Permission("p1", role, res, "create", false);
                            TestAssert.Equal("p1", p.Name, "Name");
                            TestAssert.Equal("create", p.Operation, "Operation");
                            TestAssert.False(p.Allow, "Allow");
                            TestAssert.Equal(role.GUID, p.RoleGUID, "RoleGUID");
                            TestAssert.Equal(res.GUID, p.ResourceGUID, "ResourceGUID");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "NullNameThrows", "Constructor throws ArgumentNullException on null name",
                        ct =>
                        {
                            TestAssert.Throws<ArgumentNullException>(
                                () => new Permission(null!, new Role("admin"), new Resource("documents"), "create", true), "Null name");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "NullRoleThrows", "Constructor throws ArgumentNullException on null role",
                        ct =>
                        {
                            TestAssert.Throws<ArgumentNullException>(
                                () => new Permission("p1", null!, new Resource("documents"), "create", true), "Null role");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "NullResourceThrows", "Constructor throws ArgumentNullException on null resource",
                        ct =>
                        {
                            TestAssert.Throws<ArgumentNullException>(
                                () => new Permission("p1", new Role("admin"), null!, "create", true), "Null resource");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "NullOperationThrows", "Constructor throws ArgumentNullException on null operation",
                        ct =>
                        {
                            TestAssert.Throws<ArgumentNullException>(
                                () => new Permission("p1", new Role("admin"), new Resource("documents"), null!, true), "Null operation");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "OperationSetterNullThrows", "Operation setter throws ArgumentNullException on null",
                        ct =>
                        {
                            Permission p = new Permission();
                            TestAssert.Throws<ArgumentNullException>(() => p.Operation = null!, "Operation setter null");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "RoleGuidSetterNullThrows", "RoleGUID setter throws ArgumentNullException on null",
                        ct =>
                        {
                            Permission p = new Permission();
                            TestAssert.Throws<ArgumentNullException>(() => p.RoleGUID = null!, "RoleGUID setter null");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "ResourceGuidSetterNullThrows", "ResourceGUID setter throws ArgumentNullException on null",
                        ct =>
                        {
                            Permission p = new Permission();
                            TestAssert.Throws<ArgumentNullException>(() => p.ResourceGUID = null!, "ResourceGUID setter null");
                            return Task.CompletedTask;
                        }),
                });
        }

        #endregion

        #region UserRole-Model

        /// <summary>
        /// UserRole model coverage.
        /// </summary>
        /// <returns>User-role model suite.</returns>
        public static TestSuiteDescriptor UserRoleModelSuite()
        {
            const string suiteId = "Model.UserRole";
            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Model - UserRole",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "DefaultConstructor", "Default constructor yields a 36-char GUID",
                        ct =>
                        {
                            UserRole ur = new UserRole();
                            TestAssert.Equal(36, ur.GUID.Length, "GUID length");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "ConstructorMapsGuids", "Constructor copies the user and role GUIDs",
                        ct =>
                        {
                            User u = new User("alice");
                            Role r = new Role("admin");
                            UserRole ur = new UserRole(u, r);
                            TestAssert.Equal(u.GUID, ur.UserGUID, "UserGUID");
                            TestAssert.Equal(r.GUID, ur.RoleGUID, "RoleGUID");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "NullUserThrows", "Constructor throws ArgumentNullException on null user",
                        ct =>
                        {
                            TestAssert.Throws<ArgumentNullException>(() => new UserRole(null!, new Role("admin")), "Null user");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "NullRoleThrows", "Constructor throws ArgumentNullException on null role",
                        ct =>
                        {
                            TestAssert.Throws<ArgumentNullException>(() => new UserRole(new User("alice"), null!), "Null role");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "UserGuidSetterNullThrows", "UserGUID setter throws ArgumentNullException on null",
                        ct =>
                        {
                            UserRole ur = new UserRole();
                            TestAssert.Throws<ArgumentNullException>(() => ur.UserGUID = null!, "UserGUID setter null");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "RoleGuidSetterNullThrows", "RoleGUID setter throws ArgumentNullException on null",
                        ct =>
                        {
                            UserRole ur = new UserRole();
                            TestAssert.Throws<ArgumentNullException>(() => ur.RoleGUID = null!, "RoleGUID setter null");
                            return Task.CompletedTask;
                        }),
                });
        }

        #endregion

        #region Sanitization

        /// <summary>
        /// Input sanitization behavior exercised through the model property setters
        /// (the parameterized constructors assign fields directly and are not sanitized).
        /// </summary>
        /// <returns>Sanitization suite.</returns>
        public static TestSuiteDescriptor SanitizationSuite()
        {
            const string suiteId = "Sanitization";
            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Input Sanitization",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "StripsDoubleDash", "Setter removes SQL comment double-dashes",
                        ct =>
                        {
                            User u = new User();
                            u.Name = "ab--cd";
                            TestAssert.Equal("abcd", u.Name, "Double-dash stripped");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "StripsBlockComment", "Setter removes block-comment markers",
                        ct =>
                        {
                            User u = new User();
                            u.Name = "a/*b*/c";
                            TestAssert.Equal("abc", u.Name, "Block comment stripped");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "StripsControlCharacters", "Setter removes sub-space control characters (except CR/LF)",
                        ct =>
                        {
                            User u = new User();
                            u.Name = "a\tb";
                            TestAssert.Equal("ab", u.Name, "Tab stripped");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "PreservesNewline", "Setter preserves carriage return and line feed",
                        ct =>
                        {
                            User u = new User();
                            u.Name = "a\nb";
                            TestAssert.Equal("a\nb", u.Name, "Newline preserved");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "EscapesSingleQuote", "Setter doubles single quotes",
                        ct =>
                        {
                            User u = new User();
                            u.Name = "O'Brien";
                            TestAssert.Equal("O''Brien", u.Name, "Single quote doubled");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "FullyStrippedValueThrows", "Setter throws when the value sanitizes to empty",
                        ct =>
                        {
                            User u = new User();
                            TestAssert.Throws<ArgumentNullException>(() => u.Name = "--", "Empty after sanitize");
                            return Task.CompletedTask;
                        }),
                });
        }

        #endregion
    }
}
