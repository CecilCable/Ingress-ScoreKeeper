using System;

namespace Upchurch.Ingress.Domain
{
    public class FinalScoreProjection
    {
        public FinalScoreProjection(OverallScore overallScore, CpScore latestCheckpoint)
        {
            if (latestCheckpoint == null)
            {
                throw new ArgumentNullException("latestCheckpoint");
            }

            var cpsleft = overallScore.CheckPointsLeft;
            
            FinalEnlightenedScore = (overallScore.EnlightenedScoreTotal + latestCheckpoint.EnlightenedScore*cpsleft)/35;
            FinalResistanceScore = (overallScore.ResistanceScoreTotal + latestCheckpoint.ResistanceScore*cpsleft)/35;

            if (FinalEnlightenedScore > FinalResistanceScore ^ overallScore.EnlightenedScoreTotal > overallScore.ResistanceScoreTotal)
            {
                CpsToLeadChange = CalculateCPsToLeadChange(latestCheckpoint, overallScore.EnlightenedScoreTotal, overallScore.ResistanceScoreTotal);
            }
        }

        public int? CpsToLeadChange { get; private set; }

        private static int? CalculateCPsToLeadChange(CpScore lastCpScore, int enlightenedScoreTotal, int resistanceScoreTotal)
        {
            var gainEachCp = Math.Abs(lastCpScore.EnlightenedScore - lastCpScore.ResistanceScore);
            var amountNeededToGain = Math.Abs(resistanceScoreTotal - enlightenedScoreTotal);
            return CPsToLeadChange(gainEachCp, amountNeededToGain);
            
        }

        private static int? CPsToLeadChange(int gainEachCp, int amountNeededToGain)
        {
            var cps = 0;
            do
            {
                cps++;
            } while (cps * gainEachCp < amountNeededToGain);
            return cps;
        }

        public int FinalEnlightenedScore { get; private set; }
        public int FinalResistanceScore { get; private set; }
    }
}
