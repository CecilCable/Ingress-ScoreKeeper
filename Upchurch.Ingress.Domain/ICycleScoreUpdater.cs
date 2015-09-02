using System;
using System.Collections.Generic;

namespace Upchurch.Ingress.Domain
{
    public interface ICycleScoreUpdater
    {
        CycleScore GetScoreForCycle(CycleIdentifier cycle);

        bool UpdateScore(CycleIdentifier cycle, int checkpoint, long timestampTicks, CpScore cpScore);
     
        bool SetSnooze(CycleIdentifier cycle, long timestampTicks, bool isSnooze);
    }
}