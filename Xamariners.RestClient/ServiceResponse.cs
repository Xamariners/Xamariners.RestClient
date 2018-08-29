// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceResponse.cs" company="Xamariners ">
//     All code copyright Xamariners . all rights reserved
//     Date: 06 08 2018
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Xamariners.RestClient.Helpers;
using Xamariners.RestClient.Infrastructure;
using Xamariners.RestClient.Interfaces;

namespace Xamariners.RestClient
{
    using System.IO;

    /// <inheritdoc />
    /// <summary>
    /// The service response.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class ServiceResponse<T> : IServiceResponse 
    {
        private string _errorMessage;
        private List<string> _errors;

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceResponse{T}"/> class. 
        ///     Initializes a new instance of the ServiceResponse class.
        /// </summary>
        public ServiceResponse()
        {
            Errors = new ObservableCollection<string>();
            Errors.CollectionChanged += ErrorsOnCollectionChanged;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceResponse{T}"/> class. 
        /// Initializes a new instance of the ServiceResponse class.
        /// </summary>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="errorMessage"></param>
        public ServiceResponse(ServiceStatus status, string message = "", string errorMessage = "", Exception exception = null, int amount = 0, int start = 0, T data = default(T), String stringData = null, ServiceErrorType errorType = ServiceErrorType.General) : this()
        {
            Status = status;
            Message = message;
            Amount = amount;
            Start = start;
            Data = data;
            StringData = stringData;
            ServiceErrorType = errorType;
            
            if (exception != null)
            {
                ErrorMessage = exception.Message;
                Message = exception.GetInnermostExceptionMessage();
                Status = ServiceStatus.Error;
            }
            else if (!string.IsNullOrEmpty(errorMessage))
            {
                ErrorMessage = errorMessage;
                Status = ServiceStatus.Error;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceResponse{T}"/> class. 
        /// Initializes a new instance of the ServiceResponse class.
        /// </summary>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="errorMessage"></param>
        public ServiceResponse(bool status, string message = "", string errorMessage = "", Exception exception = null, int amount = 0, int start = 0, T data = default(T))
            : this(ParseStatus(status), message, errorMessage, exception, amount, start, data)
        {
        }

        #endregion

        #region Public Properties
        
        /// <summary>
        ///     Gets or sets Message.
        ///     That's the bit we want to display to users
        ///     TODO to be replaced by ServiceErrorType only
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        ///     Gets or sets Status.
        /// </summary>
        public ServiceStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Gets or sets the string data.
        /// </summary>
        public string StringData { get; set; }

        /// <summary>
        /// The set data.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        public void SetData(object data)
        {
            Data = (T)data;
        }

        /// <summary>
        ///     Gets or sets Error Message.
        ///     That's the bit that we want to log and show developers
        ///     never show to users
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                if(!string.IsNullOrEmpty(value))
                    Status = ServiceStatus.Error;

                _errorMessage = value;
            }
        }

        /// <summary>
        /// Gets or sets the elapsed time.
        /// </summary>
        public TimeSpan ElapsedTime { get; set; }

        /// <summary>
        /// Gets or sets the requested time.
        /// </summary>
        public DateTime RequestDateTime { get; set; }

        /// <summary>
        /// Gets or sets the errors as string 
        /// (errors are not serializable)
        /// </summary>
        public ObservableCollection<string> Errors { get; set; }

        /// <summary>
        /// Gets or sets the record count.
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// Gets or sets the paging start.
        /// </summary>
        public int Start { get; set; }

        /// <summary>
        ///     We need to get rid of the Message and just use ServiceErrorType
        ///     this will allow clients to then display message based on what error they get
        /// </summary>
        public ServiceErrorType ServiceErrorType { get; set; }

        /// <summary>
        /// ProcessingTimestamp
        /// </summary>
        public DateTime? ProcessingTimestamp { get; set; }

        /// <summary>
        /// TotalCount
        /// </summary>
        public long? TotalCount { get; set; }

        // Hack to get image stream on
        public Stream Stream { get; set; }
        
        public int StatusCode { get; set; }

        #endregion

        #region Methods
        
        /// <summary>
        /// The is ok.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsOK()
        {
            return Status == ServiceStatus.Success;
        }

        private static ServiceStatus ParseStatus(bool status)
        {
            return status ? ServiceStatus.Success : ServiceStatus.Error;
        }

        private void ErrorsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if(notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Add)
                Status = ServiceStatus.Error;
        }
        #endregion
    }
}