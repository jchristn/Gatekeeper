using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watson.ORM.Core;

namespace GateKeeper
{
    /// <summary>
    /// Assignment of permissions to a role by name, for a specified resource and a specified operation.
    /// </summary>
    [Table("permissions")]
    public class Permission
    {
        #region Public-Members

        /// <summary>
        /// Database row ID.
        /// </summary>
        [Column("id", true, DataTypes.Int, false)]
        public int Id { get; set; }

        /// <summary>
        /// GUID.
        /// Must be exactly 36 characters.
        /// Must not be null.
        /// </summary>
        [Column("guid", false, DataTypes.Nvarchar, 36, false)]
        public string GUID
        {
            get
            {
                return _GUID;
            }
            set
            {
                value = Helpers.Sanitize(value);
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(GUID));
                _GUID = value;
            }
        }

        /// <summary>
        /// Name.
        /// Maximum 64 characters.
        /// Must not be null.
        /// </summary>
        [Column("name", false, DataTypes.Nvarchar, 64, false)]
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                value = Helpers.Sanitize(value);
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(GUID));
                _Name = value;
            }
        }

        /// <summary>
        /// Role GUID.
        /// Must be exactly 36 characters.
        /// Must not be null.
        /// </summary>
        [Column("roleguid", false, DataTypes.Nvarchar, 36, false)]
        public string RoleGUID
        {
            get
            {
                return _RoleGUID;
            }
            set
            {
                value = Helpers.Sanitize(value);
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(RoleGUID));
                _RoleGUID = value;
            }
        }

        /// <summary>
        /// Resource GUID.
        /// Must be exactly 36 characters.
        /// Must not be null.
        /// </summary>
        [Column("resourceguid", false, DataTypes.Nvarchar, 36, false)]
        public string ResourceGUID
        {
            get
            {
                return _ResourceGUID;
            }
            set
            {
                value = Helpers.Sanitize(value);
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(ResourceGUID));
                _ResourceGUID = value;
            }
        }

        /// <summary>
        /// Operation.
        /// Maximum 64 characters.
        /// Must not be null.
        /// </summary>
        [Column("operation", false, DataTypes.Nvarchar, 64, false)]
        public string Operation
        {
            get
            {
                return _Operation;
            }
            set
            {
                value = Helpers.Sanitize(value);
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Operation));
                _Operation = value;
            }
        }

        /// <summary>
        /// True if the operation is allowed.
        /// </summary>
        [Column("allow", false, DataTypes.Boolean, false)]
        public bool Allow { get; set; } = true;

        #endregion

        #region Private-Members

        private string _GUID = Guid.NewGuid().ToString();
        private string _RoleGUID = Guid.NewGuid().ToString();
        private string _ResourceGUID = Guid.NewGuid().ToString();
        private string _Name = "My Permission";
        private string _Operation = "My Operation";

        private Role _Role = null;
        private Resource _Resource = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public Permission()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="rolename">Role.</param>
        /// <param name="resource">Resource.</param>
        /// <param name="operation">Operation.</param>
        /// <param name="allow">True if the operation is allowed.</param>
        public Permission(string name, Role role, Resource resource, string operation, bool allow)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (role == null) throw new ArgumentNullException(nameof(role));
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            if (String.IsNullOrEmpty(operation)) throw new ArgumentNullException(nameof(operation));

            _Name = name;
            _Role = role;
            _RoleGUID = _Role.GUID;
            _Resource = resource;
            _ResourceGUID = _Resource.GUID;

            Operation = operation;
            Allow = allow;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
