namespace Upchurch.Ingress.Domain
{
    public class CpScore
    {
        public int Cp { get; set; }
        public int ResistanceScore { get; set; }
        public int EnlightenedScore { get; set; }

        public CpScore(int cp, int resistanceScore, int enlightenedScore)
        {
            Cp = cp;
            ResistanceScore = resistanceScore;
            EnlightenedScore = enlightenedScore;
        }

        /// <summary>
        ///     this only exists for serialization
        /// </summary>
        private CpScore()
        {
        }

        public override string ToString()
        {
            return string.Format("CP:{0} Enlightened:{1} Resistance:{2}", Cp, ResistanceScore, EnlightenedScore);
        }
    }
}