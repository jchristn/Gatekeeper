using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watson.ORM.Core;
using Watson.ORM.Sqlite;

namespace GateKeeper
{
    /// <summary>
    /// GateKeeper roles-based access control server.
    /// </summary>
    public class RbacServer
    {
        #region Public-Members

        /// <summary>
        /// Specify the default response if no matching roles and permissions are found when authorizing a user's request.
        /// </summary>
        public bool DefaultPermit = false;

        #endregion

        #region Private-Members
         
        private WatsonORM _ORM = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="dbFile">Database file.</param>
        public RbacServer(string dbFile)
        {
            if (String.IsNullOrEmpty(dbFile)) throw new ArgumentNullException(nameof(dbFile));
            _ORM = new WatsonORM(new DatabaseSettings(dbFile));
            _ORM.InitializeDatabase();
            _ORM.InitializeTable(typeof(UserRole));
            _ORM.InitializeTable(typeof(RolePermission));
        }
          
        #endregion

        #region Public-Methods

        /// <summary>
        /// Authorize a user's request against a resource by operation type.
        /// </summary>
        /// <param name="username">The name of the user.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="operation">The type of operation.</param>
        /// <returns>True if authorized.</returns>
        public bool Authorize(string username, string resource, string operation)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
            if (String.IsNullOrEmpty(resource)) throw new ArgumentNullException(nameof(resource));
            if (String.IsNullOrEmpty(operation)) throw new ArgumentNullException(nameof(operation));

            DbExpression e1 = new DbExpression(_ORM.GetColumnName<UserRole>(nameof(UserRole.Username)), DbOperators.Equals, username);
            List<UserRole> u = _ORM.SelectMany<UserRole>(e1);
            List<string> r = new List<string>();
            if (u != null && u.Count > 0) foreach (UserRole role in u) r.Add(role.Rolename);

            if (r.Count > 0)
            {
                r = r.Distinct().ToList();
                DbExpression e2 = new DbExpression(_ORM.GetColumnName<RolePermission>(nameof(RolePermission.Rolename)), DbOperators.In, r);
                List<RolePermission> p = _ORM.SelectMany<RolePermission>(e2);

                if (p != null && p.Count > 0)
                {
                    return p.Any(rp => rp.Allow);
                }
            } 

            return DefaultPermit;
        }

