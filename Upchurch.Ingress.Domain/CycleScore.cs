using System;
using System.Collections.Generic;
using System.Linq;

namespace Upchurch.Ingress.Domain
{
     

    public class CycleScore
    {
        private readonly IDictionary<int, CpScore> _scores;
        private readonly long _timestampTicks;

        public IEnumerable<CpStatus> AllCPs()
        {
            var currentCp = CheckPoint.Current();
            int cpMax;
            if (Cycle.Id == currentCp.Cycle.Id)
            {
                cpMax = currentCp.CP;
            }
            else if (Cycle.Id < currentCp.Cycle.Id)
            {
                cpMax = 35;
            }
            else
            {
                cpMax = 0;
            }

            for (var i = 1; i <= 35; i++)
            {
                CpScore score;
                if (_scores.TryGetValue(i, out score))
                {
                    yield return new RecordedScore(score,Cycle,i);
                    continue;
                }
                if (i <= cpMax)
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
                yield return "No scores recorded. Cycle has not started.";
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
                yield return string.Format("CP {0}: Enlightened:{1:n0} Resistance:{2:n0}", overallScore.LastCp, latestCpScore.EnlightenedScore, latestCpScore.ResistanceScore);
            }
            //tie game. So unlikely to happen. Except after the 35th checkpoint.
            if (overallScore.EnlightenedScore == overallScore.ResistanceScore)
            {
                //overallScore.ResistanceScoreTotal == overallScore.EnlightenedScoreTotal check this instead ???
                yield return string.Format("The score is tied {0:n0} to {1:n0}", overallScore.EnlightenedScore, overallScore.ResistanceScore);
                if (checkpointsLeft == 1)
                {
                    yield return "Who ever gets more MUs in the last CP wins the cycle.";
                }
                else
                {
                    if (latestCpScore.ResistanceScore > latestCpScore.EnlightenedScore)
                    {
                        yield return string.Format("We won the last CP.");
                    }
                    else if (latestCpScore.ResistanceScore < latestCpScore.EnlightenedScore)
                    {
                        yield return string.Format("Enlightened won the last CP.");
                    }

                    yield return string.Format("Who ever gets more MUs in the last {0} CPs wins the cycle.", checkpointsLeft);
                }

                yield break;
            }
            if (checkpointsLeft == 0)
            {
                if (overallScore.EnlightenedScore > overallScore.ResistanceScore)
                {
                  //  overallScore.cyclesWon--;
                    yield return string.Format("We lost the cycle {0:n0} to {1:n0}", overallScore.ResistanceScore, overallScore.EnlightenedScore/*, overallScore.cyclesWon*-1*/);
                }
                if (overallScore.EnlightenedScore < overallScore.ResistanceScore)
                {
                    //overallScore.cyclesWon++;
                    yield return string.Format("We won the cycle {0:n0} to {1:n0}!", overallScore.ResistanceScore, overallScore.EnlightenedScore/*, overallScore.cyclesWon*/);
                }
                yield break;

            }
            

            //resistance winning
            if (overallScore.ResistanceScoreTotal > overallScore.EnlightenedScoreTotal)
            {
                yield return string.Format("We are winning the cycle {0:n0} to {1:n0}", overallScore.ResistanceScore, overallScore.EnlightenedScore);
                if (checkpointsLeft == 1)
                {
                    yield return string.Format("Enlightened would need to beat us in the last CP by {0:n0} MUs to win the cycle", overallScore.ResistanceScoreTotal - overallScore.EnlightenedScoreTotal);
                }
                else
                {
                    yield return string.Format("Enlightened would need beat our score by {0:n0} MUs in 1 CP or beat us by {1:n0} MUs in the remaining {2} CPs to win"
                                               , overallScore.ResistanceScoreTotal - overallScore.EnlightenedScoreTotal
                                               , (overallScore.ResistanceScoreTotal - overallScore.EnlightenedScoreTotal)/checkpointsLeft
                                               , checkpointsLeft);


                    if (finalScoreProjection.CpsToLeadChange.HasValue)
                    {
                        yield return string.Format("If cp scores remain unchanged the score at the end of the cycle will be Enlightened {0:n0} Resistance {1:n0} with the lead changing in {2} CPs",
                                                   finalScoreProjection.FinalEnlightenedScore,
                                                   finalScoreProjection.FinalResistanceScore,
                                                   finalScoreProjection.CpsToLeadChange.Value);
                    }
                    else
                    {
                        yield return string.Format("If cp scores remain unchanged the score at the end of the cycle will be Enlightened {0:n0} Resistance {1:n0}",
                                                   finalScoreProjection.FinalEnlightenedScore,
                                                   finalScoreProjection.FinalResistanceScore);
                    }
                }
                yield break;
            }
            
            //enlightened winning
            //resistanceScoreTotal < enlightenedScoreTotal
            yield return string.Format("We are Losing the cycle {0:n0} to {1:n0}", overallScore.ResistanceScore, overallScore.EnlightenedScore);
            if (checkpointsLeft == 1)
            {
                yield return string.Format("We need to beat the enlightened score by {0:n0} MUs in the last CP", overallScore.EnlightenedScoreTotal - overallScore.ResistanceScoreTotal);
            }
            else
            {
                yield return string.Format("We need to beat the enlightened by {0:n0} MUs in 1 CP or beat them by {1:n0} MUs in the remaining {2} CPs to win"
                                           , overallScore.EnlightenedScoreTotal - overallScore.ResistanceScoreTotal
                                           , (overallScore.EnlightenedScoreTotal - overallScore.ResistanceScoreTotal)/checkpointsLeft
                                           , checkpointsLeft
                    );
                if (finalScoreProjection.CpsToLeadChange.HasValue)
                {
                    yield return string.Format("If cp scores remain unchanged the score at the end of the cycle will be Enlightened {0:n0} Resistance {1:n0} with the lead changing in {2} CPs",
                                               finalScoreProjection.FinalEnlightenedScore,
                                               finalScoreProjection.FinalResistanceScore,
                                               finalScoreProjection.CpsToLeadChange.Value);
                }
                else
                {
                    yield return string.Format("If cp scores remain unchanged the score at the end of the cycle will be Enlightened {0:n0} Resistance {1:n0}",
                                               finalScoreProjection.FinalEnlightenedScore,
                                               finalScoreProjection.FinalResistanceScore);
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

        public bool IsSnoozed { get; private set; }

        /// <summary>
        ///     Cycle with checpoint scores
        /// </summary>
        /// <param name="cycleIdentifier"></param>
        /// <param name="timestampTicks"></param>
        /// <param name="isSnoozed"></param>
        /// <param name="scores"></param>
        public CycleScore(CycleIdentifier cycleIdentifier, long timestampTicks, bool isSnoozed, params KeyValuePair<int, CpScore>[] scores)
        {
            _timestampTicks = timestampTicks;
            Cycle = cycleIdentifier;
            IsSnoozed = isSnoozed;
            _scores = scores.ToDictionary(score => score.Key, val=>val.Value);
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
        /// <param name="currentCheckPoint"></param>
        /// <returns></returns>
        public string IsUpdatable(UpdateScore cpScore, int checkpoint, CheckPoint currentCheckPoint)
        {
            if (Cycle.Id > currentCheckPoint.Cycle.Id)
            {
                return "Cannot set a cycle that is in the future";
            }
            if (currentCheckPoint.Cycle.Id == Cycle.Id && checkpoint > currentCheckPoint.CP)
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

        public OverallScore OverallScore()
        {
            if (_scores.Count == 0)
            {
                return new OverallScore();
            }
            var lastCP = _scores.Keys.Max();
            var latestCp = _scores[lastCP];
            return new OverallScore(_scores.Values, latestCp, lastCP);
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
        /// <param name="cpScore"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        public bool SetScore(int checkpoint, UpdateScore updateScore, ICycleScoreUpdater update)
        {
            var cpScore = new CpScore(updateScore.ResistanceScore.Value, updateScore.EnlightenedScore.Value, updateScore.Kudos);
            _scores[checkpoint] = cpScore;
            //this saves it
            return update.UpdateScore(Cycle, checkpoint, long.Parse(updateScore.TimeStamp), cpScore);
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