using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Upchurch.Ingress.Domain;

namespace Upchurch.Ingress.Infrastructure
{
    public class AzureScoreFactory : ICycleScoreUpdater
    {
        private readonly CloudTable _cloudTable;

        private readonly IDictionary<int, ScoreEntity> _cycleScoresCache;

        public AzureScoreFactory(string connectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create the table client.
            var tableClient = storageAccount.CreateCloudTableClient();

            // Create the table if it doesn't exist.
            _cloudTable = tableClient.GetTableReference("scores");
            _cloudTable.CreateIfNotExists();

            _cycleScoresCache = new ConcurrentDictionary<int, ScoreEntity>();

        }




        private ScoreEntity ScoreForCycleFromStorage(CycleIdentifier cycle)
        {
            var scoreEntity = new ScoreEntity(cycle);

            var retrieveOperation = TableOperation.Retrieve<ScoreEntity>(ScoreEntity.CincinnatiArea, scoreEntity.RowKey);

            var retrievedResult = _cloudTable.Execute(retrieveOperation);

            if (retrievedResult.Result == null)
            {
                // Execute the insert operation.
                //don't insert yet
                //_cloudTable.Execute(TableOperation.Insert(scoreEntity));
                return scoreEntity;//what is the timestamp here???
            }
            else
            {
                scoreEntity = (ScoreEntity) retrievedResult.Result;
            }
            return scoreEntity;
        }

        public CycleScore GetScoreForCycle(CycleIdentifier cycle)
        {
            ScoreEntity scoreEntity;
            if (_cycleScoresCache.TryGetValue(cycle.Id, out scoreEntity))
            {
                return scoreEntity.CycleScore();
            }
            scoreEntity = ScoreForCycleFromStorage(cycle);
            _cycleScoresCache[cycle.Id] = scoreEntity;
            return scoreEntity.CycleScore();
        }

        public bool UpdateScore(CycleIdentifier cycle, int checkpoint, long timestampTicks, CpScore cpScore)
        {
            var scoreEntity = _cycleScoresCache[cycle.Id];
            if (scoreEntity.Timestamp.Ticks != timestampTicks)//final check before we overwrite something we didn't mean to
            {
                return false;
            }
            scoreEntity.SaveScores(checkpoint,cpScore);
            //this does update scoreEntity.TimeStamp
            _cloudTable.Execute(TableOperation.Replace(scoreEntity));
            //should we check the _cloudTable.Execute().HttpStatusCode ??
            return true;
            //what is the new TimeStamp??
            //else it's not the right timestamp
        }

       
    }
}