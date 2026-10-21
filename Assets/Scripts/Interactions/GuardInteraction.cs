using UnityEngine;

public class GuardInteraction : MonoBehaviour
{
    private bool isPlayerInRange;
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
        if(!other.CompareTag("Player"))
                return;

        isPlayerInRange = false;
        Debug.Log("[Guard Interaction] Oyuncu etki alanýndan çýktý.");
    }

}
