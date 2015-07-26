using System;
using System.Collections.Generic;
using System.Linq;

namespace Upchurch.Ingress.Domain
{
    public class CycleScore
    {
        private readonly IDictionary<int, CpScore> _scores;
        private readonly DateTimeOffset _timestamp;

        public ICollection<CpScore> Scores
        {
            get { return _scores.Values; }
        }

        /// <summary>
        /// if latestCPScore isn't provider it will be looked up
        /// </summary>
        /// <param name="latestCpScore"></param>
        /// <returns></returns>
        public IEnumerable<string> Summary(CpScore latestCpScore = null)
        {
            var checkpointsLeft = CheckPointsLeft();
            if (latestCpScore == null)
            {
                latestCpScore = LatestCpScore();
            }
            var overallScore = OverallScore();
            if (overallScore.ResistanceScoreTotal == overallScore.EnlightenedScoreTotal)
            {
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
            var cps = overallScore.CPsToLeadChange(latestCpScore);

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
                    if (cps.HasValue)
                    {
                        yield return string.Format("If cp scores remain unchanged the enlightened will be winning in {0} CPs", cps.Value);
                    }
                }
                yield break;
            }
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
                if (cps.HasValue)
                {
                    yield return string.Format("If cp scores remain unchanged we will be winning in {0} CPs", cps.Value);
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

        /// <summary>
        ///     Cycle with checpoint scores
        /// </summary>
        /// <param name="cycleIdentifier"></param>
        /// <param name="timestamp"></param>
        /// <param name="scores"></param>
        public CycleScore(CycleIdentifier cycleIdentifier, DateTimeOffset timestamp, params CpScore[] scores)
        {
            _timestamp = timestamp;
            Cycle = cycleIdentifier;
            _scores = scores.ToDictionary(score => score.Cp);
        }

        public MissingCps CurrentMissingCps()
        {
            var currentCp = CheckPoint.Current();
            var cpMax = Cycle.Id == currentCp.Cycle.Id ? currentCp.CP : 35;
            var missing = new MissingCps(_timestamp.Ticks);
            for (var i = 1; i <= cpMax; i++)
            {
                if (_scores.ContainsKey(i))
                {
                    continue;
                }
                missing.Cps.Add(new CpScore(i, 0, 0));
            }
            return missing;
        }

        public bool HasExactScore(CpScore newScore)
        {
            CpScore existingScore;
            if (!_scores.TryGetValue(newScore.Cp, out existingScore))
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
        /// <param name="timestamp"></param>
        /// <param name="currentCheckPoint"></param>
        /// <returns></returns>
        public string IsUpdatable(CpScore cpScore, DateTimeOffset timestamp, CheckPoint currentCheckPoint)
        {
            if (currentCheckPoint.Cycle.Id == Cycle.Id && cpScore.Cp > currentCheckPoint.CP)
            {
                //only check this if the cycle is the same, else it's probably a previous cycle? Add some testing to explore this.
                return "Cannot set a checkpoint that is in the future";
            }
            if (HasExactScore(cpScore))
            {
                return "has exact score";
            }
            if (cpScore.Cp < 1)
            {
                return "CP can't be before 1";
            }
            if (timestamp.Ticks != _timestamp.Ticks)
            {
                return "timestamp invalid";
            }
            return null;
        }

        private int CheckPointsLeft()
        {
            return 35 - _scores.Count;
        }

        public OverallScore OverallScore()
        {
            return new OverallScore(Scores);
        }

        /// <summary>
        ///     could return some other object, but i need a cycle score and a timestamp and I'm lazy
        /// </summary>
        /// <param name="checkpoint"></param>
        /// <returns></returns>
        public MissingCps ScoreForCheckpoint(int checkpoint)
        {
            var missingCp = new MissingCps(_timestamp.Ticks);
            CpScore cpScore;
            if (!_scores.TryGetValue(checkpoint, out cpScore))
            {
                cpScore = new CpScore(checkpoint, 0, 0);
            }
            missingCp.Cps.Add(cpScore);
            return missingCp;
        }

        /// <summary>
        /// </summary>
        /// <param name="cpScore"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        public bool SetScore(CpScore cpScore, ICycleScoreUpdater update)
        {
            _scores[cpScore.Cp] = cpScore;
            //this saves it
            return update.UpdateScore(Cycle, _timestamp, Scores.ToArray());
        }

        public override string ToString()
        {
            var latestCpScore = LatestCpScore();
            var lastCpDescription = new[]
            {
                string.Format("CP {0}: Enlightened:{1:n0} Resistance:{2:n0}", latestCpScore.Cp, latestCpScore.EnlightenedScore, latestCpScore.ResistanceScore)
            };
            return string.Join("\n", lastCpDescription.Union(Summary(latestCpScore)));
        }

        private CpScore LatestCpScore()
        {
            return _scores.Count != 0 ? _scores[_scores.Keys.Max()] : new CpScore();
        }

    
    }
}