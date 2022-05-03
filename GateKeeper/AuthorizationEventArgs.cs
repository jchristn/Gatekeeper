using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper
{
    /// <summary>
    /// Authorization event.
    /// </summary>
    public class AuthorizationEventArgs : EventArgs
    {
        #region Public-Members

        /// <summary>
        /// Username.
        /// </summary>
        public string Username
        {
            get
            {
                return _Username;
            }
        }

        /// <summary>
        /// Resource.
        /// </summary>
        public string Resource
        {
            get
            {
                return _Resource;
            }
        }

        /// <summary>
        /// Operation.
        /// </summary>
        public string Operation
        {
            get
            {
                return _Operation;
            }
        }
        
        /// <summary>
        /// Metadata.
        /// </summary>
        public object Metadata
        {
            get
            {
                return _Metadata;
            }
        }

        /// <summary>
        /// Flag to indicate if the operation was authorized.
        /// </summary>
        public bool Authorized
        {
            get
            {
                return _Authorized;
            }
        }

        /// <summary>
        /// Matching entries.
        /// </summary>
        public List<MatchingEntry> MatchingEntries
        {
            get
            {
                return _MatchingEntries;
            }
        }

        #endregion

        #region Private-Members

        private string _Username = null;
        private string _Resource = null;
        private string _Operation = null;
        private bool _Authorized = false;
        private object _Metadata = null;
        private List<MatchingEntry> _MatchingEntries = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public AuthorizationEventArgs()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="resource">Resource.</param>
        /// <param name="operation">Operation.</param>
        /// <param name="authorized">Flag to indicate if the operation was authorized.</param>
        /// <param name="metadata">Metadata.</param>
        public AuthorizationEventArgs(string username, string resource, string operation, bool authorized, List<MatchingEntry> entries, object metadata = null)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
            if (String.IsNullOrEmpty(resource)) throw new ArgumentNullException(nameof(resource));
            if (String.IsNullOrEmpty(operation)) throw new ArgumentNullException(nameof(operation));

            _Username = username;
            _Resource = resource;
            _Operation = operation;
            _Authorized = authorized;
            _Metadata = metadata;
            _MatchingEntries = entries;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
