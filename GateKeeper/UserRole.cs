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
    /// User mapping to a given role.
    /// </summary>
    [Table("userroles")]
    public class UserRole
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
        /// User GUID.
        /// Must be exactly 36 characters.
        /// Must not be null.
        /// </summary>
        [Column("userguid", false, DataTypes.Nvarchar, 36, false)]
        public string UserGUID
        {
            get
            {
                return _UserGUID;
            }
            set
            {
                value = Helpers.Sanitize(value);
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(UserGUID));
                _UserGUID = value;
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

        #endregion

        #region Private-Members

        private string _GUID = Guid.NewGuid().ToString();
        private string _UserGUID = Guid.NewGuid().ToString();
        private string _RoleGUID = Guid.NewGuid().ToString();

        private User _User = null;
        private Role _Role = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public UserRole()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="role">Role.</param>
        public UserRole(User user, Role role)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (role == null) throw new ArgumentNullException(nameof(role));

            _Role = role;
            _RoleGUID = _Role.GUID;
            _User = user;
            _UserGUID = _User.GUID;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
