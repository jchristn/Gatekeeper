using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Watson.ORM.Core;
using Watson.ORM.Sqlite;
using DatabaseWrapper.Core;

namespace GateKeeper
{
    /// <summary>
    /// GateKeeper roles-based access control server.
    /// </summary>
    public class RbacServer
    {
        #region Public-Members

        /// <summary>
        /// GateKeeper version.
        /// </summary>
        public string Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        /// <summary>
        /// Specify the default response if no matching roles and permissions are found when authorizing a user's request.
        /// </summary>
        public bool DefaultPermit = false;

        /// <summary>
        /// Permissions.
        /// </summary>
        public PermissionManager Permissions
        {
            get
            {
                return _Permissions;
            }
        }

        /// <summary>
        /// Resources.
        /// </summary>
        public ResourceManager Resources
        {
            get
            {
                return _Resources;
            }
        }

        /// <summary>
        /// Roles.
        /// </summary>
        public RoleManager Roles
        {
            get
            {
                return _Roles;
            }
        }

        /// <summary>
        /// Users.
        /// </summary>
        public UserManager Users
        {
            get
            {
                return _Users;
            }
        }

        /// <summary>
        /// User to role mappings.
        /// </summary>
        public UserRoleManager UserRoles
        {
            get
            {
                return _UserRoles;
            }
        }

        /// <summary>
        /// Event to fire when authorization decisions have been processed.
        /// </summary>
        public EventHandler<AuthorizationEventArgs> AuthorizationEvent;

        #endregion

        #region Private-Members
         
        private WatsonORM _ORM = null;
        private PermissionManager _Permissions = null;
        private ResourceManager _Resources = null;
        private RoleManager _Roles = null;
        private UserManager _Users = null;
        private UserRoleManager _UserRoles = null;

        private List<Type> _TypesToInitialize = new List<Type>
        {
            typeof(Permission),
            typeof(Resource),
            typeof(Role),
            typeof(User),
            typeof(UserRole)
        };

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="dbFile">Database file.</param>
        public RbacServer(string dbFile = "gatekeeper.db")
        {
            if (String.IsNullOrEmpty(dbFile)) throw new ArgumentNullException(nameof(dbFile));
            _ORM = new WatsonORM(new DatabaseSettings(dbFile));
            _ORM.InitializeDatabase();
            _ORM.InitializeTables(_TypesToInitialize);

            _Permissions = new PermissionManager(_ORM);
            _Resources = new ResourceManager(_ORM);
            _Roles = new RoleManager(_ORM);
            _Users = new UserManager(_ORM);
            _UserRoles = new UserRoleManager(_ORM, _Users, _Roles);

            _Permissions.Resources = _Resources;
            _Permissions.Roles = _Roles;
            _Permissions.Users = _Users;
            _Permissions.UserRoles = _UserRoles;

            _Resources.Permissions = _Permissions;
            _Resources.Roles = _Roles;
            _Resources.Users = _Users;
            _Resources.UserRoles = _UserRoles;

            _Roles.Permissions = _Permissions;
            _Roles.Resources = _Resources;
            _Roles.Users = _Users;
            _Roles.UserRoles = _UserRoles;

            _Users.Permissions = _Permissions;
            _Users.Resources = _Resources;
            _Users.Roles = _Roles;
            _Users.UserRoles = _UserRoles;

            _UserRoles.Permissions = _Permissions;
            _UserRoles.Resources = _Resources;
        }
          
        #endregion

        #region Public-Methods

        /// <summary>
        /// Authorize a user's request against a resource by operation type.
        /// </summary>
        /// <param name="username">The name of the user.</param>
        /// <param name="operation">The type of operation.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="metadata">Request metadata, included in events.</param>
        /// <returns>True if authorized.</returns>
        public bool Authorize(string username, string operation, string resource, object metadata = null)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
            if (String.IsNullOrEmpty(resource)) throw new ArgumentNullException(nameof(resource));
            if (String.IsNullOrEmpty(operation)) throw new ArgumentNullException(nameof(operation));

            bool authorized = false;
            DataTable table = null;
            List<MatchingEntry> matchingEntries = null;

            try
            {
                string query = AuthorizeQuery(username, operation, resource);
                Console.WriteLine(query);
                table = _ORM.Query(query);
                matchingEntries = MatchingEntry.FromDataTable(table);

                if (matchingEntries != null && matchingEntries.Count > 0)
                {
                    if (matchingEntries.Any(e => e.Allow)) authorized = true;
                }
                else
                {
                    authorized = DefaultPermit;
                }
            }
            finally
            {
                Task unawaited = Task.Run(() => 
                    AuthorizationEvent?.Invoke(this, new AuthorizationEventArgs(username, resource, operation, authorized, matchingEntries, metadata))
                );
            }

            return authorized;
        }

        #endregion

        #region Private-Methods

        private string AuthorizeQuery(string username, string operation, string resource)
        {
            username = Helpers.Sanitize(username);
            operation = Helpers.Sanitize(operation);
            resource = Helpers.Sanitize(resource);

            if (String.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
            if (String.IsNullOrEmpty(operation)) throw new ArgumentNullException(nameof(operation));
            if (String.IsNullOrWhiteSpace(resource)) throw new ArgumentNullException(nameof(resource));

            return
                "SELECT " + Environment.NewLine +
                "  resources.guid AS resourceguid, " + Environment.NewLine +
                "  resources.name AS resourcename, " + Environment.NewLine +
                "  users.guid AS userguid, " + Environment.NewLine +
                "  users.name AS username, " + Environment.NewLine +
                "  roles.guid AS roleguid, " + Environment.NewLine +
                "  roles.name AS rolename, " + Environment.NewLine +
                "  userroles.guid AS userroleguid, " + Environment.NewLine +
                "  permissions.guid AS permissionguid, " + Environment.NewLine +
                "  permissions.name AS permissionname, " + Environment.NewLine +
                "  permissions.operation AS operation, " + Environment.NewLine +
                "  permissions.allow AS allow " + Environment.NewLine +
                "FROM resources " + Environment.NewLine +
                "INNER JOIN permissions ON permissions.resourceguid = resources.guid " + Environment.NewLine +
                "INNER JOIN userroles ON userroles.roleguid = permissions.roleguid " + Environment.NewLine +
                "INNER JOIN roles ON roles.guid = permissions.roleguid " + Environment.NewLine +
                "INNER JOIN users ON users.guid = userroles.userguid " + Environment.NewLine +
                "WHERE " + Environment.NewLine +
                "  resources.name = '" + resource + "' " + Environment.NewLine +
                "  AND users.name = '" + username + "' " + Environment.NewLine +
                "  AND permissions.operation = '" + operation + "' " + Environment.NewLine;
        }

        #endregion
    }
}
