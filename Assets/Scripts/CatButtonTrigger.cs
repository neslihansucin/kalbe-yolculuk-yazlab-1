using UnityEngine;

public class CatButtonTrigger : MonoBehaviour
{
    public GameObject wall;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cat"))
        {
            wall.SetActive(false);
        }
    }
}