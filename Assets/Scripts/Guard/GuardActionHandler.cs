using UnityEngine;

public class GuardActionHandler : MonoBehaviour
{
    [SerializeField] private DoorController doorController;
    [SerializeField] private GuardInteraction guardInteraction;
    public void HandleAction(string action)
    {
        if (action== "allow_entry")
        {
            doorController.Open();
            guardInteraction.DisableInteraction();
        }
    }
}
