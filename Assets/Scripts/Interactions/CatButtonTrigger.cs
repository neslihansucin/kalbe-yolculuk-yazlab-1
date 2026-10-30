using UnityEngine;

public class CatButtonTrigger : MonoBehaviour
{
    public BreakableWall emotionWall;
    public GameObject corridorWall;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Buton triggerýna giren obje: " + other.name);

        if (other.CompareTag("Cat"))
        {
            Debug.Log("CatButtonTrigger çalýþtý!");

            if (emotionWall != null)
            {
                emotionWall.BreakWall();
                Debug.Log("EmotionWall parçalanma komutu verildi.");
            }
            else
            {
                Debug.LogWarning("EmotionWall atanmamýþ!");
            }

            if (corridorWall != null)
            {
                corridorWall.SetActive(false);
                Debug.Log("CorridorWall kapatýldý.");
            }

            Debug.Log("Kedi butona bastý, yollar açýldý.");
        }
    }

}