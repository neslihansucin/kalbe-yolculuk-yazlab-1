using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    private IInteracable currentInteracable;

    private void OnTriggerEnter(Collider other)
    {
        IInteracable interactable = other.GetComponent<IInteracable>();

        if (interactable == null)
            return;

        currentInteracable = interactable;
    }

    private void OnTriggerExit(Collider other)
    {
        IInteracable interactable = other.GetComponent<IInteracable>();

        if (interactable == null)
            return;

        if (currentInteracable == interactable)
        {
            currentInteracable = null;
        }
    }

    public void OnInteract(InputValue value)
    {
        if (!value.isPressed)
            return;

        currentInteracable?.Interact();
    }
}
