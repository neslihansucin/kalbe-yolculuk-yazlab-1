
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneller")]
    [SerializeField] private GameObject bilgilendirmePanel;

    [Header("Ana Menü Nesneleri")]
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject exitButton;
    [SerializeField] private GameObject gameTitle;

    private void Start()
    {
        // Baþlangýçta bilgilendirme ekraný kapalý.
        bilgilendirmePanel.SetActive(false);

        // Ana menü görünür.
        startButton.SetActive(true);
        exitButton.SetActive(true);
        gameTitle.SetActive(true);
    }

    public void StartGame()
    {
        // Ana menü elemanlarýný gizle.
        startButton.SetActive(false);
        exitButton.SetActive(false);
        gameTitle.SetActive(false);

        // Bilgilendirme ekranýný aç.
        bilgilendirmePanel.SetActive(true);
    }

    public void SkipInformation()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void ExitGame()
    {
        Debug.Log("Oyundan çýkýlýyor...");
        Application.Quit();
    }
}
