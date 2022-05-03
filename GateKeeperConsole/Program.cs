using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GateKeeper;

namespace GatekeeperConsole
{
    class Program
    {
        static string _Filename = "gatekeeper.db";
        static RbacServer _Server = null;
        static bool _EnableEvents = true;

        static void Main(string[] args)
        {
            if (args != null && args.Length == 1) _Filename = args[0];
            _Server = new RbacServer(_Filename);
            _Server.AuthorizationEvent += AuthorizationEvent;
            _Server.DefaultPermit = false;

            Welcome();
            InitializeDatabase();
            
            bool runForever = true;
            while (runForever)
            {
                string userInput = Common.InputString("Command [? for help]:", null, false);

                if (userInput.StartsWith("user "))
                {
                    switch (userInput)
                    {
                        case "user all":
                            UserAll();
                            break;
                        case "user get":
                            UserGet();
                            break;
                        case "user add":
                            UserAdd();
                            break;
                        case "user del":
                            UserDelete();
                            break;
                        case "user exists":
                            UserExists();
                            break;
                    }
                }
                else if (userInput.StartsWith("role "))
                {
                    switch (userInput)
                    {
                        case "role all":
                            RoleAll();
                            break;
                        case "role get":
                            RoleGet();
                            break;
                        case "role add":
                            RoleAdd();
                            break;
                        case "role del":
                            RoleDelete();
                            break;
                        case "role exists":
                            RoleExists();
                            break;
                    }
                }
                else if (userInput.StartsWith("perm "))
                {
                    switch (userInput)
                    {
                        case "perm all":
                            PermAll();
                            break;
                        case "perm get":
                            PermGet();
                            break;
                        case "perm add":
                            PermAdd();
                            break;
                        case "perm del":
                            PermDelete();
                            break;
                        case "perm exists":
                            PermExists();
                            break;
                    }
                }
                else if (userInput.StartsWith("res "))
                {
                    switch (userInput)
                    {
                        case "res all":
                            ResourceAll();
                            break;
                        case "res get":
                            ResourceGet();
                            break;
                        case "res add":
                            ResourceAdd();
                            break;
                        case "res del":
                            ResourceDelete();
                            break;
                        case "res exists":
                            ResourceExists();
                            break;
                    }
                }
                else if (userInput.StartsWith("map "))
                {
                    switch (userInput)
                    {
                        case "map all":
                            MapAll();
                            break;
                        case "map getbyuser":
                            MapGetByUser();
                            break;
                        case "map getbyrole":
                            MapGetByRole();
                            break;
                        case "map add":
                            MapAdd();
                            break;
                        case "map del":
                            MapDelete();
                            break;
                        case "map exists":
                            MapExists();
                            break;
                    }
                }
                else
                {
                    switch (userInput)
                    {
                        case "?":
                            Menu();
                            break;

                        case "q":
                            runForever = false;
                            break;

                        case "cls":
                            Console.Clear();
                            break;

                        case "authorize":
                            Authorize();
                            break;

                        case "events":
                            _EnableEvents = !_EnableEvents;
                            Console.WriteLine("Events enabled: " + _EnableEvents);
                            break;
                    }
                }
            }
        }

        private static void Welcome()
        {
            Console.WriteLine(GateKeeper.Constants.Logo);
            Console.WriteLine(" GateKeeper console v" + _Server.Version);
            Console.WriteLine(" Using database file: " + _Filename);
            Console.WriteLine("");
        }

        private static void AuthorizationEvent(object sender, AuthorizationEventArgs e)
        {
            if (_EnableEvents)
            {
                Console.WriteLine(Common.SerializeJson(e, true));
            }
        }

