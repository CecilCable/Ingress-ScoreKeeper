using System;

namespace Upchurch.Ingress.Domain
{
    public abstract class CpStatus
    {
        protected CpStatus(CycleIdentifier cycle, int cp)
        {
            Cp = cp;
            //var localTime = new CheckPoint(cycle, cp).DateTime.ToLocalTime();
            //DateTime = localTime.ToShortDateString() + " " + localTime.ToShortTimeString();
            DateTime = new CheckPoint(cycle, cp).DateTime;
            Type = GetType().ToString();
        }

        public int Cp { get; private set; }
        public DateTime DateTime { get; private set; }
        public string Type { get; private set; }
        
    }
}