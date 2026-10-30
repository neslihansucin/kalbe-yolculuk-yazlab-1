
using UnityEngine;

public class HeartLightController : MonoBehaviour
{
    [SerializeField] private Light[] heartLights;

    public void TurnOnLights()
    {
        foreach (Light light in heartLights)
        {
            if (light != null)
            {
                light.enabled = true;
            }
        }

        Debug.Log("Kalbin merkezindeki ýþýklar yandý!");
    }
}
