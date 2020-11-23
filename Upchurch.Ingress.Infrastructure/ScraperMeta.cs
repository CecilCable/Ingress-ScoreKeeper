using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Upchurch.Ingress.Domain;

namespace Upchurch.Ingress.Infrastructure
{
    public class ScraperEntity : TableEntity, IScraperMeta
    {
        public ScraperEntity()
        {
            PartitionKey = AzureScoreFactory.CincinnatiArea;
            RowKey = AzureScoreFactory.CincinnatiArea;
            Timestamp = DateTimeOffset.UtcNow;
        }

        public string Version { get; set; }
        public string Token { get; set; }
        public string Session { get; set; }
    }
}
