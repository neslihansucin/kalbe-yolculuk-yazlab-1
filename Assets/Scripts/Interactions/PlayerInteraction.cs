
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    private IInteractable currentInteractable;

    private void OnTriggerEnter(Collider other)
    {
        IInteractable interactable =
            other.GetComponent<IInteractable>();

        if (interactable == null)
        {
            interactable =
                other.GetComponentInParent<IInteractable>();
        }

        if (interactable == null)
            return;

        currentInteractable = interactable;

        Debug.Log("Etkileþim alanýna girildi: " + other.name);
    }

    private void OnTriggerExit(Collider other)
    {
        IInteractable interactable =
            other.GetComponent<IInteractable>();

        if (interactable == null)
        {
            interactable =
                other.GetComponentInParent<IInteractable>();
        }

        if (interactable == null)
            return;

        if (currentInteractable == interactable)
        {
            currentInteractable = null;
        }
    }

    public void OnInteract(InputValue value)
    {
        if (!value.isPressed)
            return;

        Debug.Log("E tuþuna basýldý.");

        if (currentInteractable == null)
        {
            Debug.LogWarning("Etkileþim nesnesi bulunamadý!");
            return;
        }

        currentInteractable.Interact();
        Debug.Log("Interact çalýþtýrýldý.");
    }
}
