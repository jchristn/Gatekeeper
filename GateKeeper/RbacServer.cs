using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqliteWrapper;

namespace GateKeeper
{
    public class RbacServer
    {
        #region Public-Members

        public bool DefaultPermit = false;

        #endregion

        #region Private-Members
         
        private DatabaseClient Db;

        #endregion

        #region Constructors-and-Factories

        public RbacServer(string dbFile)
        {
            if (String.IsNullOrEmpty(dbFile)) throw new ArgumentNullException(nameof(dbFile)); 
            Db = new DatabaseClient(dbFile, false);

            CreateTables();
        }
         
        public RbacServer(string dbFile, bool dbDebug)
        {
            if (String.IsNullOrEmpty(dbFile)) throw new ArgumentNullException(nameof(dbFile)); 
            Db = new DatabaseClient(dbFile, dbDebug);

            CreateTables();
        }

        #endregion

        #region Public-Members

        public bool Authorize(string user, string resource, string operation)
        {
            if (String.IsNullOrEmpty(user)) throw new ArgumentNullException(nameof(user));
            if (String.IsNullOrEmpty(resource)) throw new ArgumentNullException(nameof(resource));
            if (String.IsNullOrEmpty(operation)) throw new ArgumentNullException(nameof(operation));

            string query = 
                "SELECT IFNULL(SUM(allow),0) AS allow_total FROM role_perm WHERE " +
                "    resource = '" + DatabaseClient.SanitizeString(resource) + "' " +
                "AND operation = '" + DatabaseClient.SanitizeString(operation) + "' " +
                "AND rolename IN " +
                "(" +
                "  SELECT rolename FROM user_role WHERE username = '" + DatabaseClient.SanitizeString(user) + "'" +
                ")";

            DataTable result = Db.Query(query);
            if (result == null || result.Rows.Count < 1) return DefaultPermit;

            foreach (DataRow row in result.Rows)
            {
                int ret = Convert.ToInt32(row["allow_total"]);
                return IntToBool(ret);
            }

            return DefaultPermit;
        }

        public void AddUserToRole(string user, string role)
        {
            if (String.IsNullOrEmpty(user)) throw new ArgumentNullException(nameof(user));
            if (String.IsNullOrEmpty(role)) throw new ArgumentNullException(nameof(role));

            string insertQuery = "INSERT INTO user_role (username, rolename) VALUES (" +
                "'" + DatabaseClient.SanitizeString(user) + "', " +
                "'" + DatabaseClient.SanitizeString(role) + "'" +
                ")";

            Db.Query(insertQuery);
            return;
        }

        public void AddRolePermission(string role, string resource, string operation, bool allow)
        {
            if (String.IsNullOrEmpty(role)) throw new ArgumentNullException(nameof(role));
            if (String.IsNullOrEmpty(resource)) throw new ArgumentNullException(nameof(resource));
            if (String.IsNullOrEmpty(operation)) throw new ArgumentNullException(nameof(operation));

            string insertQuery = "INSERT INTO role_perm (rolename, resource, operation, allow) VALUES (" +
                "'" + DatabaseClient.SanitizeString(role) + "', " +
                "'" + DatabaseClient.SanitizeString(resource) + "', " +
                "'" + DatabaseClient.SanitizeString(operation) + "', " +
                BoolToInt(allow) + 
                ")";

            Db.Query(insertQuery);
            return;
        }

        public void RemoveUser(string user)
        {
            if (String.IsNullOrEmpty(user)) throw new ArgumentNullException(nameof(user));

            string userQuery = "DELETE FROM user_role WHERE username = '" + DatabaseClient.SanitizeString(user) + "'";
            Db.Query(userQuery);
            return;
        }

        public void RemoveUserRole(string user, string role)
        {
            if (String.IsNullOrEmpty(user)) throw new ArgumentNullException(nameof(user));
            if (String.IsNullOrEmpty(role)) throw new ArgumentNullException(nameof(role));

            string userQuery = "DELETE FROM user_role WHERE " +
                "    username = '" + DatabaseClient.SanitizeString(user) + "' " +
                "AND rolename = '" + DatabaseClient.SanitizeString(role) + "'";
            Db.Query(userQuery); 
            return;
        }

        public void RemoveRole(string role)
        {
            if (String.IsNullOrEmpty(role)) throw new ArgumentNullException(nameof(role));

            string userQuery = "DELETE FROM user_role WHERE rolename = '" + DatabaseClient.SanitizeString(role) + "'";
            Db.Query(userQuery);
            string roleQuery = "DELETE FROM role_perm WHERE rolename = '" + DatabaseClient.SanitizeString(role) + "'";
            Db.Query(roleQuery);
            return;
        }

