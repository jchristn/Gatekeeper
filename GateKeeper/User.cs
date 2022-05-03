using System;
using System.Collections.Generic;
using System.Text;
using Watson.ORM.Core;

namespace GateKeeper
{
    /// <summary>
    /// User, i.e. an entity that is assigned one or more roles and attempts to perform an operation against a resource.
    /// </summary>
    [Table("users")]
    public class User
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

        #endregion

        #region Private-Members

        private string _GUID = Guid.NewGuid().ToString();
        private string _Name = "My User";

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public User()
        {

        }

        /// <summary>
        /// Instantiate using a system-assigned GUID.
        /// </summary>
        /// <param name="name">Name.</param>
        public User(string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            _Name = name;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
