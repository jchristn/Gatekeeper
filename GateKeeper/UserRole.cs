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
    [Table("userrole")]
    public class UserRole
    {
        /// <summary>
        /// Database row ID.
        /// </summary>
        [Column("id", true, DataTypes.Int, false)]
        public int Id { get; set; }

        /// <summary>
        /// User name.
        /// </summary>
        [Column("username", false, DataTypes.Nvarchar, 256, false)]
        public string Username { get; set; }

        /// <summary>
        /// Role name.
        /// </summary>
        [Column("rolename", false, DataTypes.Nvarchar, 256, false)]
        public string Rolename { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public UserRole()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="username">User name.</param>
        /// <param name="rolename">Role name.</param>
        public UserRole(string username, string rolename)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
            if (String.IsNullOrEmpty(rolename)) throw new ArgumentNullException(nameof(rolename));

            Username = username;
            Rolename = rolename;
        }
    }
}
