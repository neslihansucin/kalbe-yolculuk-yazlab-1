using UnityEngine;

[System.Serializable]
public class GameState
{
    public bool HasMemoryOrb { get; private set; } = false;
    public int GuardHintLevel { get; private set; } =0;

    public void CollectMemoryOrb()
    {
        HasMemoryOrb = true;
    }

    public void IncreaseGuardHintLevel()
    {
        GuardHintLevel = Mathf.Min(GuardHintLevel + 1, 1);
    }
}
