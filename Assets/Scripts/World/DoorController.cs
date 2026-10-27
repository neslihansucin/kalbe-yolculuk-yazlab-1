using UnityEngine;

public class DoorController : MonoBehaviour
{
    private bool isOpen = false;

   public void Open()
    {

        if (isOpen)
            return;

        isOpen = true;

        transform.position += Vector3.right * 3f;

        Debug.Log("[Door] Kapý açýldý.");
    }
}
