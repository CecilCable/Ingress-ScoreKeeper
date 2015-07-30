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
        private readonly IDictionary<int, CpScore> _scores = new Dictionary<int, CpScore>();

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
            //use a readonly dictionary instead?
            return new CycleScore(new CycleIdentifier(int.Parse(RowKey)), Timestamp.Ticks, _scores.ToArray());
        }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);
            foreach (var item in properties)
            {
                var cp = int.Parse(item.Key.Substring(3));
                if (item.Key.StartsWith("res"))
                {
                    SetResistanceScore(cp, item.Value.Int32Value.Value);
                }
                else if (item.Key.StartsWith("enl"))
                {
                    SetEnlightenedScore(cp, item.Value.Int32Value.Value);
                }
            }
        }

        private void SetResistanceScore(int cp, int resistanceScore)
        {
            CpScore cpScore;
            if (!_scores.TryGetValue(cp, out cpScore))
            {
                cpScore = new CpScore(resistanceScore, 0);
                _scores.Add(cp, cpScore);
                return;
            }
            cpScore.ResistanceScore = resistanceScore;
        }
        private void SetEnlightenedScore(int cp, int enlightenedScore)
        {
            CpScore cpScore;
            if (!_scores.TryGetValue(cp, out cpScore))
            {
                cpScore = new CpScore(0, enlightenedScore);
                _scores.Add(cp, cpScore);
                return;
            }
            cpScore.EnlightenedScore = enlightenedScore;
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

        public void SaveScores(int checkpoint, CpScore cpScore)
        {
            _scores[checkpoint] = cpScore;
        }
    }
}