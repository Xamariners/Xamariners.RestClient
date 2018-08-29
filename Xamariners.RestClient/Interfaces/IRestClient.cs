// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRestClient.cs" company="">
//   
// </copyright>
// <summary>
//   The RestClient interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamariners.RestClient.Infrastructure;


namespace Xamariners.RestClient.Interfaces
{
    using System.IO;

    /// <summary>
    /// The RestClient interface.
    /// </summary>
    public interface IRestClient
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the status code.
        /// </summary>
        int StatusCode { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="requestVerb">
        /// The request verb.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <param name="requestBody">
        /// The request body.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="amount">
        /// The amount.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="order">
        /// The order.
        /// </param>
        /// <param name="includes">
        /// The includes.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="ServiceResponse{T}"/>.
        /// </returns>
        ServiceResponse<T> Execute<T>(
            HttpVerb requestVerb,
            [CallerMemberName] string action = null,
            T requestBody = default(T), 
            string id = null,
            int amount = -1,
            int start = -1,
            string order = "",
            string[] includes = null,
            Dictionary<string, object> parameters = null,
            HttpParamMode paramMode = HttpParamMode.REST,
            string apiRoutePrefix = null,
            Dictionary<string, string> headers = null,
            bool isServiceResponse = false, 
            bool isAnonymous = false,
            bool skipHostCheck = false);

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="requestVerb">
        ///     The request verb.
        /// </param>
        /// <param name="action">
        ///     The action.
        /// </param>
        /// <param name="requestBody">
        ///     The request body.
        /// </param>
        /// <param name="id">
        ///     The id.
        /// </param>
        /// <param name="amount">
        ///     The amount.
        /// </param>
        /// <param name="start">
        ///     The start.
        /// </param>
        /// <param name="order">
        ///     The order.
        /// </param>
        /// <param name="includes">
        ///     The includes.
        /// </param>
        /// <param name="parameters">
        ///     The parameters.
        /// </param>
        /// <typeparam name="TResult">
        /// </typeparam>
        /// <typeparam name="TBody">
        /// </typeparam>
        /// <returns>
        /// The <see cref="ServiceResponse"/>.
        /// </returns>
        ServiceResponse<TResult> Execute<TResult, TBody>(HttpVerb requestVerb, [CallerMemberName] string action = null,
            TBody requestBody = default(TBody), string id = null, int amount = -1, int start = -1,
            string order = "", string[] includes = null, Dictionary<string, object> parameters = null, HttpParamMode paramMode = HttpParamMode.REST,
            string apiRoutePrefix = null, Dictionary<string, string> headers = null, bool isServiceResponse = false, bool isAnonymous = false, bool skipHostCheck = false);

        /// <summary>
        /// The execute async.
        /// </summary>
        /// <param name="requestVerb">
        ///     The request verb.
        /// </param>
        /// <param name="action">
        ///     The action.
        /// </param>
        /// <param name="requestBody">
        ///     The request body.
        /// </param>
        /// <param name="id">
        ///     The id.
        /// </param>
        /// <param name="amount">
        ///     The amount.
        /// </param>
        /// <param name="start">
        ///     The start.
        /// </param>
        /// <param name="order">
        ///     The order.
        /// </param>
        /// <param name="includes">
        ///     The includes.
        /// </param>
        /// <param name="parameters">
        ///     The parameters.
        /// </param>
        /// <param name="apiRoutePrefix"></param>
        /// <param name="headers"></param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="async Task"/>.
        /// </returns>
        Task<ServiceResponse<T>> ExecuteAsync<T>(HttpVerb requestVerb, [CallerMemberName] string action = null, T requestBody = default(T), 
            string id = null, int amount = -1, int start = -1, string order = "", string[] includes = null, 
            Dictionary<string, object> parameters = null, HttpParamMode paramMode = HttpParamMode.REST, string apiRoutePrefix = null, 
            Dictionary<string, string> headers = null, bool isServiceResponse = false, bool isAnonymous = false, bool skipHostCheck = false);

        /// <summary>
        /// The execute async.
        /// </summary>
        /// <param name="requestVerb">
        /// The request verb.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <param name="requestBody">
        /// The request body.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="amount">
        /// The amount.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="order">
        /// The order.
        /// </param>
        /// <param name="includes">
        /// The includes.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <typeparam name="TResult">
        /// </typeparam>
        /// <typeparam name="TBody">
        /// </typeparam>
        /// <returns>
        /// The <see cref="async Task"/>.
        /// </returns>
        Task<ServiceResponse<TResult>> ExecuteAsync<TResult, TBody>(
            HttpVerb requestVerb,
            [CallerMemberName] string action = null,
            TBody requestBody = default(TBody),
            string id = null,
            int amount = -1,
            int start = -1,
            string order = "",
            string[] includes = null,
            Dictionary<string, object> parameters = null,
            HttpParamMode paramMode = HttpParamMode.REST,
            string apiRoutePrefix = null,
            Dictionary<string, string> headers = null, bool isServiceResponse = false, bool isAnonymous = false, bool skipHostCheck = false);

        #endregion
    }
}