        private static void InitializeDatabase()
        {
            bool initDb = Common.InputBoolean("Initialize database with sample records", true);

            if (!initDb) return;

            Resource r1 = null;
            Resource r2 = null;
            User u1 = null;
            User u2 = null;
            Role role1 = null;
            Role role2 = null;
            Permission p1 = null;
            Permission p2 = null;
            UserRole ur1 = null;
            UserRole ur2 = null;

            if (!_Server.Resources.ExistsByName("resource1"))
            {
                r1 = new Resource("resource1");
                r1 = _Server.Resources.Add(r1);
            }

            if (!_Server.Resources.ExistsByName("resource2"))
            {
                r2 = new Resource("resource2");
                r2 = _Server.Resources.Add(new Resource("resource2"));
            }

            if (!_Server.Users.ExistsByName("user1"))
            {
                u1 = new User("user1");
                u1 = _Server.Users.Add(u1);
            }

            if (!_Server.Users.ExistsByName("user2"))
            {
                u2 = new User("user2");
                u2 = _Server.Users.Add(u2);
            }

            if (!_Server.Roles.ExistsByName("role1"))
            {
                role1 = new Role("role1");
                role1 = _Server.Roles.Add(role1);
            }

            if (!_Server.Roles.ExistsByName("role2"))
            {
                role2 = new Role("role2");
                role2 = _Server.Roles.Add(role2);
            }

            if (!_Server.Permissions.ExistsByName("perm1"))
            {
                p1 = new Permission("perm1", role1, r1, "create", true);
                p1 = _Server.Permissions.Add(p1);
            }

            if (!_Server.Permissions.ExistsByName("perm2"))
            {
                p2 = new Permission("perm2", role2, r2, "delete", true);
                p2 = _Server.Permissions.Add(p2);
            }

            if (!_Server.UserRoles.Exists(u1, role1))
            {
                ur1 = _Server.UserRoles.Add(u1, role1);
            }

            if (!_Server.UserRoles.Exists(u2, role2))
            {
                ur2 = _Server.UserRoles.Add(u2, role2);
            }
        }

        private static void Menu()
        {
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine("Command groups:");
            Console.WriteLine("   user <cmd>        Issue a user command");
            Console.WriteLine("   role <cmd>        Issue a role command");
            Console.WriteLine("   perm <cmd>        Issue a permission command");
            Console.WriteLine("   res <cmd>         Issue a resource command");
            Console.WriteLine("   map <cmd>         Issue a user-role map command");
            Console.WriteLine("   authorize         Attempt to authorize a request");
            Console.WriteLine("   events            Toggle events (currently " + _EnableEvents + ")");
            Console.WriteLine("");
            Console.WriteLine("Available commands for <cmd>:");
            Console.WriteLine("   all               Retrieve all records");
            Console.WriteLine("   get               Retrieve record by name");
            Console.WriteLine("   add               Create a record");
            Console.WriteLine("   del               Delete record by name");
            Console.WriteLine("   exists            Check if a record exists by name");
            Console.WriteLine("");
            Console.WriteLine("For map commands, get is replaced with getbyuser and getbyrole");
            Console.WriteLine("");
        }

        private static void Authorize()
        {
            string username =   Common.InputString("Username  :", null, true);
            if (String.IsNullOrEmpty(username)) return;

            string operation =  Common.InputString("Operation :", null, true);
            if (String.IsNullOrEmpty(operation)) return;

            string resource =   Common.InputString("Resource  :", null, true);
            if (String.IsNullOrEmpty(resource)) return;

            string metadata =   Common.InputString("Metadata  :", null, true);

            bool authorized = _Server.Authorize(username, operation, resource, metadata);
            Console.WriteLine("Authorized: " + authorized);
        }

        private static void UserAll()
        {
            List<User> objs = _Server.Users.All();
            if (objs != null && objs.Count > 0)
            {
                Console.WriteLine(Common.SerializeJson(objs, true));
                Console.WriteLine(objs.Count + " record(s)");
            }
            else
            {
                Console.WriteLine("None");
            }
        }

        private static void UserGet()
        {
            string name = Common.InputString("Name:", null, true);
            if (String.IsNullOrEmpty(name)) return;
            User obj = _Server.Users.GetFirstByName(name);
            if (obj != null)
            {
                Console.WriteLine(Common.SerializeJson(obj, true));
            }
            else
            {
                Console.WriteLine("Not found");
            }
        }

        private static void UserAdd()
        {
            string name = Common.InputString("Name:", null, true);
            if (String.IsNullOrEmpty(name)) return;
            User obj = new User(name);
            obj = _Server.Users.Add(obj);
            Console.WriteLine(Common.SerializeJson(obj, true));
        }

        private static void UserDelete()
        {
            string name = Common.InputString("Name:", null, true);
            if (String.IsNullOrEmpty(name)) return;
            _Server.Users.RemoveByName(name);
        }

        private static void UserExists()
        {
            string name = Common.InputString("Name:", null, true);
            if (String.IsNullOrEmpty(name)) return;
            Console.WriteLine("Exists: " + _Server.Users.ExistsByName(name));
        }

