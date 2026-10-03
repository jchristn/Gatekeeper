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
            return GateKeeperTelemetry.RunManagement(GateKeeperTelemetryNames.EntityUser, GateKeeperTelemetryNames.OperationAdd, () =>
            {
                if (user == null) throw new ArgumentNullException(nameof(user));

                if (ExistsByName(user.Name))
                    throw new ArgumentException("An item with the same key has already been added.");

                return GateKeeperTelemetry.RunDb(GateKeeperTelemetryNames.DbOperationInsert, GateKeeperTelemetryNames.DbCollectionUsers, () => _ORM.Insert<User>(user));
            });
        }

        /// <summary>
        /// Remove.
        /// </summary>
        /// <param name="user">User.</param>
        public void Remove(User user)
        {
            GateKeeperTelemetry.RunManagement(GateKeeperTelemetryNames.EntityUser, GateKeeperTelemetryNames.OperationRemove, () =>
            {
                if (user == null) throw new ArgumentNullException(nameof(user));

                if (!ExistsByName(user.Name))
                    throw new KeyNotFoundException("The specified key was not found.");

                UserRoles.RemoveUserRolesByUser(user);

                GateKeeperTelemetry.RunDb(GateKeeperTelemetryNames.DbOperationDelete, GateKeeperTelemetryNames.DbCollectionUsers, () => _ORM.Delete<User>(user));
            });
        }

        /// <summary>
        /// Remove by name.
        /// </summary>
        /// <param name="name">Name.</param>
        public void RemoveByName(string name)
        {
            GateKeeperTelemetry.RunManagement(GateKeeperTelemetryNames.EntityUser, GateKeeperTelemetryNames.OperationRemoveByName, () =>
            {
                if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

                if (!ExistsByName(name))
                    throw new KeyNotFoundException("The specified key was not found.");

                User u = GetFirstByName(name);
                if (u == null)
                    throw new KeyNotFoundException("The specified key was not found.");

                UserRoles.RemoveUserRolesByUser(u);

                GateKeeperTelemetry.RunDb(GateKeeperTelemetryNames.DbOperationDelete, GateKeeperTelemetryNames.DbCollectionUsers, () => _ORM.Delete<User>(u));
            });
        }

        /// <summary>
        /// Retrieve all.
        /// </summary>
        /// <returns>List.</returns>
        public List<User> All()
        {
            return GateKeeperTelemetry.RunManagement(GateKeeperTelemetryNames.EntityUser, GateKeeperTelemetryNames.OperationAll, () =>
            {
                Expr e = new Expr(_ORM.GetColumnName<User>(nameof(User.Id)), OperatorEnum.GreaterThan, 0);
                return GateKeeperTelemetry.RunDb(GateKeeperTelemetryNames.DbOperationSelect, GateKeeperTelemetryNames.DbCollectionUsers, () => _ORM.SelectMany<User>(e));
            });
        }

        /// <summary>
        /// Retrieve first record by name.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>Object.</returns>
        public User GetFirstByName(string name)
        {
            return GateKeeperTelemetry.RunManagement(GateKeeperTelemetryNames.EntityUser, GateKeeperTelemetryNames.OperationGetByName, () =>
            {
                if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
                Expr e = new Expr(_ORM.GetColumnName<User>(nameof(User.Name)), OperatorEnum.Equals, name);
                User u = GateKeeperTelemetry.RunDb(GateKeeperTelemetryNames.DbOperationSelect, GateKeeperTelemetryNames.DbCollectionUsers, () => _ORM.SelectFirst<User>(e));
                return u;
            });
        }

        /// <summary>
        /// Check existence by name.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>True if exists.</returns>
        public bool ExistsByName(string name)
        {
            return GateKeeperTelemetry.RunManagement(GateKeeperTelemetryNames.EntityUser, GateKeeperTelemetryNames.OperationExistsByName, () =>
            {
                if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
                Expr e = new Expr(_ORM.GetColumnName<User>(nameof(User.Name)), OperatorEnum.Equals, name);
                return GateKeeperTelemetry.RunDb(GateKeeperTelemetryNames.DbOperationExists, GateKeeperTelemetryNames.DbCollectionUsers, () => _ORM.Exists<User>(e));
            });
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
