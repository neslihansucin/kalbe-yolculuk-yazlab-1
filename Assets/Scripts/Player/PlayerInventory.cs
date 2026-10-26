
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    public bool HasSphere => gameManager.State.HasMemoryOrb;

    public void CollectSphere()
    {
        gameManager.State.CollectMemoryOrb();
        Debug.Log("Mutlu Anýlar Küresi alýndý!");
    }
}
