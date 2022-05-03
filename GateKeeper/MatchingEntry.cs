using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace GateKeeper
{
    /// <summary>
    /// Entry that matched the supplied authorization request.
    /// </summary>
    public class MatchingEntry
    {
        #region Public-Members

        /// <summary>
        /// Resource GUID.
        /// </summary>
        public string ResourceGUID { get; private set; } = null;

        /// <summary>
        /// Resource name.
        /// </summary>
        public string ResourceName { get; private set; } = null;

        /// <summary>
        /// User GUID.
        /// </summary>
        public string UserGUID { get; private set; } = null;

        /// <summary>
        /// User name.
        /// </summary>
        public string UserName { get; private set; } = null;

        /// <summary>
        /// Role GUID.
        /// </summary>
        public string RoleGUID { get; private set; } = null;

        /// <summary>
        /// Role name.
        /// </summary>
        public string RoleName { get; private set; } = null;

        /// <summary>
        /// User role GUID.
        /// </summary>
        public string UserRoleGUID { get; private set; } = null;

        /// <summary>
        /// Permission GUID.
        /// </summary>
        public string PermissionGUID { get; private set; } = null;

        /// <summary>
        /// Permission name.
        /// </summary>
        public string PermissionName { get; private set; } = null;

        /// <summary>
        /// Operation.
        /// </summary>
        public string Operation { get; private set; } = null;

        /// <summary>
        /// Allow.
        /// </summary>
        public bool Allow { get; private set; } = false;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public MatchingEntry()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="row">DataRow.</param>
        public MatchingEntry(DataRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));

            if (row.Table.Columns.Count > 0 && row.Table.Rows.Count > 0)
            {
                if (ColumnExists(row, "resourceguid") && ValueExists(row, "resourceguid"))
                    ResourceGUID = row["resourceguid"].ToString();

                if (ColumnExists(row, "resourcename") && ValueExists(row, "resourcename"))
                    ResourceName = row["resourcename"].ToString();

                if (ColumnExists(row, "userguid") && ValueExists(row, "userguid"))
                    UserGUID = row["userguid"].ToString();

                if (ColumnExists(row, "username") && ValueExists(row, "username"))
                    UserName = row["username"].ToString();

                if (ColumnExists(row, "roleguid") && ValueExists(row, "roleguid"))
                    RoleGUID = row["roleguid"].ToString();

                if (ColumnExists(row, "rolename") && ValueExists(row, "rolename"))
                    RoleName = row["rolename"].ToString();

                if (ColumnExists(row, "userroleguid") && ValueExists(row, "userroleguid"))
                    UserRoleGUID = row["userroleguid"].ToString();

                if (ColumnExists(row, "permissionguid") && ValueExists(row, "permissionguid"))
                    PermissionGUID = row["permissionguid"].ToString();

                if (ColumnExists(row, "permissionname") && ValueExists(row, "permissionname"))
                    PermissionName = row["permissionname"].ToString();

                if (ColumnExists(row, "operation") && ValueExists(row, "operation"))
                    Operation = row["operation"].ToString();

                if (ColumnExists(row, "allow") && ValueExists(row, "allow"))
                {
                    int val = Convert.ToInt32(row["allow"]);
                    if (val > 0) Allow = true;
                    else Allow = false;
                }
            }
        }

        /// <summary>
        /// Instantiate from DataTable.
        /// </summary>
        /// <param name="table">DataTable.</param>
        /// <returns>List.</returns>
        public static List<MatchingEntry> FromDataTable(DataTable table)
        {
            List<MatchingEntry> ret = new List<MatchingEntry>();
            if (table == null || table.Rows.Count < 1) return ret;

            foreach (DataRow row in table.Rows)
            {
                ret.Add(new MatchingEntry(row));
            }

            return ret;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        private bool ColumnExists(DataRow row, string columnName)
        {
            if (String.IsNullOrEmpty(columnName)) throw new ArgumentNullException(nameof(columnName));
            if (row == null) throw new ArgumentNullException(nameof(row));
            if (row.Table.Columns.Contains(columnName)) return true;
            return false;
        }

        private bool ValueExists(DataRow row, string columnName)
        {
            if (String.IsNullOrEmpty(columnName)) throw new ArgumentNullException(nameof(columnName));
            if (row == null) throw new ArgumentNullException(nameof(row));
            object val = row[columnName];
            if (val != null && val != DBNull.Value) return true;
            return false;
        }

        #endregion
    }
}
