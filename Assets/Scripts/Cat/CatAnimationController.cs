
using UnityEngine;
using UnityEngine.AI;

public class CatAnimationController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator catAnimator;

    [Header("Animasyon Ayarlari")]
    [SerializeField] private float walkAnimationSpeed = 2f;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private void Update()
    {
        if (agent == null || catAnimator == null)
            return;

        float speed = agent.velocity.magnitude;

        catAnimator.SetFloat(SpeedHash, speed);

        // Kedi hareket ediyorsa animasyonu hizlandir
        // Duruyorsa normal animasyon hizina don
        if (speed > 0.1f)
        {
            catAnimator.speed = walkAnimationSpeed;
        }
        else
        {
            catAnimator.speed = 1f;
        }
    }
}
