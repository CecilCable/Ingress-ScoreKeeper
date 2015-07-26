using System;
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
            var response = new SendMessageToSlack("http://localhost").Send("Test Message");
            Console.WriteLine(response.StatusCode);
            Console.WriteLine(response.Content);
        }
    }
}
