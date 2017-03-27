using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateKeeper
{
    public class RolePermission
    {
        public string RoleName { get; set; }
        public string Resource { get; set; }
        public string Operation { get; set; }
        public bool Allow { get; set; }

        public static RolePermission FromDataRow(DataRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            RolePermission r = new RolePermission();
            r.RoleName = row["rolename"].ToString();
            r.Resource = row["resource"].ToString();
            r.Operation = row["operation"].ToString();

            if (Convert.ToInt32(row["allow"]) > 0) r.Allow = true;
            else r.Allow = false;

            return r;
        }
    }
}
