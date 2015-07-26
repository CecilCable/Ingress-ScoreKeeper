using Microsoft.VisualStudio.TestTools.UnitTesting;
using Upchurch.Ingress.Domain;
using Upchurch.Ingress.Infrastructure;

namespace Upchurch.Ingress.Tests
{
    [TestClass]
    public class AzureScoreFactoryTests
    {
        [Ignore]
        [TestMethod]
        public void SetScoreTest()
        {
            var factory = new AzureScoreFactory("Paste Connection String Here");

            //179,300	147,311
            var currentCycle = CheckPoint.Current().Cycle;
            var currentCycleScore = factory.GetScoreForCycle(currentCycle);
            currentCycleScore.SetScore(new CpScore(33, 179300, 147311), factory);
        }
    }
}
