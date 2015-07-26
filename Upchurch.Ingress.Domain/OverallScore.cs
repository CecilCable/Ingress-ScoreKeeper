using System.Collections.Generic;
using System.Linq;

namespace Upchurch.Ingress.Domain
{
    public class OverallScore
    {
        private readonly int _cps;
        private readonly CpScore _lastCpScore;

        public int EnlightenedScore
        {
            get
            {
                if (_cps == 0)
                {
                    return 0;
                }
                return EnlightenedScoreTotal/_cps;
            }
        }

        public int CheckPointsLeft {
            get { return 35 - _cps; }
        }
        public int EnlightenedScoreTotal { get; private set; }
        public int ResistanceScoreTotal { get; private set; }

        public int ResistanceScore
        {
            get
            {
                if (_cps == 0)
                {
                    return 0;
                }
                return ResistanceScoreTotal/_cps;
            }
        }

        public CpScore LastCpScore()
        {
            return _lastCpScore;
        }

        public OverallScore(ICollection<CpScore> cpScores)
        {
            _cps = cpScores.Count;
            EnlightenedScoreTotal = cpScores.Sum(item => item.EnlightenedScore);
            ResistanceScoreTotal = cpScores.Sum(item => item.ResistanceScore);
            _lastCpScore = _cps > 0 ? cpScores.OrderByDescending(item => item.Cp).First() : null;
        }

        public FinalScoreProjection FinalScoreProjection()
        {

            return new FinalScoreProjection(this, _lastCpScore);
            
        }
        private int? CPsToLeadChange(CpScore lastCpScore)
        {
            if (lastCpScore.EnlightenedScore == lastCpScore.ResistanceScore)
            {
                return null;
            }
            if (lastCpScore.EnlightenedScore > lastCpScore.ResistanceScore && EnlightenedScoreTotal < ResistanceScoreTotal)
            {
                var gainEachCp = lastCpScore.EnlightenedScore - lastCpScore.ResistanceScore;
                var amountNeededToGain = ResistanceScoreTotal - EnlightenedScoreTotal;
                return CPsToLeadChange(gainEachCp, amountNeededToGain);
            }
            if (lastCpScore.ResistanceScore > lastCpScore.EnlightenedScore && ResistanceScoreTotal < EnlightenedScoreTotal)
            {
                var gainEachCp = lastCpScore.ResistanceScore - lastCpScore.EnlightenedScore;
                var amountNeededToGain = EnlightenedScoreTotal - ResistanceScoreTotal;
                return CPsToLeadChange(gainEachCp, amountNeededToGain);
            }
            return null;
        }

        private static int? CPsToLeadChange(int gainEachCp, int amountNeededToGain)
        {
            var cps = 0;
            do
            {
                cps++;
            } while (cps*gainEachCp < amountNeededToGain);
            return cps;
        }
    }
}