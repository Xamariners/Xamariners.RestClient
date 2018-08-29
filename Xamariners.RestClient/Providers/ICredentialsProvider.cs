// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICredentialsProvider.cs" company="">
//
// </copyright>
// <summary>
//   The CredentialsProvider interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Xamariners.RestClient.Models;

namespace Xamariners.RestClient.Providers
{
    /// <summary>
    /// The CredentialsProvider interface.
    /// </summary>
    public interface ICredentialsProvider
    {
        /// <summary>
        /// Gets or sets the auth token.
        /// </summary>
        AuthToken AuthToken { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        string Username { get; set; }

        Guid UserID { get; set; }

        string UserRole { get; set; }
    }
}