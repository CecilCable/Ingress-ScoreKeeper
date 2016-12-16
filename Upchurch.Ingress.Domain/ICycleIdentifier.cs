namespace Upchurch.Ingress.Domain
{
    public class CycleIdentifier
    {

        public CycleIdentifier(int id)
        {
            Id = id;
        }

        public CycleIdentifier(int year, int cp)
        {
            //2024 has 51 CPs. So pleanty of time to figure out better calculation that year-2015*50
            Id = cp - 21 + (year - 2015) * 50;

        }

        public int Id { get; private set; }
    }
}