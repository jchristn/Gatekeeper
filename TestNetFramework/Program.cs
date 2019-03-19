using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateKeeper
{
    class Program
    {
        static void Main(string[] args)
        {
            RbacServer server = new RbacServer("rbac.db");

            server.AddRolePermission("user", "user", "get", true);
            server.AddRolePermission("user", "transaction", "get", false);
            server.AddRolePermission("user", "location", "get", false);

            server.AddRolePermission("admin", "user", "get", true);
            server.AddRolePermission("admin", "transaction", "get", true);
            server.AddRolePermission("admin", "location", "get", true);

            server.AddUserToRole("user", "user");
            server.AddUserToRole("admin", "admin");

            server.DefaultPermit = false;

            bool runForever = true; 
            while (runForever)
            {
                Console.Write("Command [? for help] > ");
                string userInput = Console.ReadLine();
                if (String.IsNullOrEmpty(userInput)) continue;

                switch (userInput)
                {
                    case "?":
                        Console.WriteLine("---");
                        Console.WriteLine("  ?  q  cls");
                        Console.WriteLine("  auth");
                        Console.WriteLine("  add_user_role  del_user_role  user_exists");
                        Console.WriteLine("  del_role  get_user_roles  get_role_perms");
                        Console.WriteLine("  add_role_perm  del_role_perm  role_perm_exists");
                        break;

                    case "q":
                        runForever = false;
                        break;

                    case "cls":
                        Console.Clear();
                        break;

                    case "auth":
                        if (server.Authorize(
                            InputString("User", null, false),
                            InputString("Resource", null, false),
                            InputString("Operation", null, false)))
                        {
                            Console.WriteLine("Authorized");
                        }
                        else
                        {
                            Console.WriteLine("Declined");
                        }
                        break;

                    case "add_user_role":
                        server.AddUserToRole(
                            InputString("User", null, false),
                            InputString("Role", null, false));
                        break;

                    case "del_user_role":
                        server.RemoveUserRole(
                            InputString("User", null, false),
                            InputString("Role", null, false));
                        break;

                    case "del_role":
                        server.RemoveRole(InputString("Role", null, false));
                        break;

                    case "get_user_roles":
                        List<string> roles = server.GetUserRoles(InputString("User", null, false));
                        if (roles != null && roles.Count > 0)
                        {
                            foreach (string currRole in roles) Console.WriteLine("  " + currRole);
                        }
                        break;

                    case "get_role_perms":
                        List<RolePermission> perms = server.GetRolePermissions(InputString("Role", null, false));
                        if (perms != null && perms.Count > 0)
                        {
                            foreach (RolePermission currPerm in perms)
                            {
                                Console.WriteLine("  " + currPerm.RoleName + " " + currPerm.Operation + " " + currPerm.Resource + ": " + currPerm.Allow);
                            }
                        }
                        break;

                    case "user_exists":
                        if (server.UserExists(InputString("User", null, false)))
                        {
                            Console.WriteLine("Exists");
                        }
                        else
                        {
                            Console.WriteLine("Doesn't exist");
                        }
                        break;

                    case "add_role_perm":
                        server.AddRolePermission(
                            InputString("Role", null, false),
                            InputString("Resource", null, false),
                            InputString("Operation", null, false),
                            InputBoolean("Allow", true));
                        break;

                    case "del_role_perm":
                        server.RemoveRolePermission(
                            InputString("Role", null, false),
                            InputString("Resource", null, false),
                            InputString("Operation", null, false));
                        break;

                    case "role_perm_exists":
                        if (server.RolePermissionExists(
                            InputString("Role", null, false),
                            InputString("Resource", null, false),
                            InputString("Operation", null, false)))
                        {
                            Console.WriteLine("Exists");
                        }
                        else
                        {
                            Console.WriteLine("Doesn't exist");
                        }
                        break;
                }
            }
        }

        public static string InputString(string question, string defaultAnswer, bool allowNull)
        {
            while (true)
            {
                Console.Write(question);

                if (!String.IsNullOrEmpty(defaultAnswer))
                {
                    Console.Write(" [" + defaultAnswer + "]");
                }

                Console.Write(" ");

                string userInput = Console.ReadLine();

                if (String.IsNullOrEmpty(userInput))
                {
                    if (!String.IsNullOrEmpty(defaultAnswer)) return defaultAnswer;
                    if (allowNull) return null;
                    else continue;
                }

                return userInput;
            }
        }

        public static int InputInteger(string question, int defaultAnswer, bool positiveOnly, bool allowZero)
        {
            while (true)
            {
                Console.Write(question);
                Console.Write(" [" + defaultAnswer + "] ");

                string userInput = Console.ReadLine();

                if (String.IsNullOrEmpty(userInput))
                {
                    return defaultAnswer;
                }

                int ret = 0;
                if (!Int32.TryParse(userInput, out ret))
                {
                    Console.WriteLine("Please enter a valid integer.");
                    continue;
                }

                if (ret == 0)
                {
                    if (allowZero)
                    {
                        return 0;
                    }
                }

                if (ret < 0)
                {
                    if (positiveOnly)
                    {
                        Console.WriteLine("Please enter a value greater than zero.");
                        continue;
                    }
                }

                return ret;
            }
        }
         
        public static bool InputBoolean(string question, bool yesDefault)
        {
            Console.Write(question);

            if (yesDefault) Console.Write(" [Y/n]? ");
            else Console.Write(" [y/N]? ");

            string userInput = Console.ReadLine();

            if (String.IsNullOrEmpty(userInput))
            {
                if (yesDefault) return true;
                return false;
            }

            userInput = userInput.ToLower();

            if (yesDefault)
            {
                if (
                    (String.Compare(userInput, "n") == 0)
                    || (String.Compare(userInput, "no") == 0)
                   )
                {
                    return false;
                }

                return true;
            }
            else
            {
                if (
                    (String.Compare(userInput, "y") == 0)
                    || (String.Compare(userInput, "yes") == 0)
                   )
                {
                    return true;
                }

                return false;
            }
        }

    }
}
