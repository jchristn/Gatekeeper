using System;
using System.Collections.Generic;
using System.Text;
using Watson.ORM.Sqlite;
using Watson.ORM.Core;
using ExpressionTree;

namespace GateKeeper
{
    /// <summary>
    /// Role manager.
    /// </summary>
    public class RoleManager
    {
        #region Public-Members

        #endregion

        #region Internal-Members

        internal PermissionManager Permissions { get; set; } = null;
        internal ResourceManager Resources { get; set; } = null;
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
        public RoleManager(WatsonORM orm)
        {
            _ORM = orm ?? throw new ArgumentNullException(nameof(orm));
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Add.
        /// </summary>
        /// <param name="role">Role.</param>
        /// <returns>Object.</returns>
        public Role Add(Role role)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            if (ExistsByName(role.Name))
                throw new ArgumentException("An item with the same key has already been added.");

            return _ORM.Insert<Role>(role);
        }

        /// <summary>
        /// Remove.
        /// </summary>
        /// <param name="role">Role.</param>
        public void Remove(Role role)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            if (!ExistsByName(role.Name))
                throw new KeyNotFoundException("The specified key was not found.");

            UserRoles.RemoveUserRolesByRole(role);

            _ORM.Delete<Role>(role);
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

            Role r = GetFirstByName(name);
            if (r == null)
                throw new KeyNotFoundException("The specified key was not found.");

            UserRoles.RemoveUserRolesByRole(r);

            _ORM.Delete<Role>(r);
        }

        /// <summary>
        /// Retrieve all.
        /// </summary>
        /// <returns>List.</returns>
        public List<Role> All()
        {
            Expr e = new Expr(_ORM.GetColumnName<Role>(nameof(Role.Id)), OperatorEnum.GreaterThan, 0);
            return _ORM.SelectMany<Role>(e);
        }

        /// <summary>
        /// Retrieve first record by name.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>Object.</returns>
        public Role GetFirstByName(string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            Expr e = new Expr(_ORM.GetColumnName<Role>(nameof(Role.Name)), OperatorEnum.Equals, name);
            Role r = _ORM.SelectFirst<Role>(e);
            return r;
        }

        /// <summary>
        /// Check existence by name.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>True if exists.</returns>
        public bool ExistsByName(string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            Expr e = new Expr(_ORM.GetColumnName<Role>(nameof(Role.Name)), OperatorEnum.Equals, name);
            return _ORM.Exists<Role>(e);
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
