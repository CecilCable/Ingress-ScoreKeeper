namespace Upchurch.Ingress.Domain
{
    public class FutureScore : CpStatus
    {
        public FutureScore(int cp, CycleIdentifier cycleIdentifier)
            : base(cycleIdentifier, cp)
        {

            
        }
    }
}