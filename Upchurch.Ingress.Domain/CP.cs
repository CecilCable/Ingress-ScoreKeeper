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
            Cycle = new CycleIdentifier(cps/35);
        }

        public CycleIdentifier Cycle { get; private set; }

        public static CheckPoint Current()
        {
            return new CheckPoint(DateTime.UtcNow);
        }
    }
}