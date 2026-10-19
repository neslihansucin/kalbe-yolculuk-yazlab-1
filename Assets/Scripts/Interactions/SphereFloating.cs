
using UnityEngine;

public class SphereFloating : MonoBehaviour
{
    [Header("Süzülme Ayarlarý")]
    [SerializeField] private float floatHeight = 0.15f;
    [SerializeField] private float floatSpeed = 2f;

    [Header("Dönme Ayarlarý")]
    [SerializeField] private float rotationSpeed = 35f;

    private Vector3 startPosition;
    private bool isFloating = false;

    private void Update()
    {
        if (!isFloating)
            return;

        float newY = Mathf.Sin(Time.time * floatSpeed)
                     * floatHeight;

        transform.localPosition = startPosition
                                  + Vector3.up * newY;

        transform.Rotate(
            Vector3.up,
            rotationSpeed * Time.deltaTime,
            Space.Self
        );
    }

    public void StartFloating()
    {
        startPosition = transform.localPosition;
        isFloating = true;
    }
}
