using System;
using System.Collections.Generic;

namespace NexusDogsGo.Gameplay.Missions;

public enum MissionMetric
{
    WalkMeters,
    CaptureDogs,
    VisitPoi,
    DefeatBoss
}

[Serializable]
public sealed class MissionProgress
{
    public string Id = string.Empty;
    public string Title = string.Empty;
    public MissionMetric Metric;
    public double Target;
    public double Current;
    public int RewardExperience;
    public int RewardCoins;
    public bool Claimed;
    public bool IsComplete => Current >= Target;
}

public sealed class MissionTracker
{
    private readonly List<MissionProgress> _missions = new();
    public IReadOnlyList<MissionProgress> Missions => _missions;

    public MissionTracker()
    {
        _missions.AddRange(CreateDailyStarterMissions());
    }

    public void Report(MissionMetric metric, double amount)
    {
        if (amount <= 0) return;
        foreach (var mission in _missions)
        {
            if (mission.Metric != metric || mission.IsComplete) continue;
            mission.Current = Math.Min(mission.Target, mission.Current + amount);
        }
    }

    public bool TryClaim(string id, out int experience, out int coins)
    {
        var mission = _missions.Find(x => x.Id == id);
        if (mission == null || !mission.IsComplete || mission.Claimed)
        {
            experience = 0;
            coins = 0;
            return false;
        }

        mission.Claimed = true;
        experience = mission.RewardExperience;
        coins = mission.RewardCoins;
        return true;
    }

    private static IEnumerable<MissionProgress> CreateDailyStarterMissions()
    {
        yield return new MissionProgress { Id = "walk-1k", Title = "Caminhar 1 km", Metric = MissionMetric.WalkMeters, Target = 1000, RewardExperience = 100, RewardCoins = 50 };
        yield return new MissionProgress { Id = "capture-3", Title = "Capturar 3 cães", Metric = MissionMetric.CaptureDogs, Target = 3, RewardExperience = 100, RewardCoins = 50 };
        yield return new MissionProgress { Id = "poi-2", Title = "Explorar 2 pontos", Metric = MissionMetric.VisitPoi, Target = 2, RewardExperience = 100, RewardCoins = 50 };
        yield return new MissionProgress { Id = "boss-1", Title = "Derrotar 1 boss", Metric = MissionMetric.DefeatBoss, Target = 1, RewardExperience = 200, RewardCoins = 100 };
        yield return new MissionProgress { Id = "walk-2k", Title = "Andar 2 km", Metric = MissionMetric.WalkMeters, Target = 2000, RewardExperience = 100, RewardCoins = 50 };
    }
}
