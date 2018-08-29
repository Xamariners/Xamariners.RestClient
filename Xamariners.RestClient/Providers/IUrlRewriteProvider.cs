// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICredentialsProvider.cs" company="">
//
// </copyright>
// <summary>
//   The CredentialsProvider interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;

namespace Xamariners.RestClient.Providers
{
    /// <summary>
    /// The CredentialsProvider interface.
    /// </summary>
    public interface IUrlRewriteProvider
    {
        HttpRequestMessage Rewrite(HttpRequestMessage message);
        string Rewrite(string url);
        Dictionary<string, string> UrlMapping { get; set; }
    }
}