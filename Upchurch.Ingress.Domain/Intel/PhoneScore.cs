using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable InconsistentNaming

namespace Upchurch.Ingress.Domain.Intel
{
       
    public class History : IScoreDetails
    {
        public Card card { get; set; }
        public CycleTimelineDetails cycleTimelineDetails { get; set; }

        public int CycleId()
        {
            //2015.45
            var parts = card.scoreCycleTitle.Split('.');
            var indexForYear = int.Parse(parts[1]);
            return indexForYear - 21;
        }

        public IEnumerable<KeyValuePair<int, CpScore>> ScoreDictionary()
        {
            for (var i = 0; i < cycleTimelineDetails.scoreHistory.Length; i++)
            {
                var row = cycleTimelineDetails.scoreHistory[i];

                yield return new KeyValuePair<int, CpScore>(i+1,new CpScore(row.ParseResistanceScore(), row.ParseEnlightenedScore(),null));
            }
        }

        public string RegionName()
        {
            return card.regionName;
        }
    }

    public class PhoneScore
    {
        public string alienScore { get; set; }
        public string resistanceScore { get; set; }

        public int ParseResistanceScore()
        {
            return int.Parse(resistanceScore);
        }

        public int ParseEnlightenedScore()
        {
            return int.Parse(alienScore);
        }

    }
    public class Card
    {
        public string regionName;
        public string scoreCycleTitle;
    }

    public class CycleTimelineDetails
    {
        public PhoneScore[] scoreHistory { get; set; }
    }
}
