using System;

namespace Upchurch.Ingress.Domain
{
    public class CheckPoint
    {
        private readonly DateTime _zeroTime = new DateTime(2015, 6, 2, 13, 0, 0, DateTimeKind.Utc);
        public int CP { get; private set; }

        public CheckPoint(DateTime dateTime)
        {
            var timesincezero = dateTime.Subtract(_zeroTime);
            var cps = (timesincezero.Days*24 + timesincezero.Hours)/5;

            CP = cps%35;

            //rather than say this is CP 0 of cycle 2, consider it CP 35 of cycle 1
            if (CP == 0)
            {
                CP = 35;
                Cycle = new CycleIdentifier((cps / 35) - 1);
            }
            else
            {
                Cycle = new CycleIdentifier(cps / 35);
            }
        }

        public CheckPoint(CycleIdentifier cycle, int cp)
        {
            Cycle = cycle;
            CP = cp;
            
        }

        public DateTime DateTime
        {
            get
            {
                var hours = CP * 5 + Cycle.Id * 35 * 5;
                return _zeroTime.AddHours(hours);
            }
        }

        public CycleIdentifier Cycle { get; private set; }

        public static CheckPoint Current()
        {
            return new CheckPoint(DateTime.UtcNow);
        }
    }
}