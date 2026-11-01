using UnityEngine;

public class DoorController : MonoBehaviour
{
    private bool isOpen = false;
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private Collider blockingCollider;
    [SerializeField] private float colliderDisableDelay = 2.5f;

    public void Open()
    {

        if (isOpen)
            return;

        isOpen = true;
        
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open");
            Invoke(nameof(DisableBlockingCollider), colliderDisableDelay);
        }
        else
        {
            Debug.Log("[Door] Animator referansý atanamadý.");
        }
            Debug.Log("[Door] Kapý açýldý.");
    }

    private void DisableBlockingCollider()
    {
        if (blockingCollider != null)
        {
            blockingCollider.enabled = false;
        }
    }
}
