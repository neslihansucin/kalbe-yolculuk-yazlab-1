
using UnityEngine;

public class GuardInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogueUI dialogueUI;

    private bool isPlayerInRange;
    private bool canInteract = true;

    public bool IsPlayerInRange => isPlayerInRange;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        isPlayerInRange = true;

        Debug.Log("[Guard Interaction] Oyuncu etkileþim alanýna girdi.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        isPlayerInRange = false;
        dialogueUI.CloseDialogue();

        Debug.Log("[Guard Interaction] Oyuncu etkileþim alanýndan çýktý.");
    }

    public void Interact()
    {
        if (!canInteract)
            return;

        Debug.Log("[Guard Interaction] Konuþma ekraný açýlýyor.");

        dialogueUI.OpenDialogue();
    }

    public void DisableInteraction()
    {
        canInteract = false;
        dialogueUI.CloseDialogue();
    }
}
