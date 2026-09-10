using System;

namespace NexusDogsGo.Gameplay.Battle;

public enum BattleAction
{
    Attack,
    Defend,
    Item
}

public sealed class BattleFighter
{
    public string Name = string.Empty;
    public int MaxHp = 100;
    public int Hp = 100;
    public int Attack = 50;
    public int Defense = 50;
    public bool Guarding;
    public bool IsDefeated => Hp <= 0;
}

public readonly struct BattleTurnResult
{
    public readonly int Damage;
    public readonly int Healing;
    public readonly bool TargetDefeated;

    public BattleTurnResult(int damage, int healing, bool targetDefeated)
    {
        Damage = damage;
        Healing = healing;
        TargetDefeated = targetDefeated;
    }
}

public static class BattleEngine
{
    public static BattleTurnResult Resolve(BattleFighter actor, BattleFighter target, BattleAction action)
    {
        if (actor == null) throw new ArgumentNullException(nameof(actor));
        if (target == null) throw new ArgumentNullException(nameof(target));
        if (actor.IsDefeated) return default;

        switch (action)
        {
            case BattleAction.Defend:
                actor.Guarding = true;
                return new BattleTurnResult(0, 0, target.IsDefeated);
            case BattleAction.Item:
                var before = actor.Hp;
                actor.Hp = Math.Min(actor.MaxHp, actor.Hp + Math.Max(15, actor.MaxHp / 4));
                return new BattleTurnResult(0, actor.Hp - before, target.IsDefeated);
            default:
                var raw = Math.Max(1, actor.Attack - target.Defense / 2);
                var damage = target.Guarding ? Math.Max(1, raw / 2) : raw;
                target.Guarding = false;
                target.Hp = Math.Max(0, target.Hp - damage);
                return new BattleTurnResult(damage, 0, target.IsDefeated);
        }
    }
}
