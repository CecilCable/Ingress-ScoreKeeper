namespace Upchurch.Ingress.Domain
{
    public interface IScraperService
    {
        IScraperMeta GetData();

        void SetMeta(IScraperMeta scraperMeta);
    }
}