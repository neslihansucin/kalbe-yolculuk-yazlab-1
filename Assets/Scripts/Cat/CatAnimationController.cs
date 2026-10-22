
using UnityEngine;
using UnityEngine.AI;

public class CatAnimationController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator catAnimator;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private void Update()
    {
        if (agent == null || catAnimator == null)
            return;

        float speed = agent.velocity.magnitude;
        catAnimator.SetFloat(SpeedHash, speed);
    }
}
