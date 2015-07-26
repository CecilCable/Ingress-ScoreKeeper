using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Upchurch.Ingress.Domain;

namespace Upchurch.Ingress.Infrastructure
{
    public class ScoreEntity : TableEntity
    {
        public const string CincinnatiArea = "AM02-KILO-00";
        private IDictionary<int, CpScore> _scores = new Dictionary<int, CpScore>();

        /// <summary>
        ///     create a new checkpoint
        /// </summary>
        /// <param name="cycleScore"></param>
        public ScoreEntity(CycleIdentifier cycleScore)
        {
            PartitionKey = CincinnatiArea;
            RowKey = cycleScore.Id.ToString();
        }

        // for Serialization
        public ScoreEntity()
        {
            PartitionKey = CincinnatiArea;
        }

        public CycleScore CycleScore()
        {

            return new CycleScore(new CycleIdentifier(int.Parse(RowKey)), Timestamp, _scores.Values.ToArray());
        }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);
            foreach (var item in properties)
            {
                var cp = int.Parse(item.Key.Substring(3));
                CpScore cpScore;
                if (!_scores.TryGetValue(cp, out cpScore))
                {
                    cpScore = new CpScore(cp, 0, 0);
                    _scores.Add(cp, cpScore);
                }

                if (item.Key.StartsWith("res"))
                {
                    cpScore.ResistanceScore = item.Value.Int32Value.Value;
                }
                else if (item.Key.StartsWith("enl"))
                {
                    cpScore.EnlightenedScore = item.Value.Int32Value.Value;
                }
            }
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var results = base.WriteEntity(operationContext);
            foreach (var item in _scores)
            {
                results.Add("res" + item.Key, new EntityProperty(item.Value.ResistanceScore));
                results.Add("enl" + item.Key, new EntityProperty(item.Value.EnlightenedScore));
            }

            return results;
        }

        internal void SaveScores(ICollection<CpScore> scores)
        {
            _scores = scores.ToDictionary(item => item.Cp);
        }
    }
}