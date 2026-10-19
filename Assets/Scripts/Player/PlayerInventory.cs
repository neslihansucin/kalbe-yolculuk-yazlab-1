
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public bool HasSphere { get; private set; }

    public void CollectSphere()
    {
        HasSphere = true;
        Debug.Log("Mutlu Anýlar Küresi alýndý!");
    }
}
