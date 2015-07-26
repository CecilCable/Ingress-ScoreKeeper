using Upchurch.Ingress.Domain;

namespace Upchurch.Ingress.Infrastructure
{
    public interface ICycleScoreUpdater
    {
        CycleScore GetScoreForCycle(CycleIdentifier cycle);

        void UpdateScore(CycleIdentifier cycle, CpScore newScore);
    }
}