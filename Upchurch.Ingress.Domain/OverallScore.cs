using System.Collections.Generic;
using System.Linq;

namespace Upchurch.Ingress.Domain
{
    public class OverallScore
    {
        private readonly int _cps;
       // public int cyclesWon = 0; //can't remember if this is legal in C# or just a java thing

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

        public CpScore LastCpScore { get; private set; }
        public int LastCp { get; private set; }
        

        //no scores recorded
        public OverallScore()
        {
            _cps = 0;
            EnlightenedScoreTotal = 0;
            ResistanceScoreTotal = 0;
            LastCp = 0;
            LastCpScore = null;
            
        }
        public OverallScore(ICollection<CpScore> cpScores, CpScore latestCpScore, int latestCp)
        {
            _cps = cpScores.Count;
            EnlightenedScoreTotal = cpScores.Sum(item => item.EnlightenedScore);
            ResistanceScoreTotal = cpScores.Sum(item => item.ResistanceScore);
            LastCpScore = latestCpScore;
            LastCp = latestCp;
        }

        public FinalScoreProjection FinalScoreProjection()
        {

            return new FinalScoreProjection(this, LastCpScore);
            
        }
    }
}