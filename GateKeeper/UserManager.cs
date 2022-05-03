using System;
using System.Collections.Generic;
using System.Text;
using Watson.ORM.Sqlite;
using Watson.ORM.Core;
using ExpressionTree;

namespace GateKeeper
{
    /// <summary>
    /// User manager.
    /// </summary>
    public class UserManager
    {
        #region Public-Members

        #endregion

        #region Internal-Members

        internal PermissionManager Permissions { get; set; } = null;
        internal ResourceManager Resources { get; set; } = null;
        internal RoleManager Roles { get; set; } = null;
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
        public UserManager(WatsonORM orm)
        {
            _ORM = orm ?? throw new ArgumentNullException(nameof(orm));
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Add.
        /// </summary>
        /// <param name="user">User.</param>
        /// <returns>Object.</returns>
        public User Add(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            if (ExistsByName(user.Name))
                throw new ArgumentException("An item with the same key has already been added.");

            return _ORM.Insert<User>(user);
        }

        /// <summary>
        /// Remove.
        /// </summary>
        /// <param name="user">User.</param>
        public void Remove(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            if (!ExistsByName(user.Name))
                throw new KeyNotFoundException("The specified key was not found.");

            UserRoles.RemoveUserRolesByUser(user);

            _ORM.Delete<User>(user);
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

            User u = GetFirstByName(name);
            if (u == null)
                throw new KeyNotFoundException("The specified key was not found.");

            UserRoles.RemoveUserRolesByUser(u);

            _ORM.Delete<User>(u);
        }

        /// <summary>
        /// Retrieve all.
        /// </summary>
        /// <returns>List.</returns>
        public List<User> All()
        {
            Expr e = new Expr(_ORM.GetColumnName<User>(nameof(User.Id)), OperatorEnum.GreaterThan, 0);
            return _ORM.SelectMany<User>(e);
        }

        /// <summary>
        /// Retrieve first record by name.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>Object.</returns>
        public User GetFirstByName(string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            Expr e = new Expr(_ORM.GetColumnName<User>(nameof(User.Name)), OperatorEnum.Equals, name);
            User u = _ORM.SelectFirst<User>(e);
            return u;
        }

        /// <summary>
        /// Check existence by name.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>True if exists.</returns>
        public bool ExistsByName(string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            Expr e = new Expr(_ORM.GetColumnName<User>(nameof(User.Name)), OperatorEnum.Equals, name);
            return _ORM.Exists<User>(e);
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
