
using UnityEngine;

[RequireComponent(typeof(Light))]
public class HeartPulseLight : MonoBehaviour
{
    [SerializeField] private float minIntensity = 1.5f;
    [SerializeField] private float maxIntensity = 3f;
    [SerializeField] private float pulseSpeed = 1.2f;

    private Light heartLight;

    private void Awake()
    {
        heartLight = GetComponent<Light>();
    }

    private void Update()
    {
        float pulse = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) + 1f) / 2f;

        heartLight.intensity = Mathf.Lerp(
            minIntensity,
            maxIntensity,
            pulse
        );
    }
}
