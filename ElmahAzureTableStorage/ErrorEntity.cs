namespace ElmahAzureTableStorage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.WindowsAzure.StorageClient;

    /// <summary>
    /// Descriptions of the Entity Written To Windows Azure Table Storage
    /// </summary>
    public class ErrorEntity : TableServiceEntity
    {
        [System.Obsolete("Provided For Serialization From Windows Azure Do No Call Directly")]
        public ErrorEntity()
        {
        }

        /// <summary>
        /// Initialize a new instance of the ErrorEntity class. 
        /// </summary>
        /// <param name="timeUtc"></param>
        /// <param name="Id"></param>
        public ErrorEntity(DateTime timeUtc, Guid Id)
            : base(ErrorEntity.GetParitionKey(timeUtc), ErrorEntity.GetRowKey(Id))
        {
            this.TimeUtc = timeUtc;
            this.Id = Id;
        }

        /// <summary>
        /// Given a DateTime Return a Parition Key
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string GetParitionKey(DateTime time)
        {
            return time.ToString("yyyyMMddHH");
        }

        /// <summary>
        /// Given a Error Identifier Return A Parition Key
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetRowKey(Guid id)
        {
            return id.ToString().Replace("-", "").ToLower();
        }

        /// <summary>
        /// Unique Error Identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Get or set
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// Get or set
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Get or set
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Get or set
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Get or set
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Get or set
        /// </summary>
        public DateTime TimeUtc { get; set; }

        /// <summary>
        /// Get or set
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Get or set
        /// </summary>
        public string ErrorXml { get; set; }
    }
}