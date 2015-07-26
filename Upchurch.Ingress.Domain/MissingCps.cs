using System.Collections.Generic;

namespace Upchurch.Ingress.Domain
{
    public class MissingCps
    {
        public IList<CpScore> Cps;
        public string TimeStamp { get; set; }
        public MissingCps()
        {
            Cps = new List<CpScore>();
        }
        public MissingCps(long timeStamp):this()
        {
            TimeStamp = timeStamp.ToString();
        }
    }
}
