using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CatSensor
{
    private readonly Transform catTransform;

    private readonly float buttonDetectionDistance;
    private readonly float explorationDistance;
    private readonly int rayCount;

    public CatSensor(
        Transform catTransform,
        float buttonDetectionDistance = 10f,
        float explorationDistance = 4f,
        int rayCount = 36)
    {
        this.catTransform = catTransform;
        this.buttonDetectionDistance = buttonDetectionDistance;
        this.explorationDistance = explorationDistance;
        this.rayCount = rayCount;
    }

    public bool TryFindButton(out Vector3 buttonPosition)
    {
        Vector3 origin =
            catTransform.position + Vector3.up * 0.35f;

        for (int i = 0; i < rayCount; i++)
        {
            float angle =
                i * (360f / rayCount);

            Vector3 direction =
                Quaternion.Euler(0f, angle, 0f) *
                Vector3.forward;

            if (Physics.Raycast(
                origin,
                direction,
                out RaycastHit hit,
                buttonDetectionDistance))
            {
                Debug.DrawRay(
                    origin,
                    direction * hit.distance,
                    Color.yellow
                );

                if (hit.collider.CompareTag("Button"))
                {
                    buttonPosition =
                        hit.collider.transform.position;

                    Debug.Log(
                        "Kedi butonu sensörle algýladý: " +
                        buttonPosition
                    );

                    return true;
                }
            }
            else
            {
                Debug.DrawRay(
                    origin,
                    direction * buttonDetectionDistance,
                    Color.green
                );
            }
        }

        buttonPosition = Vector3.zero;
        return false;
    }

    public bool TryFindExplorationPoint(
     List<Vector3> visitedPoints,
     float visitedRadius,
     Collider searchArea,
     out Vector3 explorationPoint)
    {
        Vector3 origin =
            catTransform.position + Vector3.up * 0.35f;

        float bestScore = -Mathf.Infinity;
        Vector3 bestPoint = Vector3.zero;
        bool pointFound = false;

        for (int i = 0; i < rayCount; i++)
        {
            float angle =
                i * (360f / rayCount);

            Vector3 direction =
                Quaternion.Euler(0f, angle, 0f) *
                Vector3.forward;

            bool blocked =
    Physics.Raycast(
        origin,
        direction,
        out RaycastHit hit,
        explorationDistance,
        ~0,
        QueryTriggerInteraction.Ignore
    );
            float freeDistance;

            if (blocked)
            {
                freeDistance = hit.distance;

                Debug.DrawRay(
                    origin,
                    direction * hit.distance,
                    Color.red
                );
            }
            else
            {
                freeDistance = explorationDistance;

                Debug.DrawRay(
                    origin,
                    direction * explorationDistance,
                    Color.blue
                );
            }

            if (freeDistance < 0.5f)
                continue;
            float moveDistance =
                Mathf.Clamp(
                    freeDistance - 0.2f,
                    0.5f,
                    1.5f
                );

            Vector3 candidate =
                catTransform.position +
                direction * moveDistance;
            if (!NavMesh.SamplePosition(
                candidate,
                out NavMeshHit navHit,
                1f,
                NavMesh.AllAreas))
            {
                continue;
            }
            if (searchArea != null)
            {
                Bounds areaBounds = searchArea.bounds;

                bool insideArea =
                    navHit.position.x >= areaBounds.min.x &&
                    navHit.position.x <= areaBounds.max.x &&
                    navHit.position.z >= areaBounds.min.z &&
                    navHit.position.z <= areaBounds.max.z;

                if (!insideArea)
                {
                    continue;
                }
            }
            bool alreadyVisited = false;

            foreach (Vector3 visitedPoint in visitedPoints)
            {
                if (Vector3.Distance(
                    navHit.position,
                    visitedPoint) <= visitedRadius)
                {
                    alreadyVisited = true;
                    break;
                }
            }

            if (alreadyVisited)
                continue;

            NavMeshPath path =
                new NavMeshPath();

            bool pathExists =
                NavMesh.CalculatePath(
                    catTransform.position,
                    navHit.position,
                    NavMesh.AllAreas,
                    path
                );

            if (!pathExists)
                continue;

            if (path.status !=
                NavMeshPathStatus.PathComplete)
            {
                continue;
            }

            float score = freeDistance;

            if (score > bestScore)
            {
                bestScore = score;
                bestPoint = navHit.position;
                pointFound = true;
            }
        }

        explorationPoint = bestPoint;
        return pointFound;
    }
}