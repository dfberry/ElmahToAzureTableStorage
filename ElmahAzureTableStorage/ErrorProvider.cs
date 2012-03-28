namespace ElmahAzureTableStorage
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Data.Services.Client;
    using System.Text;
    using System.Linq;
    using Microsoft.WindowsAzure.StorageClient;
    using Elmah;
    using Microsoft.WindowsAzure;
    using System.Text.RegularExpressions;
    using System.Xml;

    public class WindowsAzureErrorLogs : ErrorLog
    {
        /// <summary>
        /// Table Name To Use In Windows Azure Storage
        /// </summary>
        private readonly string tableName = "Elmah";

        /// <summary>
        /// Cloud Table Client To Use When Accessing Windows Azure Storage
        /// </summary>
        private readonly CloudTableClient cloudTableClient;

        /// <summary>
        /// Initialize a new instance of the WindowsAzureErrorLogs class.
        /// </summary>
        /// <param name="config"></param>
        public WindowsAzureErrorLogs(IDictionary config)
        {
            if (!(config["connectionString"] is string))
            {
                throw new Elmah.ApplicationException("Connection string is missing for the Windows Azure error log.");
            }

            if (string.IsNullOrWhiteSpace((string)config["connectionString"]))
            {
                throw new Elmah.ApplicationException("Connection string is missing for the Windows Azure error log.");
            }

            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse((string)config["connectionString"]);
            this.cloudTableClient = cloudStorageAccount.CreateCloudTableClient();

            this.cloudTableClient.CreateTableIfNotExist(this.tableName);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public override string Log(Error error)
        {
            ErrorEntity entity = new ErrorEntity(error.Time, Guid.NewGuid())
            {
                HostName = error.HostName,
                Type = error.Type,
                ErrorXml = ErrorXml.EncodeString(error),
                Message = error.Message,
                StatusCode = error.StatusCode,
                User = error.User,
                Source = error.Source
            };

            TableServiceContext tableServiceContext = this.cloudTableClient.GetDataServiceContext();
            tableServiceContext.AddObject(this.tableName, entity);
            tableServiceContext.SaveChanges();

            return entity.Id.ToString();
        }

        /// <summary>
        /// Get a Error From Windows Azure Storage
        /// </summary>
        /// <param name="id">Error Identifier (Guid)</param>
        /// <returns>Error Fetched (or Null If Not Found)</returns>
        public override ErrorLogEntry GetError(string id)
        {
            TableServiceContext tableServiceContext = this.cloudTableClient.GetDataServiceContext();

            var query = from entity in tableServiceContext.CreateQuery<ErrorEntity>(this.tableName).AsTableServiceQuery()
                        where ErrorEntity.GetRowKey(Guid.Parse(id)) == entity.RowKey
                        select entity;

            ErrorEntity errorEntity = query.FirstOrDefault();
            if (errorEntity == null)
            {
                return null;
            }

            return new ErrorLogEntry(this, id, ErrorXml.DecodeString(errorEntity.ErrorXml));
        }

        /// <summary>
        /// Get A Page Of Errors From Windows Azure Storage
        /// </summary>
        /// <param name="pageIndex">Page Index</param>
        /// <param name="pageSize">Size Of Page To Return</param>
        /// <param name="errorEntryList">List of Errors Returned</param>
        /// <returns>Total Count of Errors</returns>
        public override int GetErrors(int pageIndex, int pageSize, System.Collections.IList errorEntryList)
        {
            if (pageIndex < 0)
                throw new ArgumentOutOfRangeException("pageIndex", pageIndex, null);

            if (pageSize < 0)
                throw new ArgumentOutOfRangeException("pageSize", pageSize, null);

            TableServiceContext tableServiceContext = this.cloudTableClient.GetDataServiceContext();

            // WWB: Server Side Call To Get All Data
            ErrorEntity[] serverSideQuery = tableServiceContext.CreateQuery<ErrorEntity>(this.tableName).AsTableServiceQuery().Execute().ToArray();

            // WWB: Sorted in Reverse Order So Oldest are First
            var sorted = serverSideQuery.OrderByDescending(entity => entity.TimeUtc);

            // WWB: Trim To Just a Page From The End
            ErrorEntity[] page = sorted.Skip(pageIndex * pageSize).Take(pageSize).ToArray();

            // WWB: Convert To ErrorLogEntry classes From Windows Azure Table Entities
            IEnumerable<ErrorLogEntry> errorLogEntries = page.Select(errorEntity => new ErrorLogEntry(this, errorEntity.Id.ToString(), ErrorXml.DecodeString(errorEntity.ErrorXml)));

            // WWB: Stuff them into the class we were passed
            foreach (var errorLogEntry in errorLogEntries)
            {
                errorEntryList.Add(errorLogEntry);
            };

            return serverSideQuery.Length;
        }
    }
}