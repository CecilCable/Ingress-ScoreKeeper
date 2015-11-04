namespace Upchurch.Ingress.Domain
{
    /// <summary>
    /// This class is posted via UI. So keep it simple
    /// </summary>
    public class CpScore
    {
        public int ResistanceScore { get; }
        public int EnlightenedScore { get; }

        public string Kudos { get; }

        public CpScore(int resistanceScore, int enlightenedScore, string kudos)
        {
            //Cp = cp;
            ResistanceScore = resistanceScore;
            EnlightenedScore = enlightenedScore;
            Kudos = kudos;
        }

        public override string ToString()
        {
            return $"Enlightened:{EnlightenedScore} Resistance:{ResistanceScore}";
        }
    }
}