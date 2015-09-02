namespace Upchurch.Ingress.Domain
{
    /// <summary>
    /// This class is posted via UI. So keep it simple
    /// </summary>
    public class CpScore
    {
        public int ResistanceScore { get; private set; }
        public int EnlightenedScore { get; private set; }

        public string Kudos { get; private set; }

        public CpScore(int resistanceScore, int enlightenedScore, string kudos)
        {
            //Cp = cp;
            ResistanceScore = resistanceScore;
            EnlightenedScore = enlightenedScore;
            Kudos = kudos;
        }

        public override string ToString()
        {
            return string.Format("Enlightened:{0} Resistance:{1}", ResistanceScore, EnlightenedScore);
        }
    }
}