using System;
using System.Collections.Generic;
using System.Text;
using Watson.ORM.Sqlite;
using Watson.ORM.Core;
using ExpressionTree;

namespace GateKeeper
{
    /// <summary>
    /// Resource manager.
    /// </summary>
    public class ResourceManager
    {
        #region Public-Members

        #endregion

        #region Internal-Members

        internal PermissionManager Permissions { get; set; } = null;
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
        public ResourceManager(WatsonORM orm)
        {
            _ORM = orm ?? throw new ArgumentNullException(nameof(orm));
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Add.
        /// </summary>
        /// <param name="resource">Resource.</param>
        /// <returns>Object.</returns>
        public Resource Add(Resource resource)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            if (ExistsByName(resource.Name))
                throw new ArgumentException("An item with the same key has already been added.");

            return _ORM.Insert<Resource>(resource);
        }

        /// <summary>
        /// Remove.
        /// </summary>
        /// <param name="resource">Resource.</param>
        public void Remove(Resource resource)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            if (!ExistsByName(resource.Name))
                throw new KeyNotFoundException("The specified key was not found.");

            Permissions.RemoveResourcePermissions(resource);

            _ORM.Delete<Resource>(resource);
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

            Resource r = GetFirstByName(name);
            if (r == null)
                throw new KeyNotFoundException("The specified key was not found.");

            Permissions.RemoveResourcePermissions(r);

            _ORM.Delete<Resource>(r);
        }

        /// <summary>
        /// Retrieve all.
        /// </summary>
        /// <returns>List.</returns>
        public List<Resource> All()
        {
            Expr e = new Expr(_ORM.GetColumnName<Resource>(nameof(Resource.Id)), OperatorEnum.GreaterThan, 0);
            return _ORM.SelectMany<Resource>(e);
        }

        /// <summary>
        /// Retrieve first record by name.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>Object.</returns>
        public Resource GetFirstByName(string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            Expr e = new Expr(_ORM.GetColumnName<Resource>(nameof(Resource.Name)), OperatorEnum.Equals, name);
            Resource r = _ORM.SelectFirst<Resource>(e);
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
            Expr e = new Expr(_ORM.GetColumnName<Resource>(nameof(Resource.Name)), OperatorEnum.Equals, name);
            return _ORM.Exists<Resource>(e);
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
