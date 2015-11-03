using System.Collections.Generic;

namespace Upchurch.Ingress.Domain.Intel
{
    public class Result
    {
        public string[][] scoreHistory { get; set; }
        public string regionName { get; set; }

        public IEnumerable<KeyValuePair<int, CpScore>> Generate()
        {
            foreach (var cp in scoreHistory)
            {
                yield return new KeyValuePair<int, CpScore>(int.Parse(cp[0]), new CpScore(int.Parse(cp[2]), int.Parse(cp[1]), string.Empty));
            }
        }
    }
}