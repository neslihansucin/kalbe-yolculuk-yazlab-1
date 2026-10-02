using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using System.Collections.Generic;

public class CatStateMachine : MonoBehaviour
{
    public enum CatState
    {
        FollowingPlayer,
        CrossingObstacle,
        Searching,
        MovingToButton,
        ActivatingButton,
        TaskComplete
    }

    public CatState currentState = CatState.FollowingPlayer;

    public Transform player;
    public Transform searchPoint;

    public float followDistance = 2f;

    private NavMeshAgent agent;
    public Transform obstacleWall;
    private Vector3 crossingTarget;
    private bool crossingTargetChosen = false;

    private Vector3 selectedGapEntrance;
    private Vector3 selectedGapExit;

    private bool gapSelected = false;
    private bool reachedGapEntrance = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentState = CatState.FollowingPlayer;
    }

    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame &&
            currentState == CatState.FollowingPlayer)
        {
            currentState = CatState.CrossingObstacle;
            Debug.Log("Kedi komutu aldý, engeli geçiyor...");
        }

        switch (currentState)
        {
            case CatState.FollowingPlayer:
                FollowingPlayerState();
                break;

            case CatState.CrossingObstacle:
                CrossingObstacleState();
                break;

            case CatState.Searching:
                SearchingState();
                break;

            case CatState.MovingToButton:
                MovingState();
                break;

            case CatState.ActivatingButton:
                ActivatingState();
                break;

            case CatState.TaskComplete:
                TaskCompleteState();
                break;
        }
    }

    void FollowingPlayerState()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance > followDistance)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            agent.ResetPath();
        }
    }
    void CrossingObstacleState()
    {
        if (obstacleWall == null)
            return;

        // Geçit daha önce seçilmediyse duvarý sadece bir kere tara
        if (!gapSelected)
        {
            Renderer[] renderers =
                obstacleWall.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0)
                return;

            Bounds wallBounds = renderers[0].bounds;

            foreach (Renderer rend in renderers)
            {
                wallBounds.Encapsulate(rend.bounds);
            }

            // Duvarýn gerçek sað-sol yönü
            Vector3 sideDirection = obstacleWall.right;
            sideDirection.y = 0;
            sideDirection.Normalize();

            // Duvara dik yön
            Vector3 wallDirection =
                Vector3.Cross(sideDirection, Vector3.up).normalized;

            // Yön ters tarafa bakýyorsa düzelt
            Vector3 catToWall =
                wallBounds.center - transform.position;

            catToWall.y = 0;

            if (Vector3.Dot(wallDirection, catToWall) < 0)
            {
                wallDirection = -wallDirection;
            }

            float wallWidth =
                Mathf.Max(wallBounds.size.x, wallBounds.size.z);

            Vector3 rayOriginBase =
                wallBounds.center - wallDirection * 1f;

            rayOriginBase.y =
                transform.position.y + 0.35f;

            // Her deliðin merkezini burada tutacaðýz
            List<Vector3> gapCenters = new List<Vector3>();

            bool insideGap = false;
            Vector3 gapStart = Vector3.zero;
            Vector3 gapEnd = Vector3.zero;

            int scanCount = 80;

            for (int i = 0; i <= scanCount; i++)
            {
                float t = i / (float)scanCount;

                float offset =
                    Mathf.Lerp(
                        -wallWidth / 2f,
                        wallWidth / 2f,
                        t
                    );

                Vector3 rayOrigin =
                    rayOriginBase +
                    sideDirection * offset;

                Debug.DrawRay(
                    rayOrigin,
                    wallDirection * 2f,
                    Color.yellow,
                    3f
                );

                bool blocked =
                    Physics.Raycast(
                        rayOrigin,
                        wallDirection,
                        2f
                    );

                // Açýklýk bulundu
                if (!blocked)
                {
                    if (!insideGap)
                    {
                        insideGap = true;
                        gapStart = rayOrigin;
                    }

                    gapEnd = rayOrigin;
                }
                else
                {
                    // Açýk bölge bittiyse tek bir delik oluþtur
                    if (insideGap)
                    {
                        Vector3 gapCenter =
                            (gapStart + gapEnd) / 2f;

                        gapCenters.Add(gapCenter);

                        insideGap = false;
                    }
                }
            }

            // Duvarýn en sonunda da açýklýk devam ediyorsa
            if (insideGap)
            {
                Vector3 gapCenter =
                    (gapStart + gapEnd) / 2f;

                gapCenters.Add(gapCenter);
            }

            Debug.Log(
                "Algýlanan delik sayýsý: " +
                gapCenters.Count
            );

            float nearestDistance = Mathf.Infinity;
            bool validGapFound = false;

            foreach (Vector3 gapCenter in gapCenters)
            {
                Vector3 entrancePosition =
                    gapCenter - wallDirection * 0.6f;

                Vector3 exitPosition =
                    gapCenter + wallDirection * 0.6f;

                NavMeshHit entranceHit;
                NavMeshHit exitHit;

                bool entranceValid =
                    NavMesh.SamplePosition(
                        entrancePosition,
                        out entranceHit,
                        0.8f,
                        NavMesh.AllAreas
                    );

                bool exitValid =
                    NavMesh.SamplePosition(
                        exitPosition,
                        out exitHit,
                        0.8f,
                        NavMesh.AllAreas
                    );

                if (!entranceValid || !exitValid)
                    continue;

                // Deliðin iki tarafý gerçekten NavMesh ile baðlý mý?
                NavMeshPath gapPath =
                    new NavMeshPath();

                bool pathValid =
                    NavMesh.CalculatePath(
                        entranceHit.position,
                        exitHit.position,
                        NavMesh.AllAreas,
                        gapPath
                    );

                if (!pathValid ||
                    gapPath.status !=
                    NavMeshPathStatus.PathComplete)
                {
                    continue;
                }

                float lateralDistance = Mathf.Abs(
    Vector3.Dot(
        entranceHit.position - transform.position,
        sideDirection
    )
);
                Debug.Log(
     "Delik merkezi: " +
     gapCenter +
     " | Yatay mesafe: " +
     lateralDistance.ToString("F2")
 );

                // En yakýn geçerli deliði seç
                if (lateralDistance < nearestDistance)
                {
                    nearestDistance = lateralDistance;

                    selectedGapEntrance =
                        entranceHit.position;

                    selectedGapExit =
                        exitHit.position;

                    validGapFound = true;
                }
            }

            if (validGapFound)
            {
                gapSelected = true;

                agent.SetDestination(
                    selectedGapEntrance
                );

                Debug.Log(
                    "Kedi kendisine en yakýn deliði seçti."
                );
            }
            else
            {
                Debug.Log(
                    "Geçerli bir delik bulunamadý."
                );
            }

            return;
        }

        // Önce deliðin giriþine git
        if (!reachedGapEntrance)
        {
            agent.SetDestination(
                selectedGapEntrance
            );

            if (!agent.pathPending &&
                agent.remainingDistance <= 0.2f)
            {
                reachedGapEntrance = true;

                Debug.Log(
                    "Kedi deliðin giriþine ulaþtý."
                );
            }

            return;
        }

        // Sonra ayný deliðin öbür tarafýna geç
        agent.SetDestination(
            selectedGapExit
        );

        if (!agent.pathPending &&
            agent.remainingDistance <= 0.2f)
        {
            currentState =
                CatState.Searching;

            Debug.Log(
                "Kedi seçtiði delikten geçti, þimdi butonu arýyor..."
            );
        }
    }
    void SearchingState()
    {
        // Kedi butonu arayacak.
    }

    void MovingState()
    {
        // Kedi NavMesh ile butona gidecek.
    }

    void ActivatingState()
    {
        // Kedi butonu aktif edecek.
    }

    void TaskCompleteState()
    {
        // Görev tamamlandý.
    }
}