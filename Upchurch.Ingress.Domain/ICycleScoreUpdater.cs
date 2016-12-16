using System.Collections.Generic;

namespace Upchurch.Ingress.Domain
{
    public interface ICycleScoreUpdater
    {
        CycleScore GetScoreForCycle(CycleIdentifier cycle);

        bool UpdateScore(CycleIdentifier cycle, int checkpoint, long timestampTicks, CpScore cpScore);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cycle"></param>
        /// <param name="timestampTicks">optional because fiddler update might not be able to ask for the timestampTicks</param>
        /// <param name="cpScores"></param>
        /// <returns></returns>
        bool UpdateScore(CycleIdentifier cycle , long? timestampTicks, params KeyValuePair<int, CpScore>[] cpScores);
        
        bool SetSnooze(CycleIdentifier cycle, long timestampTicks, bool isSnooze);
    }
}