        /// <summary>
        /// Add a user to a specified role.
        /// </summary>
        /// <param name="username">The name of the user.</param>
        /// <param name="rolename">The name of the role.</param>
        public void AddUserToRole(string username, string rolename)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
            if (String.IsNullOrEmpty(rolename)) throw new ArgumentNullException(nameof(rolename)); 
            UserRole u = new UserRole(username, rolename);
            _ORM.Insert<UserRole>(u); 
        }

        /// <summary>
        /// Add a role with a specific permission, or add permission to an existing role.
        /// </summary>
        /// <param name="rolename">The name of the role.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="operation">The type of operation.</param>
        /// <param name="allow">Permit (true) or deny (false).</param>
        public void AddRolePermission(string rolename, string resource, string operation, bool allow)
        {
            if (String.IsNullOrEmpty(rolename)) throw new ArgumentNullException(nameof(rolename));
            if (String.IsNullOrEmpty(resource)) throw new ArgumentNullException(nameof(resource));
            if (String.IsNullOrEmpty(operation)) throw new ArgumentNullException(nameof(operation));
            RolePermission r = new RolePermission(rolename, resource, operation, allow);
            _ORM.Insert<RolePermission>(r);
        }

        /// <summary>
        /// Remove a user.
        /// </summary>
        /// <param name="username">The name of the user.</param>
        public void RemoveUser(string username)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username)); 
            DbExpression e = new DbExpression(_ORM.GetColumnName<UserRole>(nameof(UserRole.Username)), DbOperators.Equals, username);
            _ORM.DeleteMany<UserRole>(e);
        }

        /// <summary>
        /// Remove a user's mapping to a role.
        /// </summary>
        /// <param name="username">The name of the user.</param>
        /// <param name="rolename">The name of the role.</param>
        public void RemoveUserRole(string username, string rolename)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
            if (String.IsNullOrEmpty(rolename)) throw new ArgumentNullException(nameof(rolename));
            DbExpression e = new DbExpression(_ORM.GetColumnName<UserRole>(nameof(UserRole.Username)), DbOperators.Equals, username);
            e.PrependAnd(_ORM.GetColumnName<UserRole>(nameof(UserRole.Rolename)), DbOperators.Equals, rolename);
            _ORM.DeleteMany<UserRole>(e);
        }

        /// <summary>
        /// Remove a role.
        /// </summary>
        /// <param name="rolename">The name of the role.</param>
        public void RemoveRole(string rolename)
        {
            if (String.IsNullOrEmpty(rolename)) throw new ArgumentNullException(nameof(rolename));
            DbExpression e1 = new DbExpression(_ORM.GetColumnName<UserRole>(nameof(UserRole.Rolename)), DbOperators.Equals, rolename);
            DbExpression e2 = new DbExpression(_ORM.GetColumnName<RolePermission>(nameof(RolePermission.Rolename)), DbOperators.Equals, rolename);
            _ORM.DeleteMany<UserRole>(e1);
            _ORM.DeleteMany<RolePermission>(e2);
        }

        /// <summary>
        /// Remove a permission entry from a role.
        /// </summary>
        /// <param name="rolename">The name of the role.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="operation">The type of operation.</param>
        public void RemoveRolePermission(string rolename, string resource, string operation)
        {
            if (String.IsNullOrEmpty(rolename)) throw new ArgumentNullException(nameof(rolename));
            if (String.IsNullOrEmpty(resource)) throw new ArgumentNullException(nameof(resource));
            if (String.IsNullOrEmpty(operation)) throw new ArgumentNullException(nameof(operation));
            DbExpression e = new DbExpression(_ORM.GetColumnName<RolePermission>(nameof(RolePermission.Rolename)), DbOperators.Equals, rolename);
            e.PrependAnd(_ORM.GetColumnName<RolePermission>(nameof(RolePermission.Resource)), DbOperators.Equals, resource);
            e.PrependAnd(_ORM.GetColumnName<RolePermission>(nameof(RolePermission.Operation)), DbOperators.Equals, operation);
            _ORM.DeleteMany<RolePermission>(e);
        }

        /// <summary>
        /// Determine if a user exists.
        /// </summary>
        /// <param name="username">The name of the user.</param>
        /// <returns>True if exists.</returns>
        public bool UserExists(string username)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
            DbExpression e = new DbExpression(_ORM.GetColumnName<UserRole>(nameof(UserRole.Username)), DbOperators.Equals, username);
            return _ORM.Exists<UserRole>(e); 
        }

        /// <summary>
        /// Determine if a role exists.
        /// </summary>
        /// <param name="rolename">The name of the role.</param>
        /// <returns>True if exists.</returns>
        public bool RoleExists(string rolename)
        {
            if (String.IsNullOrEmpty(rolename)) throw new ArgumentNullException(nameof(rolename));
            DbExpression e = new DbExpression(_ORM.GetColumnName<RolePermission>(nameof(RolePermission.Rolename)), DbOperators.Equals, rolename);
            return _ORM.Exists<UserRole>(e);
        }

        /// <summary>
        /// Determine if a user is mapped to a role.
        /// </summary>
        /// <param name="username">The name of the user.</param>
        /// <param name="rolename">The name of the role.</param>
        /// <returns>True if the user is mapped to the role.</returns>
        public bool UserRoleExists(string username, string rolename)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
            if (String.IsNullOrEmpty(rolename)) throw new ArgumentNullException(nameof(rolename));
            DbExpression e = new DbExpression(_ORM.GetColumnName<UserRole>(nameof(UserRole.Username)), DbOperators.Equals, username);
            e.PrependAnd(_ORM.GetColumnName<UserRole>(nameof(UserRole.Rolename)), DbOperators.Equals, rolename);
            return _ORM.Exists<UserRole>(e);
        }

        /// <summary>
        /// Determine if a role has permissions defined to access a specified resource using a specific operation.
        /// </summary>
        /// <param name="rolename">The name of the role.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="operation">The type of operation.</param>
        /// <returns>True if the role has permissions defined for the specified resource and specified operation.</returns>
        public bool RolePermissionExists(string rolename, string resource, string operation)
        {
            if (String.IsNullOrEmpty(rolename)) throw new ArgumentNullException(nameof(rolename));
            if (String.IsNullOrEmpty(resource)) throw new ArgumentNullException(nameof(resource));
            if (String.IsNullOrEmpty(operation)) throw new ArgumentNullException(nameof(operation));
            DbExpression e = new DbExpression(_ORM.GetColumnName<RolePermission>(nameof(RolePermission.Rolename)), DbOperators.Equals, rolename);
            e.PrependAnd(_ORM.GetColumnName<RolePermission>(nameof(RolePermission.Resource)), DbOperators.Equals, resource);
            e.PrependAnd(_ORM.GetColumnName<RolePermission>(nameof(RolePermission.Operation)), DbOperators.Equals, operation);
            return _ORM.Exists<RolePermission>(e);
        }

        /// <summary>
        /// Retrieve the list of role names to which the user is assigned.
        /// </summary>
        /// <param name="username">The name of the user.</param>
        /// <returns>List of role names.</returns>
        public List<string> GetUserRoles(string username)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
            DbExpression e = new DbExpression(_ORM.GetColumnName<UserRole>(nameof(UserRole.Id)), DbOperators.GreaterThan, 0);
            e.PrependAnd(_ORM.GetColumnName<UserRole>(nameof(UserRole.Username)), DbOperators.Equals, username);
            List<UserRole> u = _ORM.SelectMany<UserRole>(e);
            List<string> ret = new List<string>();
            if (u.Count > 0)
            {
                foreach (UserRole r in u) ret.Add(r.Rolename);
                if (ret.Count > 0) ret = ret.Distinct().ToList();
            }
            return ret;
        }

        /// <summary>
        /// Retrieve the list of permissions assigned to a role.
        /// </summary>
        /// <param name="rolename">The name of the role.</param>
        /// <returns>List of role permissions.</returns>
        public List<RolePermission> GetRolePermissions(string rolename)
        {
            if (String.IsNullOrEmpty(rolename)) throw new ArgumentNullException(nameof(rolename));

            DbExpression e = new DbExpression(_ORM.GetColumnName<RolePermission>(nameof(RolePermission.Rolename)), DbOperators.Equals, rolename);
            List<RolePermission> rp = _ORM.SelectMany<RolePermission>(e);
            if (rp != null && rp.Count > 0) rp = rp.Distinct().ToList();
            return rp;
        }

        #endregion

        #region Private-Methods
          
        #endregion
    }
}
