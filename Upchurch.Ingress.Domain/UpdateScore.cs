namespace Upchurch.Ingress.Domain
{
    public class UpdateScore
    {
        public UpdateScore(CpScore cpScore, long timeStamp):this(timeStamp)
        {
            TimeStamp = timeStamp.ToString();
            ResistanceScore = cpScore.ResistanceScore;
            EnlightenedScore = cpScore.EnlightenedScore;
            Kudos = cpScore.Kudos;
        }
        public UpdateScore(long timeStamp)
        {
            TimeStamp = timeStamp.ToString();
        }

        public UpdateScore()
        {
            
        }

        public int? EnlightenedScore { get; set; }
        public int? ResistanceScore { get; set; }
        public string TimeStamp { get; set; }

        public string Kudos { get; set; }
    }
}