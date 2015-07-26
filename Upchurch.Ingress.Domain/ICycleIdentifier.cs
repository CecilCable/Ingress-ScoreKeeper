using System.Security.Cryptography.X509Certificates;

namespace Upchurch.Ingress.Domain
{
    public class CycleIdentifier
    {

        public CycleIdentifier(int id)
        {
            Id = id;
        }

        public int Id { get; private set; }
    }
}