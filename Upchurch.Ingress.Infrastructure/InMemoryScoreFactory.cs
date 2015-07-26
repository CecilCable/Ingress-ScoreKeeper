using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Upchurch.Ingress.Domain;

namespace Upchurch.Ingress.Infrastructure
{
    public class InMemoryScoreFactory : ICycleScoreUpdater
    {
        private readonly IDictionary<int, CycleScore> _cycleScores;
        private readonly IDictionary<int, DateTimeOffset> _timeStamps;
        public InMemoryScoreFactory()
        {
            _cycleScores = new ConcurrentDictionary<int, CycleScore>();
            _timeStamps = new ConcurrentDictionary<int, DateTimeOffset>();
            
        }

        public CycleScore GetScoreForCycle(CycleIdentifier cycle)
        {
            CycleScore cycleScore;
            if (!_cycleScores.TryGetValue(cycle.Id, out cycleScore))
            {
                var timestamp = DateTimeOffset.Now;
                cycleScore = new CycleScore(cycle, timestamp);
                _timeStamps[cycle.Id] = timestamp;
                _cycleScores[cycle.Id] = cycleScore;
            }
            return cycleScore;
        }

        public bool UpdateScore(CycleIdentifier cycle, DateTimeOffset dateTimeOffset, ICollection<CpScore> toArray)
        {
            if (_timeStamps[cycle.Id] == dateTimeOffset)
            {
                //shouldn't need to update _cycleScores since the CycleScore is updated elsewhere. This method is for persisting somewhere
                _timeStamps[cycle.Id] = DateTimeOffset.Now;
                return true;
            }
            return false;
        }

    }
}