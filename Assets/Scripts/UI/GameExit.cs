
using UnityEngine;

public class GameExit : MonoBehaviour
{
    public void OyundanCik()
    {
        Debug.Log("Oyundan çýkýlýyor...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
