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
            var score = new CycleScore(CheckPoint.Current().Cycle,DateTimeOffset.Now.Ticks,false);
            foreach (var cp in score.MissingCPs())
            {
                Console.WriteLine(cp.Cp);
            }
            score.SetScore(2,new UpdateScore(new CpScore(1000, 0,"Cecilcable"),0), new Mock<ICycleScoreUpdater>().Object);
            foreach (var cp in score.MissingCPs())
            {
                Console.WriteLine(cp.Cp);
            }
        }

        [TestMethod]
        public void CheckMessage()
        {
            var current = CheckPoint.Current();
            var score = new CycleScore(current.Cycle,DateTimeOffset.Now.Ticks,false);
            var updater = new Mock<ICycleScoreUpdater>().Object;

            score.SetScore(1,new UpdateScore {EnlightenedScore = 1000,ResistanceScore = 0}, updater);
            score.SetScore(2, new UpdateScore { EnlightenedScore = 1000, ResistanceScore = 0 }, updater);
            score.SetScore(3, new UpdateScore { EnlightenedScore = 1000, ResistanceScore = 0 }, updater);
            score.SetScore(4, new UpdateScore { EnlightenedScore = 1000, ResistanceScore = 0 }, updater);
            score.SetScore(5, new UpdateScore { EnlightenedScore = 1000, ResistanceScore = 0 }, updater);
            score.SetScore(6, new UpdateScore { EnlightenedScore = 1000, ResistanceScore = 0 }, updater);
            score.SetScore(7, new UpdateScore { EnlightenedScore = 1000, ResistanceScore = 0 }, updater);
            
            Console.WriteLine(score.ToString());
            
        }
        [TestMethod]
        public void SkipCP()
        {
            var cp = CheckPoint.Current();
            var score = new CycleScore(cp.Cycle, DateTimeOffset.Now.Ticks,false);
            var updater = new Mock<ICycleScoreUpdater>().Object;

            score.SetScore(2, new UpdateScore { EnlightenedScore = 1000, ResistanceScore = 0 }, updater);
            score.SetScore(3, new UpdateScore { EnlightenedScore = 500, ResistanceScore = 0 }, updater);
            score.SetScore(4, new UpdateScore { EnlightenedScore = 500, ResistanceScore = 1999 }, updater);
            
            Console.WriteLine(string.Join("\n",score.Summary(false)));
        }



        [TestMethod]
        public void setcp0()
        {
            var cp = new CheckPoint(new DateTime(2015, 6, 2, 13, 0, 0, DateTimeKind.Utc));
            var score = new CycleScore(cp.Cycle, DateTimeOffset.Now.Ticks,false);
            score.SetScore(0, new UpdateScore { EnlightenedScore = 1000, ResistanceScore = 0 }, new Mock<ICycleScoreUpdater>().Object);
            foreach (var summary in score.Summary(false))
            {
                Console.WriteLine(summary);
            }
            
        }
         
    }
}
