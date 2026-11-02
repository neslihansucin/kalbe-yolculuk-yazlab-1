using UnityEngine;

public class HowToPlayPanelController : MonoBehaviour
{
    [SerializeField] private GameObject howToPlayPanel;

    private void Start()
    {
        howToPlayPanel.SetActive(false);
    }

    public void OpenPanel()
    {
        howToPlayPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        howToPlayPanel.SetActive(false);
    }
}
