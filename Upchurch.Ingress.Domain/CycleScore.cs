using System.Collections.Generic;
using System.Linq;
using Upchurch.Ingress.Domain.Intel;

namespace Upchurch.Ingress.Domain
{
     

    public class CycleScore
    {
        private readonly IDictionary<int, CpScore> _scores;
        private readonly long _timestampTicks;

        public IEnumerable<CpStatus> AllCPs()
        {
            for (var i = 1; i <= 35; i++)
            {
                CpScore score;
                if (_scores.TryGetValue(i, out score))
                {
                    yield return new RecordedScore(score,Cycle,i);
                    continue;
                }
                if (i <= _maxCheckPoint)
                {
                    yield return new MissingScore(i, Cycle);
                    continue;
                    //return MissingCps;
                }
                yield return new FutureScore(i,Cycle);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="displayLastCp"></param>
        /// <returns></returns>
        public IEnumerable<string> Summary(bool displayLastCp)
        {
            var overallScore = OverallScore();
            var latestCpScore = overallScore.LastCpScore;
            if (latestCpScore == null)
            {
                if (_maxCheckPoint == 0)
                {
                    yield return "No scores recorded. Cycle has not started.";
                }
                else
                {
                    yield return "No scores recorded.";
                }
                
                yield break;
            }
            var finalScoreProjection = overallScore.FinalScoreProjection();
            var checkpointsLeft = overallScore.CheckPointsLeft;
            if (IsSnoozed)
            {
                yield return "Scoring is snoozed.";
            }
            if (displayLastCp && overallScore.LastCp!=0)
            {
                yield return $"CP {overallScore.LastCp}: Enlightened:{latestCpScore.EnlightenedScore:n0} Resistance:{latestCpScore.ResistanceScore:n0}";
            }
            //tie game. So unlikely to happen. Except after the 35th checkpoint.
            if (overallScore.EnlightenedScore == overallScore.ResistanceScore)
            {
                //overallScore.ResistanceScoreTotal == overallScore.EnlightenedScoreTotal check this instead ???
                yield return $"The score is tied {overallScore.EnlightenedScore:n0} to {overallScore.ResistanceScore:n0}";
                if (checkpointsLeft == 1)
                {
                    yield return "Who ever gets more MUs in the last CP wins the cycle.";
                }
                else
                {
                    if (latestCpScore.ResistanceScore > latestCpScore.EnlightenedScore)
                    {
                        yield return "We won the last CP.";
                    }
                    else if (latestCpScore.ResistanceScore < latestCpScore.EnlightenedScore)
                    {
                        yield return "Enlightened won the last CP.";
                    }

                    yield return $"Who ever gets more MUs in the last {checkpointsLeft} CPs wins the cycle.";
                }

                yield break;
            }
            if (checkpointsLeft == 0)
            {
                if (overallScore.EnlightenedScore > overallScore.ResistanceScore)
                {
                    yield return $"We lost the cycle {overallScore.ResistanceScore:n0} to {overallScore.EnlightenedScore:n0}. A field of {overallScore.EnlightenedScoreTotal- overallScore.ResistanceScoreTotal:n0} MUs would have won the cycle.";
                }
                if (overallScore.EnlightenedScore < overallScore.ResistanceScore)
                {
                    yield return $"We won the cycle {overallScore.ResistanceScore:n0} to {overallScore.EnlightenedScore:n0}!. A field of {overallScore.ResistanceScoreTotal- overallScore.EnlightenedScoreTotal :n0} MUs would have won the cycle for the Enlightened.";
                }
                yield break;

            }
            

            //resistance winning
            if (overallScore.ResistanceScoreTotal > overallScore.EnlightenedScoreTotal)
            {
                yield return $"We are winning the cycle {overallScore.ResistanceScore:n0} to {overallScore.EnlightenedScore:n0}";
                if (checkpointsLeft == 1)
                {
                    yield return $"Enlightened would need to beat us in the last CP by {overallScore.ResistanceScoreTotal - overallScore.EnlightenedScoreTotal:n0} MUs to win the cycle";
                }
                else
                {
                    yield return $"Enlightened would need beat our score by {overallScore.ResistanceScoreTotal - overallScore.EnlightenedScoreTotal:n0} MUs in 1 CP or beat us by {(overallScore.ResistanceScoreTotal - overallScore.EnlightenedScoreTotal)/checkpointsLeft:n0} MUs in the remaining {checkpointsLeft} CPs to win";


                    if (finalScoreProjection.CpsToLeadChange.HasValue)
                    {
                        yield return $"If cp scores remain unchanged the score at the end of the cycle will be Enlightened {finalScoreProjection.FinalEnlightenedScore:n0} Resistance {finalScoreProjection.FinalResistanceScore:n0} with the lead changing in {finalScoreProjection.CpsToLeadChange.Value} CPs";
                    }
                    else
                    {
                        yield return $"If cp scores remain unchanged the score at the end of the cycle will be Enlightened {finalScoreProjection.FinalEnlightenedScore:n0} Resistance {finalScoreProjection.FinalResistanceScore:n0}";
                    }
                }
                yield break;
            }
            
            //enlightened winning
            //resistanceScoreTotal < enlightenedScoreTotal
            yield return $"We are Losing the cycle {overallScore.ResistanceScore:n0} to {overallScore.EnlightenedScore:n0}";
            if (checkpointsLeft == 1)
            {
                yield return $"We need to beat the enlightened score by {overallScore.EnlightenedScoreTotal - overallScore.ResistanceScoreTotal:n0} MUs in the last CP";
            }
            else
            {
                yield return $"We need to beat the enlightened by {overallScore.EnlightenedScoreTotal - overallScore.ResistanceScoreTotal:n0} MUs in 1 CP or beat them by {(overallScore.EnlightenedScoreTotal - overallScore.ResistanceScoreTotal)/checkpointsLeft:n0} MUs in the remaining {checkpointsLeft} CPs to win";
                if (finalScoreProjection.CpsToLeadChange.HasValue)
                {
                    yield return $"If cp scores remain unchanged the score at the end of the cycle will be Enlightened {finalScoreProjection.FinalEnlightenedScore:n0} Resistance {finalScoreProjection.FinalResistanceScore:n0} with the lead changing in {finalScoreProjection.CpsToLeadChange.Value} CPs";
                }
                else
                {
                    yield return $"If cp scores remain unchanged the score at the end of the cycle will be Enlightened {finalScoreProjection.FinalEnlightenedScore:n0} Resistance {finalScoreProjection.FinalResistanceScore:n0}";
                }
            }

        }

        

        /// <summary>
        ///     only set during deserialization
        /// </summary>
        /// <remarks>
        ///     TODO: switch to in game cycle numbering; ie. 2015.24
        /// </remarks>
        public CycleIdentifier Cycle { get; set; }

        public bool IsSnoozed { get; }
        private readonly int _maxCheckPoint;

        /// <summary>
        ///     Cycle with checpoint scores
        /// </summary>
        /// <param name="cycleIdentifier"></param>
        /// <param name="timestampTicks"></param>
        /// <param name="isSnoozed"></param>
        /// <param name="currentCheckPoint"></param>
        /// <param name="scores"></param>
        public CycleScore(CycleIdentifier cycleIdentifier, long timestampTicks, bool isSnoozed, CheckPoint currentCheckPoint, params KeyValuePair<int, CpScore>[] scores)
        {
            _timestampTicks = timestampTicks;
            Cycle = cycleIdentifier;
            IsSnoozed = isSnoozed;
            _scores = scores.ToDictionary(score => score.Key, val=>val.Value);


            if (Cycle.Id > currentCheckPoint.Cycle.Id)
            {
                _maxCheckPoint = 0;
            }
            else if (Cycle.Id == currentCheckPoint.Cycle.Id)
            {
                _maxCheckPoint = currentCheckPoint.CP;
            }
            else
            {
                _maxCheckPoint = 35;//past cycles
            }
        }


        public bool HasMissingCPs()
        {
            return AllCPs().Any(item => item.GetType() == typeof(MissingScore));
        }
        public IEnumerable<CpStatus> MissingCPs()
        {
            return AllCPs().Where(item => item.GetType() == typeof(MissingScore));
        }

        public bool HasExactScore(int cp, UpdateScore newScore)
        {
            CpScore existingScore;
            if (!_scores.TryGetValue(cp, out existingScore))
            {
                return false;
            }
            if (existingScore.EnlightenedScore == newScore.EnlightenedScore && existingScore.ResistanceScore == newScore.ResistanceScore)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        ///     return reason not updatable
        /// </summary>
        /// <param name="cpScore"></param>
        /// <param name="checkpoint"></param>
        /// <returns></returns>
        public string IsUpdatable(UpdateScore cpScore, int checkpoint)
        {
            if (checkpoint > _maxCheckPoint)
            {
                //only check this if the cycle is the same, else it's probably a previous cycle? Add some testing to explore this.
                return "Cannot set a checkpoint that is in the future";
            }
            if (HasExactScore(checkpoint,cpScore))
            {
                return "has exact score";
            }

            if (long.Parse(cpScore.TimeStamp) != _timestampTicks)
            {
                return "timestamp invalid";
            }
            return null;
        }

        public string IsUpdatable(Result score, long timeStamp)
        {
            if (score.regionName != "AM02-KILO-00")
            {
                return "scores are not for the local region";
            }
            if (timeStamp != _timestampTicks)
            {
                return "timestamp invalid";
            }
            return null;
        }

        public OverallScore OverallScore()
        {
            if (_scores.Count == 0)
            {
                return new OverallScore();
            }
            var lastCp = _scores.Keys.Max();
            var latestCp = _scores[lastCp];
            return new OverallScore(_scores.Values, latestCp, lastCp);
        }

        /// <summary>
        ///     could return some other object, but i need a cycle score and a timestamp and I'm lazy
        /// </summary>
        /// <param name="checkpoint"></param>
        /// <returns></returns>
        public UpdateScore ScoreForCheckpoint(int checkpoint)
        {
            CpScore cpScore;
            if (_scores.TryGetValue(checkpoint, out cpScore))
            {
                return new UpdateScore(cpScore, _timestampTicks);
            }
            return new UpdateScore(_timestampTicks);
        }


        /// <summary>
        /// </summary>
        /// <param name="checkpoint"></param>
        /// <param name="updateScore"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        public bool SetScore(int checkpoint, UpdateScore updateScore, ICycleScoreUpdater update)
        {
            var cpScore = new CpScore(updateScore.ResistanceScore.Value, updateScore.EnlightenedScore.Value, updateScore.Kudos);
            _scores[checkpoint] = cpScore;
            //this persists it
            return update.UpdateScore(Cycle, checkpoint, long.Parse(updateScore.TimeStamp), cpScore);
        }

        public bool SetScore(IEnumerable<KeyValuePair<int, CpScore>> scores, long timeStamp, ICycleScoreUpdater update)
        {
            var updatedValues = new List<KeyValuePair<int, CpScore>>();

            foreach (var score in scores)
            {
                CpScore oldscore;
                if (_scores.TryGetValue(score.Key, out oldscore))
                {
                    if (oldscore.EnlightenedScore == score.Value.EnlightenedScore && oldscore.ResistanceScore == score.Value.ResistanceScore)
                    {
                        continue;
                    }
                }
                
                _scores[score.Key] = score.Value;
                updatedValues.Add(score);
            }
            if (updatedValues.Count == 0)
            {
                return false;
            }
            
            //this persists it
            return update.UpdateScore(Cycle, timeStamp, updatedValues.ToArray());
        }

        public override string ToString()
        {
            return string.Join("\n", Summary(true));
        }


        public bool SetSnooze(bool isSnooze, ICycleScoreUpdater scoreUpdater)
        {
            return scoreUpdater.SetSnooze(Cycle, _timestampTicks, isSnooze);
        }
    }
}