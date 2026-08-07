using UnityEngine;
public abstract class GameEffect : ScriptableObject
{
    public abstract void Apply(GameStateStore state);
}