        public void RemoveRolePermission(string role, string resource, string operation)
        {
            if (String.IsNullOrEmpty(role)) throw new ArgumentNullException(nameof(role));
            if (String.IsNullOrEmpty(resource)) throw new ArgumentNullException(nameof(resource));
            if (String.IsNullOrEmpty(operation)) throw new ArgumentNullException(nameof(operation));

            string query = "DELETE FROM role_perm WHERE " +
                "    rolename = '" + DatabaseClient.SanitizeString(role) + "' " +
                "AND resource = '" + DatabaseClient.SanitizeString(resource) + "' " +
                "AND operation = '" + DatabaseClient.SanitizeString(operation) + "'";

            Db.Query(query);
            return;
        }

        public bool UserExists(string user)
        {
            if (String.IsNullOrEmpty(user)) throw new ArgumentNullException(nameof(user));

            string query = "SELECT * FROM user_role WHERE username = '" + DatabaseClient.SanitizeString(user) + "'";
            DataTable result = Db.Query(query);
            if (result != null && result.Rows.Count > 0) return true;
            return false;
        }

        public bool RoleExists(string role)
        {
            if (String.IsNullOrEmpty(role)) throw new ArgumentNullException(nameof(role));

            string query = "SELECT * FROM role_perm WHERE rolename = '" + DatabaseClient.SanitizeString(role) + "'";
            DataTable result = Db.Query(query);
            if (result != null && result.Rows.Count > 0) return true;
            return false;
        }

        public bool UserRoleExists(string user, string role)
        {
            if (String.IsNullOrEmpty(user)) throw new ArgumentNullException(nameof(user));
            if (String.IsNullOrEmpty(role)) throw new ArgumentNullException(nameof(role));

            string query =
                "SELECT * FROM user_role WHERE " +
                "    username = '" + DatabaseClient.SanitizeString(user) + "' " +
                "AND rolename = '" + DatabaseClient.SanitizeString(role) + "'";
            DataTable result = Db.Query(query);
            if (result != null && result.Rows.Count > 0) return true;
            return false;
        }

        public bool RolePermissionExists(string role, string resource, string operation)
        {
            if (String.IsNullOrEmpty(role)) throw new ArgumentNullException(nameof(role));
            if (String.IsNullOrEmpty(resource)) throw new ArgumentNullException(nameof(resource));
            if (String.IsNullOrEmpty(operation)) throw new ArgumentNullException(nameof(operation));

            string query =
                "SELECT * FROM role_perm WHERE " +
                "    rolename = '" + DatabaseClient.SanitizeString(role) + "' " +
                "AND resource = '" + DatabaseClient.SanitizeString(resource) + "' " +
                "AND operation = '" + DatabaseClient.SanitizeString(operation) + "' ";
            DataTable result = Db.Query(query);
            if (result != null && result.Rows.Count > 0) return true;
            return false;
        }

        public List<string> GetUserRoles(string user)
        {
            if (String.IsNullOrEmpty(user)) throw new ArgumentNullException(nameof(user));

            string query = "SELECT * FROM user_role WHERE username = '" + DatabaseClient.SanitizeString(user) + "'";
            DataTable result = Db.Query(query);

            List<string> ret = new List<string>();
            if (result != null && result.Rows.Count > 0)
            {
                foreach (DataRow curr in result.Rows)
                {
                    ret.Add(curr["rolename"].ToString());
                }
            }

            ret = ret.Distinct().ToList();
            return ret;
        }

        public List<RolePermission> GetRolePermissions(string role)
        {
            if (String.IsNullOrEmpty(role)) throw new ArgumentNullException(nameof(role));

            string query = "SELECT * FROM role_perm WHERE rolename = '" + DatabaseClient.SanitizeString(role) + "'";
            DataTable result = Db.Query(query);

            List<RolePermission> ret = new List<RolePermission>();
            if (result != null && result.Rows.Count > 0)
            {
                foreach (DataRow curr in result.Rows)
                {
                    RolePermission r = RolePermission.FromDataRow(curr);
                    ret.Add(r);
                }
            }

            ret = ret.Distinct().ToList();
            return ret;
        }

        #endregion

        #region Private-Members

        private void CreateTables()
        {
            // CREATE TABLE IF NOT EXISTS fp_pool (fp_pool_id INTEGER PRIMARY KEY AUTOINCREMENT, fp_key VARCHAR(64), data_len INTEGER, ref_count INTEGER)
            string userRoleTable =
                "CREATE TABLE IF NOT EXISTS user_role " +
                "(" +
                "  username VARCHAR(128), " +
                "  rolename VARCHAR(128)  " +
                ")";

            string rolePermTable =
                "CREATE TABLE IF NOT EXISTS role_perm " +
                "(" +
                "  rolename VARCHAR(128), " +
                "  resource VARCHAR(128), " +
                "  operation VARCHAR(32), " +
                "  allow INTEGER " +
                ")";

            Db.Query(userRoleTable);
            Db.Query(rolePermTable);
        }

        private int BoolToInt(bool val)
        {
            if (val) return 1;
            return 0;
        }

        private bool IntToBool(int val)
        {
            if (val > 0) return true;
            return false;
        }

        #endregion
    }
}
