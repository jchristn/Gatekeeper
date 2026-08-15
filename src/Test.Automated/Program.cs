using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using GateKeeper;

namespace Test.Automated
{
    class Program
    {
        private static string _DbFile = "test_gatekeeper.db";
        private static RbacServer _Server = null;
        private static List<string> _PassedTests = new List<string>();
        private static List<string> _FailedTests = new List<string>();
        private static bool _AuthorizationEventFired = false;
        private static AuthorizationEventArgs _LastAuthorizationEvent = null;

        static int Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine(" GateKeeper Automated Test Suite");
            Console.WriteLine("========================================");
            Console.WriteLine();

            // Clean up any previous test database
            if (File.Exists(_DbFile))
            {
                File.Delete(_DbFile);
            }

            try
            {
                // Initialize server
                _Server = new RbacServer(_DbFile);
                _Server.AuthorizationEvent += OnAuthorizationEvent;

                // Run all tests
                RunServerTests();
                RunUserManagerTests();
                RunRoleManagerTests();
                RunResourceManagerTests();
                RunPermissionManagerTests();
                RunUserRoleManagerTests();
                RunAuthorizationTests();
                RunDefaultPermitTests();
                RunAuthorizationEventTests();
                RunCascadeDeleteTests();
                RunInputValidationTests();

                // Print summary
                PrintSummary();

                // Return exit code
                return _FailedTests.Count > 0 ? 1 : 0;
            }
            finally
            {
                // Cleanup
                _Server = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Thread.Sleep(100);

                try
                {
                    if (File.Exists(_DbFile))
                    {
                        File.Delete(_DbFile);
                    }
                }
                catch { }
            }
        }

        private static void OnAuthorizationEvent(object sender, AuthorizationEventArgs e)
        {
            _AuthorizationEventFired = true;
            _LastAuthorizationEvent = e;
        }

