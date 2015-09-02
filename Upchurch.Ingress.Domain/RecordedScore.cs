namespace Upchurch.Ingress.Domain
{
    public class RecordedScore : CpStatus
    {
        public int EnlightenedScore { get; private set; }
        public int ResistanceScore { get; private set; }
        public string Kudos { get; private set; }

        public RecordedScore(CpScore cpScore, CycleIdentifier cycleIdentifier, int cp)
            : base(cycleIdentifier, cp)
        {

            ResistanceScore = cpScore.ResistanceScore;
            EnlightenedScore = cpScore.EnlightenedScore;
            Kudos = cpScore.Kudos;
        }

        
    }
}