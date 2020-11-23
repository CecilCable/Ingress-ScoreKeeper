using System.Dynamic;

namespace Upchurch.Ingress.Domain
{
    public interface IScraperMeta
    {
        string Version { get; set; }
        string Token { get; set; }
        string Session { get; set; }
    }

    public class ScraperMeta: IScraperMeta
    {
        public string Version { get; set; }
        public string Token { get; set; }
        public string Session { get; set; }
    }
}
