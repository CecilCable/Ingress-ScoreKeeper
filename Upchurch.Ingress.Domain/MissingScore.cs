namespace Upchurch.Ingress.Domain
{
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
}