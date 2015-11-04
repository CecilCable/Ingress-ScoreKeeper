using Microsoft.VisualStudio.TestTools.UnitTesting;
using Upchurch.Ingress.Infrastructure;

namespace Upchurch.Ingress.Tests
{
    [TestClass]
    public class SendMessageToSlackTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            new SendMessageToSlack("http://localhost").Send("Test Message");
            
        }
    }
}
