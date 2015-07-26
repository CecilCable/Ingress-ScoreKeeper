using System;
using System.Collections.Generic;

namespace Upchurch.Ingress.Domain
{
    public interface ICycleScoreUpdater
    {
        CycleScore GetScoreForCycle(CycleIdentifier cycle);

        bool UpdateScore(CycleIdentifier cycle, DateTimeOffset dateTimeOffset, ICollection<CpScore> toArray);
    }
}