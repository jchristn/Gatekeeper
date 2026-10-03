namespace GateKeeper
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using ExpressionTree;
    using Watson.ORM.Core;
    using Watson.ORM.Sqlite;

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
            return GateKeeperTelemetry.RunManagement(GateKeeperTelemetryNames.EntityRole, GateKeeperTelemetryNames.OperationAdd, () =>
            {
                if (role == null) throw new ArgumentNullException(nameof(role));

                if (ExistsByName(role.Name))
                    throw new ArgumentException("An item with the same key has already been added.");

                return GateKeeperTelemetry.RunDb(GateKeeperTelemetryNames.DbOperationInsert, GateKeeperTelemetryNames.DbCollectionRoles, () => _ORM.Insert<Role>(role));
            });
        }

        /// <summary>
        /// Remove.
        /// </summary>
        /// <param name="role">Role.</param>
        public void Remove(Role role)
        {
            GateKeeperTelemetry.RunManagement(GateKeeperTelemetryNames.EntityRole, GateKeeperTelemetryNames.OperationRemove, () =>
            {
                if (role == null) throw new ArgumentNullException(nameof(role));

                if (!ExistsByName(role.Name))
                    throw new KeyNotFoundException("The specified key was not found.");

                UserRoles.RemoveUserRolesByRole(role);

                GateKeeperTelemetry.RunDb(GateKeeperTelemetryNames.DbOperationDelete, GateKeeperTelemetryNames.DbCollectionRoles, () => _ORM.Delete<Role>(role));
            });
        }

        /// <summary>
        /// Remove by name.
        /// </summary>
        /// <param name="name">Name.</param>
        public void RemoveByName(string name)
        {
            GateKeeperTelemetry.RunManagement(GateKeeperTelemetryNames.EntityRole, GateKeeperTelemetryNames.OperationRemoveByName, () =>
            {
                if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

                if (!ExistsByName(name))
                    throw new KeyNotFoundException("The specified key was not found.");

                Role r = GetFirstByName(name);
                if (r == null)
                    throw new KeyNotFoundException("The specified key was not found.");

                UserRoles.RemoveUserRolesByRole(r);

                GateKeeperTelemetry.RunDb(GateKeeperTelemetryNames.DbOperationDelete, GateKeeperTelemetryNames.DbCollectionRoles, () => _ORM.Delete<Role>(r));
            });
        }

        /// <summary>
        /// Retrieve all.
        /// </summary>
        /// <returns>List.</returns>
        public List<Role> All()
        {
            return GateKeeperTelemetry.RunManagement(GateKeeperTelemetryNames.EntityRole, GateKeeperTelemetryNames.OperationAll, () =>
            {
                Expr e = new Expr(_ORM.GetColumnName<Role>(nameof(Role.Id)), OperatorEnum.GreaterThan, 0);
                return GateKeeperTelemetry.RunDb(GateKeeperTelemetryNames.DbOperationSelect, GateKeeperTelemetryNames.DbCollectionRoles, () => _ORM.SelectMany<Role>(e));
            });
        }

        /// <summary>
        /// Retrieve first record by name.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>Object.</returns>
        public Role GetFirstByName(string name)
        {
            return GateKeeperTelemetry.RunManagement(GateKeeperTelemetryNames.EntityRole, GateKeeperTelemetryNames.OperationGetByName, () =>
            {
                if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
                Expr e = new Expr(_ORM.GetColumnName<Role>(nameof(Role.Name)), OperatorEnum.Equals, name);
                Role r = GateKeeperTelemetry.RunDb(GateKeeperTelemetryNames.DbOperationSelect, GateKeeperTelemetryNames.DbCollectionRoles, () => _ORM.SelectFirst<Role>(e));
                return r;
            });
        }

        /// <summary>
        /// Check existence by name.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>True if exists.</returns>
        public bool ExistsByName(string name)
        {
            return GateKeeperTelemetry.RunManagement(GateKeeperTelemetryNames.EntityRole, GateKeeperTelemetryNames.OperationExistsByName, () =>
            {
                if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
                Expr e = new Expr(_ORM.GetColumnName<Role>(nameof(Role.Name)), OperatorEnum.Equals, name);
                return GateKeeperTelemetry.RunDb(GateKeeperTelemetryNames.DbOperationExists, GateKeeperTelemetryNames.DbCollectionRoles, () => _ORM.Exists<Role>(e));
            });
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
