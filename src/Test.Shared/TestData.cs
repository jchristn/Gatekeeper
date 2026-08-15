namespace GateKeeper.Test.Shared
{
    using GateKeeper;

    /// <summary>
    /// Builders for the canonical RBAC graph shared by the authorization, event, and
    /// cascade suites. Keeping the graph in one place means every host exercises the
    /// exact same fixture.
    /// </summary>
    public static class TestData
    {
        /// <summary>
        /// A fully-populated RBAC graph and handles to its principal entities.
        /// </summary>
        public sealed class Graph
        {
            /// <summary>User mapped to the admin role.</summary>
            public User Alice = null!;

            /// <summary>User mapped to the editor role.</summary>
            public User Bob = null!;

            /// <summary>User mapped to the viewer role.</summary>
            public User Carol = null!;

            /// <summary>Admin role.</summary>
            public Role Admin = null!;

            /// <summary>Editor role.</summary>
            public Role Editor = null!;

            /// <summary>Viewer role.</summary>
            public Role Viewer = null!;

            /// <summary>Documents resource.</summary>
            public Resource Documents = null!;

            /// <summary>Reports resource.</summary>
            public Resource Reports = null!;
        }

        /// <summary>
        /// Populate the server with the canonical graph.
        ///
        /// Users:   alice -> admin, bob -> editor, carol -> viewer
        /// Grants:  admin  can create/read documents, is denied delete, and has a
        ///          conflicting allow+deny on "share" (allow must win).
        ///          editor can update documents.
        ///          viewer can read reports.
        /// </summary>
        /// <param name="server">Server to populate.</param>
        /// <returns>Handles to the created entities.</returns>
        public static Graph Standard(RbacServer server)
        {
            Graph g = new Graph();

            g.Alice = server.Users.Add(new User("alice"));
            g.Bob = server.Users.Add(new User("bob"));
            g.Carol = server.Users.Add(new User("carol"));

            g.Admin = server.Roles.Add(new Role("admin"));
            g.Editor = server.Roles.Add(new Role("editor"));
            g.Viewer = server.Roles.Add(new Role("viewer"));

            g.Documents = server.Resources.Add(new Resource("documents"));
            g.Reports = server.Resources.Add(new Resource("reports"));

            server.UserRoles.Add(g.Alice, g.Admin);
            server.UserRoles.Add(g.Bob, g.Editor);
            server.UserRoles.Add(g.Carol, g.Viewer);

            server.Permissions.Add(new Permission("admin-doc-create", g.Admin, g.Documents, "create", true));
            server.Permissions.Add(new Permission("admin-doc-read", g.Admin, g.Documents, "read", true));
            server.Permissions.Add(new Permission("admin-doc-delete-deny", g.Admin, g.Documents, "delete", false));
            server.Permissions.Add(new Permission("admin-doc-share-allow", g.Admin, g.Documents, "share", true));
            server.Permissions.Add(new Permission("admin-doc-share-deny", g.Admin, g.Documents, "share", false));
            server.Permissions.Add(new Permission("editor-doc-update", g.Editor, g.Documents, "update", true));
            server.Permissions.Add(new Permission("viewer-rep-read", g.Viewer, g.Reports, "read", true));

            return g;
        }
    }
}
