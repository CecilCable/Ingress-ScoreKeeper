using System;

namespace Upchurch.Ingress.Domain
{
    /// <summary>
    /// This class is posted via UI. So keep it simple
    /// </summary>
    public class CpScore
    {
        //public int Cp { get; set; }
        public int ResistanceScore { get; private set; }
        public int EnlightenedScore { get; private set; }

        public string Kudos { get; set; }

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

    public abstract class CpStatus
    {
        protected CpStatus(CycleIdentifier cycle, int cp)
        {
            Cp = cp;
            //var localTime = new CheckPoint(cycle, cp).DateTime.ToLocalTime();
            //DateTime = localTime.ToShortDateString() + " " + localTime.ToShortTimeString();
            DateTime = new CheckPoint(cycle, cp).DateTime;
            Type = this.GetType().ToString();
        }

        public int Cp { get; private set; }
        public DateTime DateTime { get; private set; }
        public string Type { get; private set; }
        
    }
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
    /// <summary>
    /// Using this for updating an existing score
    /// </summary>
    public class MissingScore : CpStatus
    {


        public MissingScore(int cp, CycleIdentifier cycleIdentifier)
            : base(cycleIdentifier, cp)
        {
            

        }

    }

    public class FutureScore : CpStatus
    {
        public FutureScore(int cp, CycleIdentifier cycleIdentifier)
            : base(cycleIdentifier, cp)
        {

            
        }
    }
}