// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Credentials.cs" company="">
//   
// </copyright>
// <summary>
//   The credentials.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Xamariners.RestClient.Models
{
    /// <summary>
    /// The credentials.
    /// </summary>
    public class Credentials
    {
        #region Public Properties

        public Credentials(string username, string password)
        {
            Username = username;
            Password = password;
        }

        /// <summary>
        /// Gets or sets the auth token.
        /// </summary>
        public AuthToken AuthToken { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string Username { get; set; }

        #endregion
    }
}