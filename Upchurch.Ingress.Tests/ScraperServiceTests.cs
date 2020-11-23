using Microsoft.VisualStudio.TestTools.UnitTesting;
using Upchurch.Ingress.Domain;
using Upchurch.Ingress.Infrastructure;

namespace Upchurch.Ingress.Tests
{
    [TestClass]
    public class ScraperServiceTests
    {
        IScraperService factory = new ScraperService("DefaultEndpointsProtocol=https;AccountName=PASTE");

        [TestMethod]
        public void SetScoreTest()
        {
            
            factory.SetMeta(new ScraperMeta
            {
                Session = "Session2",
                Token = "Token2",
                Version = "Version2"
            });
            
        }

        [TestMethod]
        public void GetDataTest()
        {
            var data = factory.GetData();
            Assert.IsNotNull(data);

        }
    }
}