        private static void ResourceAll()
        {
            List<Resource> objs = _Server.Resources.All();
            if (objs != null && objs.Count > 0)
            {
                Console.WriteLine(Common.SerializeJson(objs, true));
                Console.WriteLine(objs.Count + " record(s)");
            }
            else
            {
                Console.WriteLine("None");
            }
        }

        private static void ResourceGet()
        {
            string name = Common.InputString("Name:", null, true);
            if (String.IsNullOrEmpty(name)) return;
            Resource obj = _Server.Resources.GetFirstByName(name);
            if (obj != null)
            {
                Console.WriteLine(Common.SerializeJson(obj, true));
            }
            else
            {
                Console.WriteLine("Not found");
            }
        }

        private static void ResourceAdd()
        {
            string name = Common.InputString("Name:", null, true);
            if (String.IsNullOrEmpty(name)) return;
            Resource obj = new Resource(name);
            obj = _Server.Resources.Add(obj);
            Console.WriteLine(Common.SerializeJson(obj, true));
        }

        private static void ResourceDelete()
        {
            string name = Common.InputString("Name:", null, true);
            if (String.IsNullOrEmpty(name)) return;
            _Server.Resources.RemoveByName(name);
        }

        private static void ResourceExists()
        {
            string name = Common.InputString("Name:", null, true);
            if (String.IsNullOrEmpty(name)) return;
            Console.WriteLine("Exists: " + _Server.Resources.ExistsByName(name));
        }

        private static void RoleAll()
        {
            List<Role> objs = _Server.Roles.All();
            if (objs != null && objs.Count > 0)
            {
                Console.WriteLine(Common.SerializeJson(objs, true));
                Console.WriteLine(objs.Count + " record(s)");
            }
            else
            {
                Console.WriteLine("None");
            }
        }

        private static void RoleGet()
        {
            string name = Common.InputString("Name:", null, true);
            if (String.IsNullOrEmpty(name)) return;
            Role obj = _Server.Roles.GetFirstByName(name);
            if (obj != null)
            {
                Console.WriteLine(Common.SerializeJson(obj, true));
            }
            else
            {
                Console.WriteLine("Not found");
            }
        }

        private static void RoleAdd()
        {
            string name = Common.InputString("Name:", null, true);
            if (String.IsNullOrEmpty(name)) return;
            Role obj = new Role(name);
            obj = _Server.Roles.Add(obj);
            Console.WriteLine(Common.SerializeJson(obj, true));
        }

        private static void RoleDelete()
        {
            string name = Common.InputString("Name:", null, true);
            if (String.IsNullOrEmpty(name)) return;
            _Server.Roles.RemoveByName(name);
        }

        private static void RoleExists()
        {
            string name = Common.InputString("Name:", null, true);
            if (String.IsNullOrEmpty(name)) return;
            Console.WriteLine("Exists: " + _Server.Roles.ExistsByName(name));
        }

        private static void PermAll()
        {
            List<Permission> objs = _Server.Permissions.All();
            if (objs != null && objs.Count > 0)
            {
                Console.WriteLine(Common.SerializeJson(objs, true));
                Console.WriteLine(objs.Count + " record(s)");
            }
            else
            {
                Console.WriteLine("None");
            }
        }

        private static void PermGet()
        {
            string name = Common.InputString("Name:", null, true);
            if (String.IsNullOrEmpty(name)) return;
            Permission obj = _Server.Permissions.GetFirstByName(name);
            if (obj != null)
            {
                Console.WriteLine(Common.SerializeJson(obj, true));
            }
            else
            {
                Console.WriteLine("Not found");
            }
        }

        private static void PermAdd()
        {
            string name =         Common.InputString("Name          :", null, true);
            if (String.IsNullOrEmpty(name)) return;

            string roleName =     Common.InputString("Role name     :", null, true);
            if (String.IsNullOrEmpty(roleName)) return;

            Role role = _Server.Roles.GetFirstByName(roleName);
            if (role == null)
            {
                Console.WriteLine("Role not found");
                return;
            }

            string resourceName = Common.InputString("Resource name :", null, true);
            if (String.IsNullOrEmpty(resourceName)) return;

            Resource resource = _Server.Resources.GetFirstByName(resourceName);
            if (resource == null)
            {
                Console.WriteLine("Resource not found");
                return;
            }

            string operation =    Common.InputString("Operation     :", null, true);
            if (String.IsNullOrEmpty(operation)) return;

            bool allow = Common.InputBoolean("Allow         :", true);

            Permission obj = new Permission(name, role, resource, operation, allow);
            obj = _Server.Permissions.Add(obj);
            Console.WriteLine(Common.SerializeJson(obj, true));
        }

