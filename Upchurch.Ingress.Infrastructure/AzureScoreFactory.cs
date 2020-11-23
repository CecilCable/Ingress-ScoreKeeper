using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Upchurch.Ingress.Domain;

namespace Upchurch.Ingress.Infrastructure
{
    

    public class ScraperService: IScraperService
    {
        private readonly CloudTable _cloudTable;
        public ScraperService(string connectionString)
        {
            // Create the table client.
            var tableClient = CloudStorageAccount.Parse(connectionString).CreateCloudTableClient();
            _cloudTable = tableClient.GetTableReference("ScraperMeta");
        }
        public IScraperMeta GetData()
        {
            var scraperMeta = _cloudTable.Execute(TableOperation.Retrieve<ScraperEntity>(AzureScoreFactory.CincinnatiArea, AzureScoreFactory.CincinnatiArea));
            return (ScraperEntity)scraperMeta.Result;
        }

        public void SetMeta(IScraperMeta scraperMeta)
        {
            var oldMeta = _cloudTable.Execute(TableOperation.Retrieve<ScraperEntity>(AzureScoreFactory.CincinnatiArea,
                AzureScoreFactory.CincinnatiArea));
            var oldEntity = (ScraperEntity)oldMeta.Result;
            oldEntity.Session = scraperMeta.Session;
            oldEntity.Token = scraperMeta.Token;
            oldEntity.Version = scraperMeta.Version;

            _cloudTable.Execute(TableOperation.Replace(oldEntity));
        }
    }

    public class AzureScoreFactory : ICycleScoreUpdater
    {
        public const string CincinnatiArea = "AM02-KILO-00";

        private readonly CloudTable _cloudTable;

        private readonly IDictionary<int, ScoreEntity> _cycleScoresCache;

        public AzureScoreFactory(string connectionString)
        {
            // Create the table client.
            var tableClient = CloudStorageAccount.Parse(connectionString).CreateCloudTableClient();

            // Create the table if it doesn't exist.
            _cloudTable = tableClient.GetTableReference("scores");
            _cloudTable.CreateIfNotExists();
            _cycleScoresCache = new ConcurrentDictionary<int, ScoreEntity>();
        }


        private ScoreEntity ScoreForCycleFromStorage(CycleIdentifier cycle)
        {
            var scoreEntity = new ScoreEntity(cycle);

            var retrievedResult = _cloudTable.Execute(TableOperation.Retrieve<ScoreEntity>(CincinnatiArea, scoreEntity.RowKey));

            if (retrievedResult.Result == null)
            {
                // Execute the insert operation.
                // don't insert yet
                //_cloudTable.Execute(TableOperation.Insert(scoreEntity));
                return scoreEntity;
            }
            scoreEntity = (ScoreEntity) retrievedResult.Result;
            return scoreEntity;
        }

        public CycleScore GetScoreForCycle(CycleIdentifier cycle)
        {
            if (_cycleScoresCache.TryGetValue(cycle.Id, out var scoreEntity))
            {
                return scoreEntity.CycleScore();
            }
            scoreEntity = ScoreForCycleFromStorage(cycle);
            _cycleScoresCache[cycle.Id] = scoreEntity;
            return scoreEntity.CycleScore();
        }

        public bool UpdateScore(CycleIdentifier cycle, int checkpoint, long timestampTicks, CpScore cpScore)
        {
            return UpdateScore(cycle, timestampTicks, new KeyValuePair<int, CpScore>(checkpoint, cpScore));
        }

        public bool UpdateScore(CycleIdentifier cycle, long? timestampTicks, params KeyValuePair<int, CpScore>[] cpScores)
        {
            var scoreEntity = _cycleScoresCache[cycle.Id];
            if (timestampTicks.HasValue && scoreEntity.Timestamp.Ticks != timestampTicks.Value)//final check before we overwrite something we didn't mean to
            {
                return false;
            }
            if (cpScores==null || cpScores.Length == 0)
            {
                return false;
            }
            foreach (var keyValuePair in cpScores)
            {
                scoreEntity.SaveScores(keyValuePair.Key, keyValuePair.Value);
            }

            //this does update scoreEntity.TimeStamp
            _cloudTable.Execute(scoreEntity.Timestamp == DateTimeOffset.MinValue ? TableOperation.Insert(scoreEntity) : TableOperation.Replace(scoreEntity));
            //should we check the _cloudTable.Execute().HttpStatusCode ??
            return true;
            //what is the new TimeStamp??
            //else it's not the right timestamp
        }
        

        public bool SetSnooze(CycleIdentifier cycle, long timestampTicks, bool isSnooze)
        {
            var scoreEntity = _cycleScoresCache[cycle.Id];
            if (scoreEntity.Timestamp.Ticks != timestampTicks)//final check before we overwrite something we didn't mean to
            {
                return false;
            }
            scoreEntity.SetSnooze(isSnooze);
            //this does update scoreEntity.TimeStamp
            _cloudTable.Execute(scoreEntity.Timestamp == DateTimeOffset.MinValue ? TableOperation.Insert(scoreEntity) : TableOperation.Replace(scoreEntity));
            //should we check the _cloudTable.Execute().HttpStatusCode ??
            return true;
            //what is the new TimeStamp??
            //else it's not the right timestamp
        }

        
    }
}