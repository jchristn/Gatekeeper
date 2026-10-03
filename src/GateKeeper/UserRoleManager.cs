namespace GateKeeper
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using ExpressionTree;
    using Watson.ORM.Core;
    using Watson.ORM.Sqlite;

    /// <summary>
    /// User role mapping manager.
    /// </summary>
    public class UserRoleManager
    {
        #region Public-Members

        #endregion

        #region Internal-Members

        internal PermissionManager Permissions { get; set; } = null;
        internal ResourceManager Resources { get; set; } = null;

        #endregion

        #region Private-Members

        private WatsonORM _ORM = null;
        private UserManager _Users = null;
        private RoleManager _Roles = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="orm">ORM.</param>
        /// <param name="users">User manager.</param>
        /// <param name="roles">Role manager.</param>
        public UserRoleManager(WatsonORM orm, UserManager users, RoleManager roles)
        {
            _ORM = orm ?? throw new ArgumentNullException(nameof(orm));
            _Users = users ?? throw new ArgumentNullException(nameof(users));
            _Roles = roles ?? throw new ArgumentNullException(nameof(roles));
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Add.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="role">Role.</param>
        /// <returns>Object.</returns>
        public UserRole Add(User user, Role role)
        {
            return GateKeeperTelemetry.RunManagement(GateKeeperTelemetryNames.EntityUserRole, GateKeeperTelemetryNames.OperationAdd, () =>
            {
                if (user == null) throw new ArgumentNullException(nameof(user));
                if (role == null) throw new ArgumentNullException(nameof(role));

                if (!_Users.ExistsByName(user.Name)) throw new KeyNotFoundException("The specified user was not found.");
                if (!_Roles.ExistsByName(role.Name)) throw new KeyNotFoundException("The specified role was not found.");

                if (Exists(user, role))
                    throw new ArgumentException("An item with the same keys has already been added.");

                return GateKeeperTelemetry.RunDb(GateKeeperTelemetryNames.DbOperationInsert, GateKeeperTelemetryNames.DbCollectionUserRoles, () => _ORM.Insert<UserRole>(new UserRole(user, role)));
            });
        }

        /// <summary>
        /// Remove.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="role">Role.</param>
        public void Remove(User user, Role role)
        {
            GateKeeperTelemetry.RunManagement(GateKeeperTelemetryNames.EntityUserRole, GateKeeperTelemetryNames.OperationRemove, () =>
            {
                if (user == null) throw new ArgumentNullException(nameof(user));
                if (role == null) throw new ArgumentNullException(nameof(role));

                if (!_Users.ExistsByName(user.Name)) throw new KeyNotFoundException("The specified user was not found.");
                if (!_Roles.ExistsByName(role.Name)) throw new KeyNotFoundException("The specified role was not found.");

                UserRole ur = GetByUserRole(user, role);
                if (ur == null)
                    throw new KeyNotFoundException("The specified user role was not found.");

                GateKeeperTelemetry.RunDb(GateKeeperTelemetryNames.DbOperationDelete, GateKeeperTelemetryNames.DbCollectionUserRoles, () => _ORM.Delete<UserRole>(ur));
            });
        }

        /// <summary>
        /// Remove all user role mappings for a given user.
        /// </summary>
        /// <param name="user">User.</param>
        public void RemoveUserRolesByUser(User user)
        {
            GateKeeperTelemetry.RunManagement(GateKeeperTelemetryNames.EntityUserRole, GateKeeperTelemetryNames.OperationRemoveByUser, () =>
            {
                if (user == null) throw new ArgumentNullException(nameof(user));
                Expr e = new Expr(_ORM.GetColumnName<UserRole>(nameof(UserRole.UserGUID)), OperatorEnum.Equals, user.GUID);
                GateKeeperTelemetry.RunDb(GateKeeperTelemetryNames.DbOperationDelete, GateKeeperTelemetryNames.DbCollectionUserRoles, () => _ORM.DeleteMany<UserRole>(e));
            });
        }

        /// <summary>
        /// Remove all user role mappings for a given role.
        /// </summary>
        /// <param name="role">Role.</param>
        public void RemoveUserRolesByRole(Role role)
        {
            GateKeeperTelemetry.RunManagement(GateKeeperTelemetryNames.EntityUserRole, GateKeeperTelemetryNames.OperationRemoveByRole, () =>
            {
                if (role == null) throw new ArgumentNullException(nameof(role));
                Expr e = new Expr(_ORM.GetColumnName<UserRole>(nameof(UserRole.RoleGUID)), OperatorEnum.Equals, role.GUID);
                GateKeeperTelemetry.RunDb(GateKeeperTelemetryNames.DbOperationDelete, GateKeeperTelemetryNames.DbCollectionUserRoles, () => _ORM.DeleteMany<UserRole>(e));
            });
        }

        /// <summary>
        /// Retrieve all.
        /// </summary>
        /// <returns>List.</returns>
        public List<UserRole> All()
        {
            return GateKeeperTelemetry.RunManagement(GateKeeperTelemetryNames.EntityUserRole, GateKeeperTelemetryNames.OperationAll, () =>
            {
                Expr e = new Expr(_ORM.GetColumnName<UserRole>(nameof(UserRole.Id)), OperatorEnum.GreaterThan, 0);
                return GateKeeperTelemetry.RunDb(GateKeeperTelemetryNames.DbOperationSelect, GateKeeperTelemetryNames.DbCollectionUserRoles, () => _ORM.SelectMany<UserRole>(e));
            });
        }

        /// <summary>
        /// Retrieve all records for a given user.
        /// </summary>
        /// <param name="user">User.</param>
        /// <returns>List.</returns>
        public List<UserRole> GetByUser(User user)
        {
            return GateKeeperTelemetry.RunManagement(GateKeeperTelemetryNames.EntityUserRole, GateKeeperTelemetryNames.OperationGetByUser, () =>
            {
                if (user == null) throw new ArgumentNullException(nameof(user));
                Expr e = new Expr(_ORM.GetColumnName<UserRole>(nameof(UserRole.UserGUID)), OperatorEnum.Equals, user.GUID);
                return GateKeeperTelemetry.RunDb(GateKeeperTelemetryNames.DbOperationSelect, GateKeeperTelemetryNames.DbCollectionUserRoles, () => _ORM.SelectMany<UserRole>(e));
            });
        }

        /// <summary>
        /// Retrieve all records for a given role.
        /// </summary>
        /// <param name="role">Role.</param>
        /// <returns>List.</returns>
        public List<UserRole> GetByRole(Role role)
        {
            return GateKeeperTelemetry.RunManagement(GateKeeperTelemetryNames.EntityUserRole, GateKeeperTelemetryNames.OperationGetByRole, () =>
            {
                if (role == null) throw new ArgumentNullException(nameof(role));
                Expr e = new Expr(_ORM.GetColumnName<UserRole>(nameof(UserRole.RoleGUID)), OperatorEnum.Equals, role.GUID);
                return GateKeeperTelemetry.RunDb(GateKeeperTelemetryNames.DbOperationSelect, GateKeeperTelemetryNames.DbCollectionUserRoles, () => _ORM.SelectMany<UserRole>(e));
            });
        }

        public UserRole GetByUserRole(User user, Role role)
        {
            return GateKeeperTelemetry.RunManagement(GateKeeperTelemetryNames.EntityUserRole, GateKeeperTelemetryNames.OperationGetByUserRole, () =>
            {
                if (user == null) throw new ArgumentNullException(nameof(user));
                if (role == null) throw new ArgumentNullException(nameof(role));
                Expr e = new Expr(_ORM.GetColumnName<UserRole>(nameof(UserRole.UserGUID)), OperatorEnum.Equals, user.GUID);
                e.PrependAnd(_ORM.GetColumnName<UserRole>(nameof(UserRole.RoleGUID)), OperatorEnum.Equals, role.GUID);
                return GateKeeperTelemetry.RunDb(GateKeeperTelemetryNames.DbOperationSelect, GateKeeperTelemetryNames.DbCollectionUserRoles, () => _ORM.SelectFirst<UserRole>(e));
            });
        }

        /// <summary>
        /// Check existence by user and role.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="role">Role.</param>
        /// <returns>True if exists.</returns>
        public bool Exists(User user, Role role)
        {
            return GateKeeperTelemetry.RunManagement(GateKeeperTelemetryNames.EntityUserRole, GateKeeperTelemetryNames.OperationExists, () =>
            {
                if (user == null) throw new ArgumentNullException(nameof(user));
                if (role == null) throw new ArgumentNullException(nameof(role));
                Expr e = new Expr(_ORM.GetColumnName<UserRole>(nameof(UserRole.UserGUID)), OperatorEnum.Equals, user.GUID);
                e.PrependAnd(_ORM.GetColumnName<UserRole>(nameof(UserRole.RoleGUID)), OperatorEnum.Equals, role.GUID);
                return GateKeeperTelemetry.RunDb(GateKeeperTelemetryNames.DbOperationExists, GateKeeperTelemetryNames.DbCollectionUserRoles, () => _ORM.Exists<UserRole>(e));
            });
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
