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
    [Table("roleperm")]
    public class RolePermission
    {
        /// <summary>
        /// Database row ID.
        /// </summary>
        [Column("id", true, DataTypes.Int, false)]
        public int Id { get; set; }

        /// <summary>
        /// Role name.
        /// </summary>
        [Column("rolename", false, DataTypes.Nvarchar, 256, false)]
        public string Rolename { get; set; }

        /// <summary>
        /// Resource name.
        /// </summary>
        [Column("resource", false, DataTypes.Nvarchar, 256, false)]
        public string Resource { get; set; }

        /// <summary>
        /// Operation.
        /// </summary>
        [Column("operation", false, DataTypes.Nvarchar, 256, false)]
        public string Operation { get; set; }

        /// <summary>
        /// True if the operation is allowed.
        /// </summary>
        [Column("allow", false, DataTypes.Boolean, false)]
        public bool Allow { get; set; } 

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public RolePermission()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="rolename">Role name.</param>
        /// <param name="resource">Resource name.</param>
        /// <param name="operation">Operation.</param>
        /// <param name="allow">True if the operation is allowed.</param>
        public RolePermission(string rolename, string resource, string operation, bool allow)
        {
            if (String.IsNullOrEmpty(rolename)) throw new ArgumentNullException(nameof(rolename));
            if (String.IsNullOrEmpty(resource)) throw new ArgumentNullException(nameof(resource));
            if (String.IsNullOrEmpty(operation)) throw new ArgumentNullException(nameof(operation));

            Rolename = rolename;
            Resource = resource;
            Operation = operation;
            Allow = allow;
        }
    }
}
