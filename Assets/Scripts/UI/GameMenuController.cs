using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameMenuController : MonoBehaviour
{
    [SerializeField] private GameObject gameMenuPanel;
    [SerializeField] private GameObject controlsText;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        if (gameMenuPanel == null)
        {
            return;
        }

        bool isOpening = !gameMenuPanel.activeSelf;
        gameMenuPanel.SetActive(isOpening);

        if (!isOpening && controlsText != null) 
        {
            controlsText.SetActive(false);
        }

    }

    public void ToggleControls()
    {
        if (controlsText == null)
        {
            return;
        }

        controlsText.SetActive(!controlsText.activeSelf);
    }
}
