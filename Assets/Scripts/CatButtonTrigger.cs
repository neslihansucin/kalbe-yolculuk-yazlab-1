using UnityEngine;

public class CatButtonTrigger : MonoBehaviour
{
    public GameObject emotionWall;
    public GameObject corridorWall;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cat"))
        {
            if (emotionWall != null)
                emotionWall.SetActive(false);

            if (corridorWall != null)
                corridorWall.SetActive(false);

            Debug.Log("Kedi butona bastý, yollar açýldý.");
        }
    }
}