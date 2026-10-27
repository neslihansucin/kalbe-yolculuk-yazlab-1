
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameManager : MonoBehaviour
{
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private float endGameDelay = 2f;

    private bool isGameFinished = false;
    private bool isEndingStarted = false;

    private void Start()
    {
        Time.timeScale = 1f;

        if (endGamePanel != null)
        {
            endGamePanel.SetActive(false);
        }
    }

    // Kalp ana merkeze yerleþtirildiðinde çaðrýlacak
    public void HeartPlaced()
    {
        if (isEndingStarted)
            return;

        isEndingStarted = true;
        StartCoroutine(EndGameAfterDelay());
    }

    private IEnumerator EndGameAfterDelay()
    {
        yield return new WaitForSeconds(endGameDelay);
        ShowEndGameScreen();
    }

    public void ShowEndGameScreen()
    {
        if (isGameFinished || endGamePanel == null)
            return;

        isGameFinished = true;
        isEndingStarted = true;

        endGamePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
