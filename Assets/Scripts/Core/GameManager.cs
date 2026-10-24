using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameState State {  get; private set; } = new GameState();

    [ContextMenu("TEST - Memory Orb Topla")]
    private void TestCollectMemoryOrb()
    {
        State.CollectMemoryOrb();
        Debug.Log("[TEST] Memory Orb toplandý.");
    }
}
