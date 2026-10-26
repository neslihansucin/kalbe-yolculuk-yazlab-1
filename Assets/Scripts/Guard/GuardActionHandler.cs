using UnityEngine;

public class GuardActionHandler : MonoBehaviour
{
    [SerializeField] private DoorController doorController;
    [SerializeField] private GuardInteraction guardInteraction;
    public void HandleAction(GuardAction action)
    {
        if (action == GuardAction.AllowEntry)
        {
            doorController.Open();
            guardInteraction.DisableInteraction();
        }
    }
}
