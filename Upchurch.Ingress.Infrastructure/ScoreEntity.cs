using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Upchurch.Ingress.Domain;

namespace Upchurch.Ingress.Infrastructure
{
    public class ScoreEntity : TableEntity
    {
        private readonly IDictionary<int, CpScore> _scores = new Dictionary<int, CpScore>();
        private bool _isSnoozed;

        /// <summary>
        ///     create a new checkpoint
        /// </summary>
        /// <param name="cycleScore"></param>
        public ScoreEntity(CycleIdentifier cycleScore)
        {
            PartitionKey = AzureScoreFactory.CincinnatiArea;
            RowKey = cycleScore.Id.ToString();
            Timestamp = DateTimeOffset.MinValue;
        }

        // for Serialization
        public ScoreEntity()
        {
            PartitionKey = AzureScoreFactory.CincinnatiArea;
        }

        public CycleScore CycleScore()
        {
            //use a readonly dictionary instead?
            return new CycleScore(new CycleIdentifier(int.Parse(RowKey)), Timestamp.Ticks, _isSnoozed, CheckPoint.Current(), _scores.ToArray());
        }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);
            foreach (var item in properties)
            {
                if (item.Key == "snooze")
                {
                    _isSnoozed = item.Value.PropertyType == EdmType.String ? bool.Parse(item.Value.StringValue) : item.Value.BooleanValue.Value;
                    continue;
                }
                var cp = int.Parse(item.Key.Substring(3));
                if (item.Key.StartsWith("res"))
                {
                    SetResistanceScore(cp, item.Value.PropertyType == EdmType.String ? int.Parse(item.Value.StringValue) : item.Value.Int32Value.Value);
                }
                else if (item.Key.StartsWith("enl"))
                {
                    SetEnlightenedScore(cp, item.Value.PropertyType == EdmType.String ? int.Parse(item.Value.StringValue) : item.Value.Int32Value.Value);
                }
                else if (item.Key.StartsWith("kud"))
                {
                    SetKudos(cp, item.Value.StringValue);
                }
            }
        }

        private void SetResistanceScore(int cp, int resistanceScore)
        {
            if (!_scores.TryGetValue(cp, out var cpScore))
            {
                cpScore = new CpScore(resistanceScore, 0,null);
                _scores.Add(cp, cpScore);
                return;
            }
            _scores[cp] = new CpScore(resistanceScore, cpScore.EnlightenedScore, cpScore.Kudos);//kind od a hack, but trying to keep the CpScore immutable
        }

        private void SetEnlightenedScore(int cp, int enlightenedScore)
        {
            if (!_scores.TryGetValue(cp, out var cpScore))
            {
                cpScore = new CpScore(0, enlightenedScore, null);
                _scores.Add(cp, cpScore);
            }
            _scores[cp] = new CpScore(cpScore.ResistanceScore, enlightenedScore, cpScore.Kudos);
        }

        private void SetKudos(int cp, string kudos)
        {
            if (!_scores.TryGetValue(cp, out var cpScore))
            {
                cpScore = new CpScore(0, 0, kudos);
                _scores.Add(cp, cpScore);
                return;
            }
            _scores[cp] = new CpScore(cpScore.ResistanceScore, cpScore.EnlightenedScore,kudos);
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var results = base.WriteEntity(operationContext);
            foreach (var item in _scores)
            {
                results.Add("res" + item.Key, new EntityProperty(item.Value.ResistanceScore));
                results.Add("enl" + item.Key, new EntityProperty(item.Value.EnlightenedScore));
                results.Add("kud" + item.Key, new EntityProperty(item.Value.Kudos));
            }
            results.Add("snooze", new EntityProperty(_isSnoozed));

            return results;
        }

        public void SaveScores(int checkpoint, CpScore cpScore)
        {
            _scores[checkpoint] = cpScore;
        }

        public void SetSnooze(bool isSnoozed)
        {
            _isSnoozed = isSnoozed;
        }
    }
}