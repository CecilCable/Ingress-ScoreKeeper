using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Upchurch.Ingress.Domain.Intel
{
    public interface IScoreDetails
    {
        IEnumerable<KeyValuePair<int, CpScore>> ScoreDictionary();
        string RegionName();
        int CycleId();
    }
}