        private static void RunTest(string testName, Func<bool> test)
        {
            try
            {
                bool result = test();
                if (result)
                {
                    Console.WriteLine($"  [PASS] {testName}");
                    _PassedTests.Add(testName);
                }
                else
                {
                    Console.WriteLine($"  [FAIL] {testName}");
                    _FailedTests.Add(testName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [FAIL] {testName} - Exception: {ex.Message}");
                _FailedTests.Add($"{testName} (Exception: {ex.Message})");
            }
        }

        private static void RunServerTests()
        {
            Console.WriteLine();
            Console.WriteLine("--- Server Tests ---");

            RunTest("Server instantiation", () => _Server != null);
            RunTest("Server version not empty", () => !string.IsNullOrEmpty(_Server.Version));
            RunTest("DefaultPermit default is false", () => _Server.DefaultPermit == false);
            RunTest("Users manager exists", () => _Server.Users != null);
            RunTest("Roles manager exists", () => _Server.Roles != null);
            RunTest("Resources manager exists", () => _Server.Resources != null);
            RunTest("Permissions manager exists", () => _Server.Permissions != null);
            RunTest("UserRoles manager exists", () => _Server.UserRoles != null);
        }

        private static void RunUserManagerTests()
        {
            Console.WriteLine();
            Console.WriteLine("--- User Manager Tests ---");

            // Add user
            User user1 = null;
            RunTest("Add user", () =>
            {
                user1 = _Server.Users.Add(new User("testuser1"));
                return user1 != null && user1.Id > 0;
            });

            // Check exists
            RunTest("ExistsByName returns true for existing user", () =>
            {
                return _Server.Users.ExistsByName("testuser1");
            });

            RunTest("ExistsByName returns false for non-existing user", () =>
            {
                return !_Server.Users.ExistsByName("nonexistent");
            });

            // Get by name
            RunTest("GetFirstByName returns user", () =>
            {
                var user = _Server.Users.GetFirstByName("testuser1");
                return user != null && user.Name == "testuser1";
            });

            RunTest("GetFirstByName returns null for non-existing", () =>
            {
                var user = _Server.Users.GetFirstByName("nonexistent");
                return user == null;
            });

            // All users
            RunTest("All returns list with users", () =>
            {
                var users = _Server.Users.All();
                return users != null && users.Count >= 1;
            });

            // Add duplicate should throw
            RunTest("Add duplicate user throws ArgumentException", () =>
            {
                try
                {
                    _Server.Users.Add(new User("testuser1"));
                    return false;
                }
                catch (ArgumentException)
                {
                    return true;
                }
            });

            // Add second user
            User user2 = null;
            RunTest("Add second user", () =>
            {
                user2 = _Server.Users.Add(new User("testuser2"));
                return user2 != null && user2.Id > 0;
            });

            // Remove by name
            RunTest("RemoveByName removes user", () =>
            {
                _Server.Users.RemoveByName("testuser2");
                return !_Server.Users.ExistsByName("testuser2");
            });

            // Remove non-existing throws
            RunTest("RemoveByName for non-existing throws KeyNotFoundException", () =>
            {
                try
                {
                    _Server.Users.RemoveByName("nonexistent");
                    return false;
                }
                catch (KeyNotFoundException)
                {
                    return true;
                }
            });

            // Add user for later tests
            _Server.Users.Add(new User("testuser2"));
            _Server.Users.Add(new User("testuser3"));
        }

        private static void RunRoleManagerTests()
        {
            Console.WriteLine();
            Console.WriteLine("--- Role Manager Tests ---");

            // Add role
            Role role1 = null;
            RunTest("Add role", () =>
            {
                role1 = _Server.Roles.Add(new Role("testrole1"));
                return role1 != null && role1.Id > 0;
            });

            // Check exists
            RunTest("ExistsByName returns true for existing role", () =>
            {
                return _Server.Roles.ExistsByName("testrole1");
            });

            RunTest("ExistsByName returns false for non-existing role", () =>
            {
                return !_Server.Roles.ExistsByName("nonexistent");
            });

            // Get by name
            RunTest("GetFirstByName returns role", () =>
            {
                var role = _Server.Roles.GetFirstByName("testrole1");
                return role != null && role.Name == "testrole1";
            });

            // All roles
            RunTest("All returns list with roles", () =>
            {
                var roles = _Server.Roles.All();
                return roles != null && roles.Count >= 1;
            });

            // Add duplicate should throw
            RunTest("Add duplicate role throws ArgumentException", () =>
            {
                try
                {
                    _Server.Roles.Add(new Role("testrole1"));
                    return false;
                }
                catch (ArgumentException)
                {
                    return true;
                }
            });

            // Add second role
            RunTest("Add second role", () =>
            {
                var role2 = _Server.Roles.Add(new Role("testrole2"));
                return role2 != null && role2.Id > 0;
            });

            // Remove by name
            RunTest("RemoveByName removes role", () =>
            {
                _Server.Roles.RemoveByName("testrole2");
                return !_Server.Roles.ExistsByName("testrole2");
            });

            // Add roles for later tests
            _Server.Roles.Add(new Role("testrole2"));
            _Server.Roles.Add(new Role("testrole3"));
        }

        private static void RunResourceManagerTests()
        {
            Console.WriteLine();
            Console.WriteLine("--- Resource Manager Tests ---");

            // Add resource
            Resource res1 = null;
            RunTest("Add resource", () =>
            {
                res1 = _Server.Resources.Add(new Resource("testresource1"));
                return res1 != null && res1.Id > 0;
            });

            // Check exists
            RunTest("ExistsByName returns true for existing resource", () =>
            {
                return _Server.Resources.ExistsByName("testresource1");
            });

            RunTest("ExistsByName returns false for non-existing resource", () =>
            {
                return !_Server.Resources.ExistsByName("nonexistent");
            });

            // Get by name
            RunTest("GetFirstByName returns resource", () =>
            {
                var res = _Server.Resources.GetFirstByName("testresource1");
                return res != null && res.Name == "testresource1";
            });

            // All resources
            RunTest("All returns list with resources", () =>
            {
                var resources = _Server.Resources.All();
                return resources != null && resources.Count >= 1;
            });

            // Add duplicate should throw
            RunTest("Add duplicate resource throws ArgumentException", () =>
            {
                try
                {
                    _Server.Resources.Add(new Resource("testresource1"));
                    return false;
                }
                catch (ArgumentException)
                {
                    return true;
                }
            });

            // Add resources for later tests
            _Server.Resources.Add(new Resource("testresource2"));
            _Server.Resources.Add(new Resource("testresource3"));
        }

        private static void RunPermissionManagerTests()
        {
            Console.WriteLine();
            Console.WriteLine("--- Permission Manager Tests ---");

            var role = _Server.Roles.GetFirstByName("testrole1");
            var resource = _Server.Resources.GetFirstByName("testresource1");

            // Add permission
            Permission perm1 = null;
            RunTest("Add permission", () =>
            {
                perm1 = _Server.Permissions.Add(new Permission("testperm1", role, resource, "create", true));
                return perm1 != null && perm1.Id > 0;
            });

            // Check exists
            RunTest("ExistsByName returns true for existing permission", () =>
            {
                return _Server.Permissions.ExistsByName("testperm1");
            });

            RunTest("ExistsByName returns false for non-existing permission", () =>
            {
                return !_Server.Permissions.ExistsByName("nonexistent");
            });

            // Get by name
            RunTest("GetFirstByName returns permission", () =>
            {
                var perm = _Server.Permissions.GetFirstByName("testperm1");
                return perm != null && perm.Name == "testperm1";
            });

            // All permissions
            RunTest("All returns list with permissions", () =>
            {
                var permissions = _Server.Permissions.All();
                return permissions != null && permissions.Count >= 1;
            });

            // Add duplicate should throw
            RunTest("Add duplicate permission throws ArgumentException", () =>
            {
                try
                {
                    _Server.Permissions.Add(new Permission("testperm1", role, resource, "create", true));
                    return false;
                }
                catch (ArgumentException)
                {
                    return true;
                }
            });

            // GetByResource
            RunTest("GetByResource returns permissions for resource", () =>
            {
                var perms = _Server.Permissions.GetByResource(resource);
                return perms != null && perms.Count >= 1;
            });

            // Add more permissions for tests
            var role2 = _Server.Roles.GetFirstByName("testrole2");
            var resource2 = _Server.Resources.GetFirstByName("testresource2");
            _Server.Permissions.Add(new Permission("testperm2", role, resource, "read", true));
            _Server.Permissions.Add(new Permission("testperm3", role2, resource2, "delete", true));
            _Server.Permissions.Add(new Permission("testperm_deny", role, resource, "admin", false));

            // RemoveByName
            RunTest("RemoveByName removes permission", () =>
            {
                _Server.Permissions.Add(new Permission("testperm_toremove", role, resource, "test", true));
                _Server.Permissions.RemoveByName("testperm_toremove");
                return !_Server.Permissions.ExistsByName("testperm_toremove");
            });
        }

        private static void RunUserRoleManagerTests()
        {
            Console.WriteLine();
            Console.WriteLine("--- UserRole Manager Tests ---");

            var user1 = _Server.Users.GetFirstByName("testuser1");
            var user2 = _Server.Users.GetFirstByName("testuser2");
            var role1 = _Server.Roles.GetFirstByName("testrole1");
            var role2 = _Server.Roles.GetFirstByName("testrole2");

            // Add user role mapping
            UserRole ur1 = null;
            RunTest("Add user-role mapping", () =>
            {
                ur1 = _Server.UserRoles.Add(user1, role1);
                return ur1 != null && ur1.Id > 0;
            });

            // Check exists
            RunTest("Exists returns true for existing mapping", () =>
            {
                return _Server.UserRoles.Exists(user1, role1);
            });

            RunTest("Exists returns false for non-existing mapping", () =>
            {
                return !_Server.UserRoles.Exists(user2, role1);
            });

            // All
            RunTest("All returns list with user-role mappings", () =>
            {
                var mappings = _Server.UserRoles.All();
                return mappings != null && mappings.Count >= 1;
            });

            // GetByUser
            RunTest("GetByUser returns mappings for user", () =>
            {
                var mappings = _Server.UserRoles.GetByUser(user1);
                return mappings != null && mappings.Count >= 1;
            });

            // GetByRole
            RunTest("GetByRole returns mappings for role", () =>
            {
                var mappings = _Server.UserRoles.GetByRole(role1);
                return mappings != null && mappings.Count >= 1;
            });

            // GetByUserRole
            RunTest("GetByUserRole returns specific mapping", () =>
            {
                var mapping = _Server.UserRoles.GetByUserRole(user1, role1);
                return mapping != null;
            });

            // Add duplicate should throw
            RunTest("Add duplicate user-role mapping throws ArgumentException", () =>
            {
                try
                {
                    _Server.UserRoles.Add(user1, role1);
                    return false;
                }
                catch (ArgumentException)
                {
                    return true;
                }
            });

            // Add more mappings for tests
            _Server.UserRoles.Add(user2, role2);

            // Remove
            RunTest("Remove removes user-role mapping", () =>
            {
                var user3 = _Server.Users.GetFirstByName("testuser3");
                var role3 = _Server.Roles.GetFirstByName("testrole3");
                _Server.UserRoles.Add(user3, role3);
                _Server.UserRoles.Remove(user3, role3);
                return !_Server.UserRoles.Exists(user3, role3);
            });
        }

        private static void RunAuthorizationTests()
        {
            Console.WriteLine();
            Console.WriteLine("--- Authorization Tests ---");

            // User1 has role1, role1 has permission to "create" on resource1
            RunTest("Authorize returns true for permitted operation", () =>
            {
                return _Server.Authorize("testuser1", "create", "testresource1");
            });

            RunTest("Authorize returns true for read operation", () =>
            {
                return _Server.Authorize("testuser1", "read", "testresource1");
            });

            // User2 has role2, role2 has permission to "delete" on resource2
            RunTest("Authorize returns true for user2 delete on resource2", () =>
            {
                return _Server.Authorize("testuser2", "delete", "testresource2");
            });

            // User1 does not have permission for delete on resource1
            RunTest("Authorize returns false for unpermitted operation", () =>
            {
                return !_Server.Authorize("testuser1", "delete", "testresource1");
            });

            // Non-existent user
            RunTest("Authorize returns false for non-existent user", () =>
            {
                return !_Server.Authorize("nonexistent", "create", "testresource1");
            });

            // Non-existent resource
            RunTest("Authorize returns false for non-existent resource", () =>
            {
                return !_Server.Authorize("testuser1", "create", "nonexistent");
            });

            // Deny permission (allow=false) - user1 has admin permission denied
            RunTest("Authorize returns false for explicitly denied permission", () =>
            {
                return !_Server.Authorize("testuser1", "admin", "testresource1");
            });

            // Authorization with metadata
            RunTest("Authorize with metadata works", () =>
            {
                return _Server.Authorize("testuser1", "create", "testresource1", new { RequestId = 123 });
            });
        }

        private static void RunDefaultPermitTests()
        {
            Console.WriteLine();
            Console.WriteLine("--- DefaultPermit Tests ---");

            // Test with DefaultPermit = false (default)
            RunTest("DefaultPermit false - unauthorized returns false", () =>
            {
                _Server.DefaultPermit = false;
                return !_Server.Authorize("testuser1", "unknown_operation", "testresource1");
            });

            // Test with DefaultPermit = true
            RunTest("DefaultPermit true - no matching permission returns true", () =>
            {
                _Server.DefaultPermit = true;
                bool result = _Server.Authorize("testuser1", "unknown_operation_2", "testresource3");
                _Server.DefaultPermit = false; // Reset
                return result;
            });

            // Reset DefaultPermit
            _Server.DefaultPermit = false;
        }

        private static void RunAuthorizationEventTests()
        {
            Console.WriteLine();
            Console.WriteLine("--- Authorization Event Tests ---");

            _AuthorizationEventFired = false;
            _LastAuthorizationEvent = null;

            RunTest("AuthorizationEvent fires on Authorize", () =>
            {
                _Server.Authorize("testuser1", "create", "testresource1");
                Thread.Sleep(100); // Give async event time to fire
                return _AuthorizationEventFired;
            });

            RunTest("AuthorizationEvent contains correct username", () =>
            {
                return _LastAuthorizationEvent != null && _LastAuthorizationEvent.Username == "testuser1";
            });

            RunTest("AuthorizationEvent contains correct operation", () =>
            {
                return _LastAuthorizationEvent != null && _LastAuthorizationEvent.Operation == "create";
            });

            RunTest("AuthorizationEvent contains correct resource", () =>
            {
                return _LastAuthorizationEvent != null && _LastAuthorizationEvent.Resource == "testresource1";
            });

            RunTest("AuthorizationEvent contains Authorized=true for permitted", () =>
            {
                return _LastAuthorizationEvent != null && _LastAuthorizationEvent.Authorized == true;
            });

            // Test with metadata
            _AuthorizationEventFired = false;
            _LastAuthorizationEvent = null;
            RunTest("AuthorizationEvent contains metadata", () =>
            {
                _Server.Authorize("testuser1", "create", "testresource1", "test_metadata");
                Thread.Sleep(100);
                return _LastAuthorizationEvent != null &&
                       _LastAuthorizationEvent.Metadata != null &&
                       _LastAuthorizationEvent.Metadata.ToString() == "test_metadata";
            });

            // Test matching entries
            RunTest("AuthorizationEvent contains matching entries for authorized request", () =>
            {
                _Server.Authorize("testuser1", "create", "testresource1");
                Thread.Sleep(100);
                return _LastAuthorizationEvent != null &&
                       _LastAuthorizationEvent.MatchingEntries != null &&
                       _LastAuthorizationEvent.MatchingEntries.Count > 0;
            });
        }

        private static void RunCascadeDeleteTests()
        {
            Console.WriteLine();
            Console.WriteLine("--- Cascade Delete Tests ---");

            // Setup for cascade tests
            var cascadeUser = _Server.Users.Add(new User("cascade_user"));
            var cascadeRole = _Server.Roles.Add(new Role("cascade_role"));
            var cascadeResource = _Server.Resources.Add(new Resource("cascade_resource"));
            _Server.UserRoles.Add(cascadeUser, cascadeRole);
            _Server.Permissions.Add(new Permission("cascade_perm", cascadeRole, cascadeResource, "cascade_op", true));

            // Delete user should remove user-role mappings
            RunTest("Deleting user removes associated user-role mappings", () =>
            {
                _Server.Users.Remove(cascadeUser);
                var mappings = _Server.UserRoles.GetByRole(cascadeRole);
                return mappings.Count == 0 || !mappings.Exists(m => m.UserGUID == cascadeUser.GUID);
            });

            // Re-add user for role deletion test
            cascadeUser = _Server.Users.Add(new User("cascade_user2"));
            _Server.UserRoles.Add(cascadeUser, cascadeRole);

            // Delete role should remove user-role mappings
            RunTest("Deleting role removes associated user-role mappings", () =>
            {
                _Server.Roles.Remove(cascadeRole);
                var mappings = _Server.UserRoles.GetByUser(cascadeUser);
                return mappings.Count == 0 || !mappings.Exists(m => m.RoleGUID == cascadeRole.GUID);
            });

            // Delete resource should remove permissions
            RunTest("Deleting resource removes associated permissions", () =>
            {
                var permsBefore = _Server.Permissions.GetByResource(cascadeResource);
                _Server.Resources.Remove(cascadeResource);

                // Permission should no longer exist
                return !_Server.Permissions.ExistsByName("cascade_perm");
            });
        }

        private static void RunInputValidationTests()
        {
            Console.WriteLine();
            Console.WriteLine("--- Input Validation Tests ---");

            // Null argument tests
            RunTest("User constructor throws on null name", () =>
            {
                try
                {
                    new User(null);
                    return false;
                }
                catch (ArgumentNullException)
                {
                    return true;
                }
            });

            RunTest("Role constructor throws on null name", () =>
            {
                try
                {
                    new Role(null);
                    return false;
                }
                catch (ArgumentNullException)
                {
                    return true;
                }
            });

            RunTest("Resource constructor throws on null name", () =>
            {
                try
                {
                    new Resource(null);
                    return false;
                }
                catch (ArgumentNullException)
                {
                    return true;
                }
            });

            RunTest("Users.Add throws on null user", () =>
            {
                try
                {
                    _Server.Users.Add(null);
                    return false;
                }
                catch (ArgumentNullException)
                {
                    return true;
                }
            });

            RunTest("Roles.Add throws on null role", () =>
            {
                try
                {
                    _Server.Roles.Add(null);
                    return false;
                }
                catch (ArgumentNullException)
                {
                    return true;
                }
            });

            RunTest("Resources.Add throws on null resource", () =>
            {
                try
                {
                    _Server.Resources.Add(null);
                    return false;
                }
                catch (ArgumentNullException)
                {
                    return true;
                }
            });

            RunTest("Permissions.Add throws on null permission", () =>
            {
                try
                {
                    _Server.Permissions.Add(null);
                    return false;
                }
                catch (ArgumentNullException)
                {
                    return true;
                }
            });

            RunTest("Authorize throws on null username", () =>
            {
                try
                {
                    _Server.Authorize(null, "create", "testresource1");
                    return false;
                }
                catch (ArgumentNullException)
                {
                    return true;
                }
            });

            RunTest("Authorize throws on null operation", () =>
            {
                try
                {
                    _Server.Authorize("testuser1", null, "testresource1");
                    return false;
                }
                catch (ArgumentNullException)
                {
                    return true;
                }
            });

            RunTest("Authorize throws on null resource", () =>
            {
                try
                {
                    _Server.Authorize("testuser1", "create", null);
                    return false;
                }
                catch (ArgumentNullException)
                {
                    return true;
                }
            });

            RunTest("ExistsByName throws on null name (Users)", () =>
            {
                try
                {
                    _Server.Users.ExistsByName(null);
                    return false;
                }
                catch (ArgumentNullException)
                {
                    return true;
                }
            });

            RunTest("GetFirstByName throws on null name (Users)", () =>
            {
                try
                {
                    _Server.Users.GetFirstByName(null);
                    return false;
                }
                catch (ArgumentNullException)
                {
                    return true;
                }
            });

            RunTest("RemoveByName throws on null name (Users)", () =>
            {
                try
                {
                    _Server.Users.RemoveByName(null);
                    return false;
                }
                catch (ArgumentNullException)
                {
                    return true;
                }
            });

            RunTest("UserRoles.Add throws on null user", () =>
            {
                try
                {
                    var role = _Server.Roles.GetFirstByName("testrole1") ?? _Server.Roles.Add(new Role("testrole_val"));
                    _Server.UserRoles.Add(null, role);
                    return false;
                }
                catch (ArgumentNullException)
                {
                    return true;
                }
            });

            RunTest("UserRoles.Add throws on null role", () =>
            {
                try
                {
                    var user = _Server.Users.GetFirstByName("testuser1") ?? _Server.Users.Add(new User("testuser_val"));
                    _Server.UserRoles.Add(user, null);
                    return false;
                }
                catch (ArgumentNullException)
                {
                    return true;
                }
            });
        }

        private static void PrintSummary()
        {
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine(" Test Summary");
            Console.WriteLine("========================================");
            Console.WriteLine($"  Total:  {_PassedTests.Count + _FailedTests.Count}");
            Console.WriteLine($"  Passed: {_PassedTests.Count}");
            Console.WriteLine($"  Failed: {_FailedTests.Count}");
            Console.WriteLine();

            if (_FailedTests.Count > 0)
            {
                Console.WriteLine("Failed Tests:");
                foreach (var test in _FailedTests)
                {
                    Console.WriteLine($"  - {test}");
                }
                Console.WriteLine();
                Console.WriteLine("========================================");
                Console.WriteLine(" OVERALL: FAIL");
                Console.WriteLine("========================================");
            }
            else
            {
                Console.WriteLine("========================================");
                Console.WriteLine(" OVERALL: PASS");
                Console.WriteLine("========================================");
            }
        }
    }
}
