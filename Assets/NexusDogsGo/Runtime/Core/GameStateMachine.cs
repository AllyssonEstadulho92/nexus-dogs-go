using System;

namespace NexusDogsGo.Core;

public enum GameState
{
    Boot,
    Home,
    Exploring,
    Encounter,
    Battle,
    Ar,
    Suspended
}

public sealed class GameStateMachine
{
    public GameState Current { get; private set; } = GameState.Boot;
    public event Action<GameState, GameState>? Changed;

    public void Set(GameState next)
    {
        if (Current == next) return;
        var previous = Current;
        Current = next;
        Changed?.Invoke(previous, next);
    }
}