        private static void PermDelete()
        {
            string name = Common.InputString("Name:", null, true);
            if (String.IsNullOrEmpty(name)) return;
            _Server.Permissions.RemoveByName(name);
        }

        private static void PermExists()
        {
            string name = Common.InputString("Name:", null, true);
            if (String.IsNullOrEmpty(name)) return;
            Console.WriteLine("Exists: " + _Server.Permissions.ExistsByName(name));
        }

        private static void MapAll()
        {
            List<UserRole> objs = _Server.UserRoles.All();
            if (objs != null && objs.Count > 0)
            {
                Console.WriteLine(Common.SerializeJson(objs, true));
                Console.WriteLine(objs.Count + " record(s)");
            }
            else
            {
                Console.WriteLine("None");
            }
        }

        private static void MapGetByUser()
        {
            string name = Common.InputString("Name:", null, true);
            if (String.IsNullOrEmpty(name)) return;

            User user = _Server.Users.GetFirstByName(name);
            if (user == null)
            {
                Console.WriteLine("User not found");
                return;
            }

            List<UserRole> objs = _Server.UserRoles.GetByUser(user);
            if (objs != null && objs.Count > 0)
            {
                Console.WriteLine(Common.SerializeJson(objs, true));
                Console.WriteLine(objs.Count + " record(s)");
            }
            else
            {
                Console.WriteLine("Not found");
            }
        }

        private static void MapGetByRole()
        {
            string name = Common.InputString("Name:", null, true);
            if (String.IsNullOrEmpty(name)) return;

            Role role = _Server.Roles.GetFirstByName(name);
            if (role == null)
            {
                Console.WriteLine("Role not found");
                return;
            }

            List<UserRole> objs = _Server.UserRoles.GetByRole(role);
            if (objs != null && objs.Count > 0)
            {
                Console.WriteLine(Common.SerializeJson(objs, true));
                Console.WriteLine(objs.Count + " record(s)");
            }
            else
            {
                Console.WriteLine("Not found");
            }
        }

        private static void MapAdd()
        {
            string userName = Common.InputString("User name     :", null, true);
            if (String.IsNullOrEmpty(userName)) return;

            User user = _Server.Users.GetFirstByName(userName);
            if (user == null)
            {
                Console.WriteLine("User not found");
                return;
            }

            string roleName = Common.InputString("Role name     :", null, true);
            if (String.IsNullOrEmpty(roleName)) return;

            Role role = _Server.Roles.GetFirstByName(roleName);
            if (role == null)
            {
                Console.WriteLine("Role not found");
                return;
            }

            UserRole obj = _Server.UserRoles.Add(user, role);
            Console.WriteLine(Common.SerializeJson(obj, true));
        }

        private static void MapDelete()
        {
            string userName = Common.InputString("User name:", null, true);
            if (String.IsNullOrEmpty(userName)) return;
            string roleName = Common.InputString("Role name:", null, true);
            if (String.IsNullOrEmpty(roleName)) return;

            User user = _Server.Users.GetFirstByName(userName);
            Role role = _Server.Roles.GetFirstByName(roleName);
            if (user == null)
            {
                Console.WriteLine("User not found");
                return;
            }
            if (role == null)
            {
                Console.WriteLine("Role not found");
                return;
            }

            _Server.UserRoles.Remove(user, role);
        }

        private static void MapExists()
        {
            string userName = Common.InputString("User name:", null, true);
            if (String.IsNullOrEmpty(userName)) return;
            string roleName = Common.InputString("Role name:", null, true);
            if (String.IsNullOrEmpty(roleName)) return;

            User user = _Server.Users.GetFirstByName(userName);
            Role role = _Server.Roles.GetFirstByName(roleName);
            if (user == null)
            {
                Console.WriteLine("User not found");
                return;
            }
            if (role == null)
            {
                Console.WriteLine("Role not found");
                return;
            }

            Console.WriteLine("Exists: " + _Server.UserRoles.Exists(user, role));
        }
    }
}
