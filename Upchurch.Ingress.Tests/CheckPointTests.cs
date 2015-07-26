using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Upchurch.Ingress.Domain;

namespace Upchurch.Ingress.Tests
{
    [TestClass]
    public class CheckPointTests
    {
        [TestMethod]
        public void SanityCheck()
        {
            var date1 = DateTime.Now;//new DateTime(2015, 6, 1, 0, 0, 0, DateTimeKind.Local);
            var date2 = DateTime.UtcNow;//new DateTime(2015, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            Assert.AreEqual(144000000000, date2.Ticks - date1.Ticks);
            Assert.AreEqual(date1, date2);
        }
        [TestMethod]
        public void CheckPoint7a()
        {
            var cp = new CheckPoint(new DateTime(2015, 6, 4, 2, 0, 0, DateTimeKind.Utc));//10PM
            Assert.AreEqual(7, cp.CP);
            Assert.AreEqual(0, cp.Cycle);
        }

        [TestMethod]
        public void CheckPoint8()
        {
            var cp = new CheckPoint(new DateTime(2015, 6, 4, 5, 0, 0, DateTimeKind.Utc));//1AM
            Assert.AreEqual(8, cp.CP);
            Assert.AreEqual(0, cp.Cycle);
        }

        [TestMethod]
        public void CheckPoint7b()
        {
            var cp = new CheckPoint(new DateTime(2015, 6, 4, 4, 59, 59, DateTimeKind.Utc));//12:59AM
            Assert.AreEqual(7, cp.CP);
            Assert.AreEqual(0, cp.Cycle);
        }
        [TestMethod]
        public void EndOfCycle()
        {
            var date = new DateTime(2015, 6, 9, 20, 0, 0, DateTimeKind.Utc);//begininng of next cycle
            Console.WriteLine(date);
            var cp = new CheckPoint(date);//1AM
            Assert.AreEqual(0, cp.CP);
            Assert.AreEqual(1, cp.Cycle);
        }
        [TestMethod]
        public void RightBeforeEndOfCycle()
        {
            var date = new DateTime(2015, 6, 9, 19, 59, 59, DateTimeKind.Utc);//begininng of next cycle
            Console.WriteLine(date);
            var cp = new CheckPoint(date);//1AM
            Assert.AreEqual(34, cp.CP);
            Assert.AreEqual(0, cp.Cycle);
        }
    }
}
