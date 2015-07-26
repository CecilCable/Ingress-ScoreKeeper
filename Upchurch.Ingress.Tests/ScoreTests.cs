using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Upchurch.Ingress.Domain;

namespace Upchurch.Ingress.Tests
{
    [TestClass]
    public class ScoreTests
    {
        [TestMethod]
        public void MissingCheckpoints()
        {
            var score = new CycleScore(CheckPoint.Current().Cycle,DateTimeOffset.Now);
            foreach (var cp in score.CurrentMissingCps().Cps)
            {
                Console.WriteLine(cp.Cp);
            }
            score.SetScore(new CpScore(2, 1000, 0), new Mock<ICycleScoreUpdater>().Object);
            foreach (var cp in score.CurrentMissingCps().Cps)
            {
                Console.WriteLine(cp);
            }
        }

        [TestMethod]
        public void CheckMessage()
        {
            var current = CheckPoint.Current();
            var score = new CycleScore(current.Cycle,DateTimeOffset.Now);
            var updater = new Mock<ICycleScoreUpdater>().Object;

            score.SetScore(new CpScore(1, 1000, 0), updater);
            score.SetScore(new CpScore(2, 1000, 0), updater);
            score.SetScore(new CpScore(3, 1000, 0), updater);
            score.SetScore(new CpScore(4, 1000, 0), updater);
            score.SetScore(new CpScore(5, 1000, 0), updater);
            score.SetScore(new CpScore(6, 1000, 0), updater);
            score.SetScore(new CpScore(7, 1000, 0), updater);
            Console.WriteLine(score.ToString());
            
        }
        [TestMethod]
        public void SkipCP()
        {
            var cp = CheckPoint.Current();
            var score = new CycleScore(cp.Cycle,DateTimeOffset.Now);
            var cpScore = new CpScore(2, 1000, 0);
            score.SetScore(cpScore, new Mock<ICycleScoreUpdater>().Object);
            
            Console.WriteLine(string.Join("\n",score.Summary()));
        }
        [TestMethod]
        public void setcp0()
        {
            var cp = new CheckPoint(new DateTime(2015, 6, 2, 13, 0, 0, DateTimeKind.Utc));
            var score = new CycleScore(cp.Cycle, DateTimeOffset.Now);
            score.SetScore(new CpScore(0, 1000, 0), new Mock<ICycleScoreUpdater>().Object);
            foreach (var summary in score.Summary())
            {
                Console.WriteLine(summary);
            }
            
        }
         
    }
}
