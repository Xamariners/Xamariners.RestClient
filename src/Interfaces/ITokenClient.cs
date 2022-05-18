// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITokenClient.cs" company="">
//
// </copyright>
// <summary>
//   The TokenClient interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Xamariners.RestClient.Helpers.Models;
using Xamariners.RestClient.Infrastructure;

namespace Xamariners.RestClient.Interfaces
{

    /// <summary>
    /// The RestClient interface.
    /// </summary>
    public interface ITokenClient
    {
        /// <summary>
        /// Gets the client credential authentication token.
        /// </summary>
        /// <param name="scopeType">The scope type for the request, which has a corresponding value in <see cref="IIDSConfiguration"/></param>
        /// <param name="extraRequest">The extra request.</param>
        /// <param name="headers">The headers.</param>
        Task<ServiceResponse<AuthToken>> GetClientCredentialToken(ScopeType scopeType, Dictionary<string, string> extraRequest, Dictionary<string, string> headers);

        /// <summary>
        /// Refreshes the authentication token.
        /// </summary>
        /// <param name="headers">The headers.</param>
        Task<ServiceResponse<AuthToken>> RefreshPasswordToken(Dictionary<string, string> headers = null);

        /// <summary>
        /// Gets the authentication token.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="authToken">The authentication token.</param>
        /// <param name="headers">The headers.</param>
        Task<ServiceResponse<AuthToken>> GetPasswordToken(string username, string password, AuthToken authToken, Dictionary<string, string> headers);

        /// <summary>
        /// Set the current AuthToken value
        /// </summary>
        /// <param name="authToken"></param>
        void SetCurrentAuthToken(AuthToken authToken);
    }
}