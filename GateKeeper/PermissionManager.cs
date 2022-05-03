using System;
using System.Collections.Generic;
using System.Text;
using Watson.ORM.Sqlite;
using Watson.ORM.Core;
using ExpressionTree;

namespace GateKeeper
{
    /// <summary>
    /// Permission manager.
    /// </summary>
    public class PermissionManager
    {
        #region Public-Members

        #endregion

        #region Internal-Members

        internal ResourceManager Resources { get; set; } = null;
        internal RoleManager Roles { get; set; } = null;
        internal UserManager Users { get; set; } = null;
        internal UserRoleManager UserRoles { get; set; } = null;

        #endregion

        #region Private-Members

        private WatsonORM _ORM = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="orm">ORM.</param>
        public PermissionManager(WatsonORM orm)
        {
            _ORM = orm ?? throw new ArgumentNullException(nameof(orm));
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Add.
        /// </summary>
        /// <param name="permission">Permission.</param>
        /// <returns>Object.</returns>
        public Permission Add(Permission permission)
        {
            if (permission == null) throw new ArgumentNullException(nameof(permission));

            if (ExistsByName(permission.Name))
                throw new ArgumentException("An item with the same key has already been added.");

            return _ORM.Insert<Permission>(permission);
        }

        /// <summary>
        /// Remove.
        /// </summary>
        /// <param name="permission">Permission.</param>
        public void Remove(Permission permission)
        {
            if (permission == null) throw new ArgumentNullException(nameof(permission));

            if (!ExistsByName(permission.Name))
                throw new KeyNotFoundException("The specified key was not found.");

            _ORM.Delete<Permission>(permission);
        }

        /// <summary>
        /// Remove all permissions for a given resource.
        /// </summary>
        /// <param name="resource">Resource.</param>
        public void RemoveResourcePermissions(Resource resource)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            Expr e = new Expr(_ORM.GetColumnName<Permission>(nameof(Permission.ResourceGUID)), OperatorEnum.Equals, resource.GUID);
            _ORM.DeleteMany<Permission>(e);
        }

        /// <summary>
        /// Remove by name.
        /// </summary>
        /// <param name="name">Name.</param>
        public void RemoveByName(string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            if (!ExistsByName(name))
                throw new KeyNotFoundException("The specified key was not found.");

            Permission p = GetFirstByName(name);
            if (p == null)
                throw new KeyNotFoundException("The specified key was not found.");

            _ORM.Delete<Permission>(p);
        }

        /// <summary>
        /// Retrieve all.
        /// </summary>
        /// <returns>List.</returns>
        public List<Permission> All()
        {
            Expr e = new Expr(_ORM.GetColumnName<Permission>(nameof(Permission.Id)), OperatorEnum.GreaterThan, 0);
            return _ORM.SelectMany<Permission>(e);
        }

        /// <summary>
        /// Retrieve first record by name.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>Object.</returns>
        public Permission GetFirstByName(string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            Expr e = new Expr(_ORM.GetColumnName<Permission>(nameof(Permission.Name)), OperatorEnum.Equals, name);
            Permission p = _ORM.SelectFirst<Permission>(e);
            return p;
        }

        /// <summary>
        /// Retrieve permissions associated with a resource.
        /// </summary>
        /// <param name="resource">Resource.</param>
        /// <returns>List.</returns>
        public List<Permission> GetByResource(Resource resource)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            Expr e = new Expr(_ORM.GetColumnName<Permission>(nameof(Permission.ResourceGUID)), OperatorEnum.Equals, resource.GUID);
            return _ORM.SelectMany<Permission>(e);
        }

        /// <summary>
        /// Check existence by name.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>True if exists.</returns>
        public bool ExistsByName(string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            Expr e = new Expr(_ORM.GetColumnName<Permission>(nameof(Permission.Name)), OperatorEnum.Equals, name);
            return _ORM.Exists<Permission>(e);
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
