namespace GateKeeper.Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using GateKeeper;
    using Touchstone.Core;

    /// <summary>
    /// Exhaustive CRUD, lookup, and validation coverage for the five entity managers:
    /// Users, Roles, Resources, Permissions, and UserRoles.
    /// </summary>
    public static class ManagerSuites
    {
        #region User-Manager

        /// <summary>
        /// UserManager coverage.
        /// </summary>
        /// <returns>User manager suite.</returns>
        public static TestSuiteDescriptor UserManagerSuite()
        {
            const string suiteId = "Users";
            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "UserManager",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "AddReturnsPersistedRow", "Add returns a user with a positive Id",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            User u = scope.Server.Users.Add(new User("alice"));
                            TestAssert.NotNull(u, "Added user");
                            TestAssert.True(u.Id > 0, "Assigned Id");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "ExistsByNameTrue", "ExistsByName returns true for an existing user",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            scope.Server.Users.Add(new User("alice"));
                            TestAssert.True(scope.Server.Users.ExistsByName("alice"), "alice exists");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "ExistsByNameFalse", "ExistsByName returns false for a missing user",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.False(scope.Server.Users.ExistsByName("ghost"), "ghost missing");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "GetFirstByNameReturns", "GetFirstByName returns the matching user",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            scope.Server.Users.Add(new User("alice"));
                            User u = scope.Server.Users.GetFirstByName("alice");
                            TestAssert.NotNull(u, "Fetched user");
                            TestAssert.Equal("alice", u.Name, "User name");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "GetFirstByNameNull", "GetFirstByName returns null for a missing user",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.Null(scope.Server.Users.GetFirstByName("ghost"), "Missing user null");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AllReturnsAdded", "All returns every added user",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            scope.Server.Users.Add(new User("alice"));
                            scope.Server.Users.Add(new User("bob"));
                            List<User> all = scope.Server.Users.All();
                            TestAssert.NotNull(all, "All list");
                            TestAssert.Equal(2, all.Count, "User count");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AllEmptyInitially", "All returns an empty list on a fresh server",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            List<User> all = scope.Server.Users.All();
                            TestAssert.NotNull(all, "All list");
                            TestAssert.Equal(0, all.Count, "Empty count");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AddDuplicateThrows", "Add throws ArgumentException on a duplicate name",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            scope.Server.Users.Add(new User("alice"));
                            TestAssert.Throws<ArgumentException>(
                                () => scope.Server.Users.Add(new User("alice")), "Duplicate user");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "RemoveByReference", "Remove deletes a user by reference",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            User u = scope.Server.Users.Add(new User("alice"));
                            scope.Server.Users.Remove(u);
                            TestAssert.False(scope.Server.Users.ExistsByName("alice"), "alice removed");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "RemoveByName", "RemoveByName deletes a user by name",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            scope.Server.Users.Add(new User("alice"));
                            scope.Server.Users.RemoveByName("alice");
                            TestAssert.False(scope.Server.Users.ExistsByName("alice"), "alice removed");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "RemoveByNameMissingThrows", "RemoveByName throws KeyNotFoundException for a missing user",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.Throws<KeyNotFoundException>(
                                () => scope.Server.Users.RemoveByName("ghost"), "Remove missing user");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "RemoveMissingThrows", "Remove throws KeyNotFoundException for a never-added user",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.Throws<KeyNotFoundException>(
                                () => scope.Server.Users.Remove(new User("ghost")), "Remove missing user ref");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AddNullThrows", "Add throws ArgumentNullException on null",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.Throws<ArgumentNullException>(
                                () => scope.Server.Users.Add(null!), "Add null user");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "RemoveNullThrows", "Remove throws ArgumentNullException on null",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.Throws<ArgumentNullException>(
                                () => scope.Server.Users.Remove(null!), "Remove null user");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "ExistsByNameNullThrows", "ExistsByName throws ArgumentNullException on null",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.Throws<ArgumentNullException>(
                                () => scope.Server.Users.ExistsByName(null!), "ExistsByName null");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "GetFirstByNameNullThrows", "GetFirstByName throws ArgumentNullException on null",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.Throws<ArgumentNullException>(
                                () => scope.Server.Users.GetFirstByName(null!), "GetFirstByName null");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "RemoveByNameNullThrows", "RemoveByName throws ArgumentNullException on null",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.Throws<ArgumentNullException>(
                                () => scope.Server.Users.RemoveByName(null!), "RemoveByName null");
                            return Task.CompletedTask;
                        }),
                });
        }

        #endregion

        #region Role-Manager

        /// <summary>
        /// RoleManager coverage.
        /// </summary>
        /// <returns>Role manager suite.</returns>
        public static TestSuiteDescriptor RoleManagerSuite()
        {
            const string suiteId = "Roles";
            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "RoleManager",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "AddReturnsPersistedRow", "Add returns a role with a positive Id",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            Role r = scope.Server.Roles.Add(new Role("admin"));
                            TestAssert.NotNull(r, "Added role");
                            TestAssert.True(r.Id > 0, "Assigned Id");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "ExistsByNameTrue", "ExistsByName returns true for an existing role",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            scope.Server.Roles.Add(new Role("admin"));
                            TestAssert.True(scope.Server.Roles.ExistsByName("admin"), "admin exists");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "ExistsByNameFalse", "ExistsByName returns false for a missing role",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.False(scope.Server.Roles.ExistsByName("ghost"), "ghost missing");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "GetFirstByNameReturns", "GetFirstByName returns the matching role",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            scope.Server.Roles.Add(new Role("admin"));
                            Role r = scope.Server.Roles.GetFirstByName("admin");
                            TestAssert.NotNull(r, "Fetched role");
                            TestAssert.Equal("admin", r.Name, "Role name");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "GetFirstByNameNull", "GetFirstByName returns null for a missing role",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.Null(scope.Server.Roles.GetFirstByName("ghost"), "Missing role null");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AllReturnsAdded", "All returns every added role",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            scope.Server.Roles.Add(new Role("admin"));
                            scope.Server.Roles.Add(new Role("viewer"));
                            TestAssert.Equal(2, scope.Server.Roles.All().Count, "Role count");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AddDuplicateThrows", "Add throws ArgumentException on a duplicate name",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            scope.Server.Roles.Add(new Role("admin"));
                            TestAssert.Throws<ArgumentException>(
                                () => scope.Server.Roles.Add(new Role("admin")), "Duplicate role");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "RemoveByReference", "Remove deletes a role by reference",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            Role r = scope.Server.Roles.Add(new Role("admin"));
                            scope.Server.Roles.Remove(r);
                            TestAssert.False(scope.Server.Roles.ExistsByName("admin"), "admin removed");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "RemoveByName", "RemoveByName deletes a role by name",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            scope.Server.Roles.Add(new Role("admin"));
                            scope.Server.Roles.RemoveByName("admin");
                            TestAssert.False(scope.Server.Roles.ExistsByName("admin"), "admin removed");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "RemoveByNameMissingThrows", "RemoveByName throws KeyNotFoundException for a missing role",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.Throws<KeyNotFoundException>(
                                () => scope.Server.Roles.RemoveByName("ghost"), "Remove missing role");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AddNullThrows", "Add throws ArgumentNullException on null",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.Throws<ArgumentNullException>(
                                () => scope.Server.Roles.Add(null!), "Add null role");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "ExistsByNameNullThrows", "ExistsByName throws ArgumentNullException on null",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.Throws<ArgumentNullException>(
                                () => scope.Server.Roles.ExistsByName(null!), "ExistsByName null");
                            return Task.CompletedTask;
                        }),
                });
        }

        #endregion

        #region Resource-Manager

        /// <summary>
        /// ResourceManager coverage.
        /// </summary>
        /// <returns>Resource manager suite.</returns>
        public static TestSuiteDescriptor ResourceManagerSuite()
        {
            const string suiteId = "Resources";
            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "ResourceManager",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "AddReturnsPersistedRow", "Add returns a resource with a positive Id",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            Resource r = scope.Server.Resources.Add(new Resource("documents"));
                            TestAssert.NotNull(r, "Added resource");
                            TestAssert.True(r.Id > 0, "Assigned Id");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "ExistsByNameTrue", "ExistsByName returns true for an existing resource",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            scope.Server.Resources.Add(new Resource("documents"));
                            TestAssert.True(scope.Server.Resources.ExistsByName("documents"), "documents exists");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "ExistsByNameFalse", "ExistsByName returns false for a missing resource",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.False(scope.Server.Resources.ExistsByName("ghost"), "ghost missing");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "GetFirstByNameReturns", "GetFirstByName returns the matching resource",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            scope.Server.Resources.Add(new Resource("documents"));
                            Resource r = scope.Server.Resources.GetFirstByName("documents");
                            TestAssert.NotNull(r, "Fetched resource");
                            TestAssert.Equal("documents", r.Name, "Resource name");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "GetFirstByNameNull", "GetFirstByName returns null for a missing resource",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.Null(scope.Server.Resources.GetFirstByName("ghost"), "Missing resource null");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AllReturnsAdded", "All returns every added resource",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            scope.Server.Resources.Add(new Resource("documents"));
                            scope.Server.Resources.Add(new Resource("reports"));
                            TestAssert.Equal(2, scope.Server.Resources.All().Count, "Resource count");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AddDuplicateThrows", "Add throws ArgumentException on a duplicate name",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            scope.Server.Resources.Add(new Resource("documents"));
                            TestAssert.Throws<ArgumentException>(
                                () => scope.Server.Resources.Add(new Resource("documents")), "Duplicate resource");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "RemoveByReference", "Remove deletes a resource by reference",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            Resource r = scope.Server.Resources.Add(new Resource("documents"));
                            scope.Server.Resources.Remove(r);
                            TestAssert.False(scope.Server.Resources.ExistsByName("documents"), "documents removed");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "RemoveByName", "RemoveByName deletes a resource by name",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            scope.Server.Resources.Add(new Resource("documents"));
                            scope.Server.Resources.RemoveByName("documents");
                            TestAssert.False(scope.Server.Resources.ExistsByName("documents"), "documents removed");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "RemoveByNameMissingThrows", "RemoveByName throws KeyNotFoundException for a missing resource",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.Throws<KeyNotFoundException>(
                                () => scope.Server.Resources.RemoveByName("ghost"), "Remove missing resource");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AddNullThrows", "Add throws ArgumentNullException on null",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.Throws<ArgumentNullException>(
                                () => scope.Server.Resources.Add(null!), "Add null resource");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "ExistsByNameNullThrows", "ExistsByName throws ArgumentNullException on null",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.Throws<ArgumentNullException>(
                                () => scope.Server.Resources.ExistsByName(null!), "ExistsByName null");
                            return Task.CompletedTask;
                        }),
                });
        }

        #endregion

        #region Permission-Manager

        /// <summary>
        /// PermissionManager coverage.
        /// </summary>
        /// <returns>Permission manager suite.</returns>
        public static TestSuiteDescriptor PermissionManagerSuite()
        {
            const string suiteId = "Permissions";
            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "PermissionManager",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "AddReturnsPersistedRow", "Add returns a permission with a positive Id",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            Role role = scope.Server.Roles.Add(new Role("admin"));
                            Resource res = scope.Server.Resources.Add(new Resource("documents"));
                            Permission p = scope.Server.Permissions.Add(new Permission("p1", role, res, "create", true));
                            TestAssert.NotNull(p, "Added permission");
                            TestAssert.True(p.Id > 0, "Assigned Id");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "ExistsByNameTrue", "ExistsByName returns true for an existing permission",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            Role role = scope.Server.Roles.Add(new Role("admin"));
                            Resource res = scope.Server.Resources.Add(new Resource("documents"));
                            scope.Server.Permissions.Add(new Permission("p1", role, res, "create", true));
                            TestAssert.True(scope.Server.Permissions.ExistsByName("p1"), "p1 exists");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "ExistsByNameFalse", "ExistsByName returns false for a missing permission",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.False(scope.Server.Permissions.ExistsByName("ghost"), "ghost missing");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "GetFirstByNameReturns", "GetFirstByName returns the matching permission",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            Role role = scope.Server.Roles.Add(new Role("admin"));
                            Resource res = scope.Server.Resources.Add(new Resource("documents"));
                            scope.Server.Permissions.Add(new Permission("p1", role, res, "create", true));
                            Permission p = scope.Server.Permissions.GetFirstByName("p1");
                            TestAssert.NotNull(p, "Fetched permission");
                            TestAssert.Equal("p1", p.Name, "Permission name");
                            TestAssert.Equal("create", p.Operation, "Permission operation");
                            TestAssert.True(p.Allow, "Permission allow");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "DenyPermissionPersistsAllowFalse", "A deny permission round-trips with Allow=false",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            Role role = scope.Server.Roles.Add(new Role("admin"));
                            Resource res = scope.Server.Resources.Add(new Resource("documents"));
                            scope.Server.Permissions.Add(new Permission("deny1", role, res, "delete", false));
                            Permission p = scope.Server.Permissions.GetFirstByName("deny1");
                            TestAssert.NotNull(p, "Fetched deny permission");
                            TestAssert.False(p.Allow, "Allow persisted false");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AllReturnsAdded", "All returns every added permission",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            Role role = scope.Server.Roles.Add(new Role("admin"));
                            Resource res = scope.Server.Resources.Add(new Resource("documents"));
                            scope.Server.Permissions.Add(new Permission("p1", role, res, "create", true));
                            scope.Server.Permissions.Add(new Permission("p2", role, res, "read", true));
                            TestAssert.Equal(2, scope.Server.Permissions.All().Count, "Permission count");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AddDuplicateThrows", "Add throws ArgumentException on a duplicate name",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            Role role = scope.Server.Roles.Add(new Role("admin"));
                            Resource res = scope.Server.Resources.Add(new Resource("documents"));
                            scope.Server.Permissions.Add(new Permission("p1", role, res, "create", true));
                            TestAssert.Throws<ArgumentException>(
                                () => scope.Server.Permissions.Add(new Permission("p1", role, res, "read", true)), "Duplicate permission");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "GetByResourceReturnsMatches", "GetByResource returns only permissions for that resource",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            Role role = scope.Server.Roles.Add(new Role("admin"));
                            Resource docs = scope.Server.Resources.Add(new Resource("documents"));
                            Resource reps = scope.Server.Resources.Add(new Resource("reports"));
                            scope.Server.Permissions.Add(new Permission("p1", role, docs, "create", true));
                            scope.Server.Permissions.Add(new Permission("p2", role, docs, "read", true));
                            scope.Server.Permissions.Add(new Permission("p3", role, reps, "read", true));
                            List<Permission> docPerms = scope.Server.Permissions.GetByResource(docs);
                            TestAssert.Equal(2, docPerms.Count, "documents permission count");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "GetByResourceNullThrows", "GetByResource throws ArgumentNullException on null",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.Throws<ArgumentNullException>(
                                () => scope.Server.Permissions.GetByResource(null!), "GetByResource null");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "RemoveByReference", "Remove deletes a permission by reference",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            Role role = scope.Server.Roles.Add(new Role("admin"));
                            Resource res = scope.Server.Resources.Add(new Resource("documents"));
                            Permission p = scope.Server.Permissions.Add(new Permission("p1", role, res, "create", true));
                            scope.Server.Permissions.Remove(p);
                            TestAssert.False(scope.Server.Permissions.ExistsByName("p1"), "p1 removed");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "RemoveByName", "RemoveByName deletes a permission by name",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            Role role = scope.Server.Roles.Add(new Role("admin"));
                            Resource res = scope.Server.Resources.Add(new Resource("documents"));
                            scope.Server.Permissions.Add(new Permission("p1", role, res, "create", true));
                            scope.Server.Permissions.RemoveByName("p1");
                            TestAssert.False(scope.Server.Permissions.ExistsByName("p1"), "p1 removed");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "RemoveByNameMissingThrows", "RemoveByName throws KeyNotFoundException for a missing permission",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.Throws<KeyNotFoundException>(
                                () => scope.Server.Permissions.RemoveByName("ghost"), "Remove missing permission");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AddNullThrows", "Add throws ArgumentNullException on null",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.Throws<ArgumentNullException>(
                                () => scope.Server.Permissions.Add(null!), "Add null permission");
                            return Task.CompletedTask;
                        }),
                });
        }

        #endregion

        #region UserRole-Manager

        /// <summary>
        /// UserRoleManager coverage.
        /// </summary>
        /// <returns>User-role manager suite.</returns>
        public static TestSuiteDescriptor UserRoleManagerSuite()
        {
            const string suiteId = "UserRoles";
            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "UserRoleManager",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "AddReturnsPersistedRow", "Add returns a mapping with a positive Id",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            User u = scope.Server.Users.Add(new User("alice"));
                            Role r = scope.Server.Roles.Add(new Role("admin"));
                            UserRole ur = scope.Server.UserRoles.Add(u, r);
                            TestAssert.NotNull(ur, "Added mapping");
                            TestAssert.True(ur.Id > 0, "Assigned Id");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "ExistsTrue", "Exists returns true for an existing mapping",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            User u = scope.Server.Users.Add(new User("alice"));
                            Role r = scope.Server.Roles.Add(new Role("admin"));
                            scope.Server.UserRoles.Add(u, r);
                            TestAssert.True(scope.Server.UserRoles.Exists(u, r), "mapping exists");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "ExistsFalse", "Exists returns false for a non-existent mapping",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            User u = scope.Server.Users.Add(new User("alice"));
                            Role r = scope.Server.Roles.Add(new Role("admin"));
                            Role other = scope.Server.Roles.Add(new Role("viewer"));
                            scope.Server.UserRoles.Add(u, r);
                            TestAssert.False(scope.Server.UserRoles.Exists(u, other), "mapping absent");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AllReturnsAdded", "All returns every mapping",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            User u = scope.Server.Users.Add(new User("alice"));
                            Role r1 = scope.Server.Roles.Add(new Role("admin"));
                            Role r2 = scope.Server.Roles.Add(new Role("viewer"));
                            scope.Server.UserRoles.Add(u, r1);
                            scope.Server.UserRoles.Add(u, r2);
                            TestAssert.Equal(2, scope.Server.UserRoles.All().Count, "mapping count");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "GetByUser", "GetByUser returns mappings for the user",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            User alice = scope.Server.Users.Add(new User("alice"));
                            User bob = scope.Server.Users.Add(new User("bob"));
                            Role r1 = scope.Server.Roles.Add(new Role("admin"));
                            Role r2 = scope.Server.Roles.Add(new Role("viewer"));
                            scope.Server.UserRoles.Add(alice, r1);
                            scope.Server.UserRoles.Add(alice, r2);
                            scope.Server.UserRoles.Add(bob, r1);
                            TestAssert.Equal(2, scope.Server.UserRoles.GetByUser(alice).Count, "alice mappings");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "GetByRole", "GetByRole returns mappings for the role",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            User alice = scope.Server.Users.Add(new User("alice"));
                            User bob = scope.Server.Users.Add(new User("bob"));
                            Role admin = scope.Server.Roles.Add(new Role("admin"));
                            scope.Server.UserRoles.Add(alice, admin);
                            scope.Server.UserRoles.Add(bob, admin);
                            TestAssert.Equal(2, scope.Server.UserRoles.GetByRole(admin).Count, "admin mappings");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "GetByUserRoleReturns", "GetByUserRole returns the specific mapping",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            User u = scope.Server.Users.Add(new User("alice"));
                            Role r = scope.Server.Roles.Add(new Role("admin"));
                            scope.Server.UserRoles.Add(u, r);
                            TestAssert.NotNull(scope.Server.UserRoles.GetByUserRole(u, r), "specific mapping");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "GetByUserRoleNull", "GetByUserRole returns null for a non-existent mapping",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            User u = scope.Server.Users.Add(new User("alice"));
                            Role r = scope.Server.Roles.Add(new Role("admin"));
                            TestAssert.Null(scope.Server.UserRoles.GetByUserRole(u, r), "no mapping");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AddDuplicateThrows", "Add throws ArgumentException on a duplicate mapping",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            User u = scope.Server.Users.Add(new User("alice"));
                            Role r = scope.Server.Roles.Add(new Role("admin"));
                            scope.Server.UserRoles.Add(u, r);
                            TestAssert.Throws<ArgumentException>(
                                () => scope.Server.UserRoles.Add(u, r), "Duplicate mapping");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AddUnknownUserThrows", "Add throws KeyNotFoundException when the user was never persisted",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            Role r = scope.Server.Roles.Add(new Role("admin"));
                            TestAssert.Throws<KeyNotFoundException>(
                                () => scope.Server.UserRoles.Add(new User("ghost"), r), "Add unknown user");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AddUnknownRoleThrows", "Add throws KeyNotFoundException when the role was never persisted",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            User u = scope.Server.Users.Add(new User("alice"));
                            TestAssert.Throws<KeyNotFoundException>(
                                () => scope.Server.UserRoles.Add(u, new Role("ghost")), "Add unknown role");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AddNullUserThrows", "Add throws ArgumentNullException on a null user",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            Role r = scope.Server.Roles.Add(new Role("admin"));
                            TestAssert.Throws<ArgumentNullException>(
                                () => scope.Server.UserRoles.Add(null!, r), "Add null user");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AddNullRoleThrows", "Add throws ArgumentNullException on a null role",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            User u = scope.Server.Users.Add(new User("alice"));
                            TestAssert.Throws<ArgumentNullException>(
                                () => scope.Server.UserRoles.Add(u, null!), "Add null role");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "Remove", "Remove deletes an existing mapping",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            User u = scope.Server.Users.Add(new User("alice"));
                            Role r = scope.Server.Roles.Add(new Role("admin"));
                            scope.Server.UserRoles.Add(u, r);
                            scope.Server.UserRoles.Remove(u, r);
                            TestAssert.False(scope.Server.UserRoles.Exists(u, r), "mapping removed");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "RemoveMissingThrows", "Remove throws KeyNotFoundException for a non-existent mapping",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            User u = scope.Server.Users.Add(new User("alice"));
                            Role r = scope.Server.Roles.Add(new Role("admin"));
                            TestAssert.Throws<KeyNotFoundException>(
                                () => scope.Server.UserRoles.Remove(u, r), "Remove missing mapping");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "RemoveUserRolesByUserClearsOnlyThatUser", "RemoveUserRolesByUser removes every mapping for the user and leaves others intact",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            User alice = scope.Server.Users.Add(new User("alice"));
                            User bob = scope.Server.Users.Add(new User("bob"));
                            Role r1 = scope.Server.Roles.Add(new Role("admin"));
                            Role r2 = scope.Server.Roles.Add(new Role("viewer"));
                            scope.Server.UserRoles.Add(alice, r1);
                            scope.Server.UserRoles.Add(alice, r2);
                            scope.Server.UserRoles.Add(bob, r1);
                            scope.Server.UserRoles.RemoveUserRolesByUser(alice);
                            TestAssert.Equal(0, scope.Server.UserRoles.GetByUser(alice).Count, "alice mappings cleared");
                            TestAssert.Equal(1, scope.Server.UserRoles.GetByUser(bob).Count, "bob mapping intact");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "RemoveUserRolesByRoleClearsOnlyThatRole", "RemoveUserRolesByRole removes every mapping for the role and leaves others intact",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            User alice = scope.Server.Users.Add(new User("alice"));
                            User bob = scope.Server.Users.Add(new User("bob"));
                            Role admin = scope.Server.Roles.Add(new Role("admin"));
                            Role viewer = scope.Server.Roles.Add(new Role("viewer"));
                            scope.Server.UserRoles.Add(alice, admin);
                            scope.Server.UserRoles.Add(bob, admin);
                            scope.Server.UserRoles.Add(alice, viewer);
                            scope.Server.UserRoles.RemoveUserRolesByRole(admin);
                            TestAssert.Equal(0, scope.Server.UserRoles.GetByRole(admin).Count, "admin mappings cleared");
                            TestAssert.Equal(1, scope.Server.UserRoles.GetByRole(viewer).Count, "viewer mapping intact");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "RemoveUserRolesByUserNullThrows", "RemoveUserRolesByUser throws ArgumentNullException on null",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.Throws<ArgumentNullException>(
                                () => scope.Server.UserRoles.RemoveUserRolesByUser(null!), "RemoveUserRolesByUser null");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "RemoveUserRolesByRoleNullThrows", "RemoveUserRolesByRole throws ArgumentNullException on null",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.Throws<ArgumentNullException>(
                                () => scope.Server.UserRoles.RemoveUserRolesByRole(null!), "RemoveUserRolesByRole null");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "ExistsNullRoleThrows", "Exists throws ArgumentNullException on a null role",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            User u = scope.Server.Users.Add(new User("alice"));
                            TestAssert.Throws<ArgumentNullException>(
                                () => scope.Server.UserRoles.Exists(u, null!), "Exists null role");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "ExistsNullUserThrows", "Exists throws ArgumentNullException on a null user",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            Role r = scope.Server.Roles.Add(new Role("admin"));
                            TestAssert.Throws<ArgumentNullException>(
                                () => scope.Server.UserRoles.Exists(null!, r), "Exists null user");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "GetByUserNullThrows", "GetByUser throws ArgumentNullException on null",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.Throws<ArgumentNullException>(
                                () => scope.Server.UserRoles.GetByUser(null!), "GetByUser null");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "GetByRoleNullThrows", "GetByRole throws ArgumentNullException on null",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.Throws<ArgumentNullException>(
                                () => scope.Server.UserRoles.GetByRole(null!), "GetByRole null");
                            return Task.CompletedTask;
                        }),
                });
        }

        #endregion
    }
}
