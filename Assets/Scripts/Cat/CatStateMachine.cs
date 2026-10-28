using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using System.Collections.Generic;

public class CatStateMachine : MonoBehaviour
{
   

    public CatState currentState = CatState.FollowingPlayer;

    public Transform player;
    public Transform searchPoint;

    public float followDistance = 2f;

    private NavMeshAgent agent;
    public Transform obstacleWall;
    [Header("Wall Detection")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallDetectionDistance = 12f;
    [SerializeField] private float wallDetectionAngle = 120f;
    [SerializeField] private int wallRayCount = 25;
    private Vector3 crossingTarget;
    private bool crossingTargetChosen = false;

    private Vector3 selectedGapEntrance;
    private Vector3 selectedGapExit;

    private bool gapSelected = false;
    private bool reachedGapEntrance = false;
    private CatSensor sensor;

    private Vector3 detectedButtonPosition;
    private bool buttonDetected = false;

    private Vector3 explorationTarget;
    private bool hasExplorationTarget = false;

    private List<Vector3> visitedPoints =
        new List<Vector3>();

    [SerializeField]
    private float visitedPointRadius = 1.5f;

    [SerializeField]
    private float scanInterval = 0.5f;

    private float nextScanTime = 0f;

    [SerializeField]
    private Collider searchArea; 
    private float taskCompleteTime;
    private bool missionCompleted = false;
    public float followAgainDelay = 2.5f;
    [SerializeField] private float wallSearchTimeout = 5f;
    private float wallSearchStartTime;
    [Header("Camera")]
    [SerializeField] private CameraFollow cameraFollow;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            transform.position,
            out hit,
            2f,
            NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }

        sensor = new CatSensor(
            transform,
            10f,
            4f,
            36
        );

        currentState = CatState.FollowingPlayer;
    }
    void Update()
    {

        if (Keyboard.current != null &&
     Keyboard.current.eKey.wasPressedThisFrame &&
     currentState == CatState.FollowingPlayer &&
     !missionCompleted)
        {
            currentState = CatState.CrossingObstacle;
            wallSearchStartTime = Time.time;

            // Kamera kediyi takip etmeye baþlasýn
            if (cameraFollow != null)
            {
                cameraFollow.WatchCat();
            }

            Debug.Log("Kedi komutu aldý, engeli geçiyor...");
        }

        if (currentState == CatState.CrossingObstacle &&
    !gapSelected &&
    Time.time - wallSearchStartTime >= wallSearchTimeout)
        {
            Debug.LogWarning("Kedi 5 saniyede geçit bulamadý. Tekrar denenebilir.");

            if (agent.isOnNavMesh)
                agent.ResetPath();

            obstacleWall = null;

            gapSelected = false;
            reachedGapEntrance = false;

            selectedGapEntrance = Vector3.zero;
            selectedGapExit = Vector3.zero;

            currentState = CatState.FollowingPlayer;

            if (cameraFollow != null)
                cameraFollow.WatchPlayer();

            return;
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
        if (player == null || agent == null || !agent.isOnNavMesh)
            return;

        // Oyuncunun tam konumu yerine
        // biraz arkasi ve sagini hedefle
        Vector3 followTarget =
            player.position
            - player.forward * followDistance
            + player.right * 0.8f;

        // Kediye uygun NavMesh noktasi bul
        NavMeshQueryFilter filter = new NavMeshQueryFilter
        {
            agentTypeID = agent.agentTypeID,
            areaMask = agent.areaMask
        };

        if (!NavMesh.SamplePosition(
            followTarget,
            out NavMeshHit hit,
            1.5f,
            filter))
        {
            if (agent.hasPath)
                agent.ResetPath();

            return;
        }

        // Yalnizca yatay mesafeyi kontrol et
        Vector3 difference =
            hit.position - transform.position;

        difference.y = 0f;

        // Hedefe yeterince yaklastiysa dur
        if (difference.sqrMagnitude <= 0.36f)
        {
            if (agent.hasPath)
                agent.ResetPath();

            return;
        }

        // Oyuncunun yanindaki noktaya ilerle
        agent.SetDestination(hit.position);
    }

    private bool TryDetectWall(out Transform detectedWall)
    {
        detectedWall = null;

        Vector3 origin =
            transform.position + Vector3.up * 0.35f;

        float nearestDistance = Mathf.Infinity;

        for (int i = 0; i < wallRayCount; i++)
        {
            float angle =
                -wallDetectionAngle / 2f
                + wallDetectionAngle * i / (wallRayCount - 1f);

            Vector3 direction =
                Quaternion.Euler(0f, angle, 0f)
                * transform.forward;

            Debug.DrawRay(
                origin,
                direction * wallDetectionDistance,
                Color.cyan,
                0.5f
            );

            if (Physics.Raycast(
                origin,
                direction,
                out RaycastHit hit,
                wallDetectionDistance,
                wallLayer,
                QueryTriggerInteraction.Ignore))
            {
                if (hit.distance < nearestDistance)
                {
                    ObstacleWallMarker marker =
                        hit.collider.GetComponentInParent
                        <ObstacleWallMarker>();

                    if (marker != null)
                    {
                        detectedWall = marker.transform;
                        nearestDistance = hit.distance;
                    }
                }
            }
        }

        return detectedWall != null;
    }

    void CrossingObstacleState()
    {
        if (obstacleWall == null)
        {
            if (!TryDetectWall(out obstacleWall))
            {
                Debug.Log("Kedi henüz bir duvar algýlayamadý.");
                return;
            }

            Debug.Log("Kedi duvarý kendi sensörleriyle buldu: "
                      + obstacleWall.name);
        }
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
        2f,
        ~0,
        QueryTriggerInteraction.Ignore
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
        if (Time.time < nextScanTime)
            return;

        nextScanTime = Time.time + scanInterval;

        Vector3 buttonPosition;

        // Önce çevrede buton var mý kontrol et.
        if (sensor.TryFindButton(out buttonPosition))
        {
            detectedButtonPosition = buttonPosition;
            buttonDetected = true;
            hasExplorationTarget = false;

            agent.ResetPath();

            currentState = CatState.MovingToButton;

            Debug.Log("Buton bulundu. Kedi butona gidiyor.");

            return;
        }

        // Kedi zaten bir keþif noktasýna gidiyorsa oraya ulaþmasýný bekle.
        if (hasExplorationTarget)
        {
            if (!agent.pathPending &&
                agent.remainingDistance <= 0.3f)
            {
                visitedPoints.Add(explorationTarget);

                hasExplorationTarget = false;

                Debug.Log("Bu bölge araþtýrýldý.");
            }

            return;
        }

        // Buton görünmüyorsa yeni bir bölge ara.
        Vector3 newExplorationPoint;

        if (sensor.TryFindExplorationPoint(
            visitedPoints,
            visitedPointRadius,
            searchArea,
            out newExplorationPoint))
        {
            explorationTarget = newExplorationPoint;
            hasExplorationTarget = true;

            agent.SetDestination(explorationTarget);

            Debug.Log("Kedi yeni bir bölgeyi keþfetmeye gidiyor.");
        }
        else
        {
            Debug.Log("Yeni keþif noktasý bulunamadý.");

            agent.ResetPath();

            gapSelected = false;
            reachedGapEntrance = false;

            selectedGapEntrance = Vector3.zero;
            selectedGapExit = Vector3.zero;

            buttonDetected = false;
            hasExplorationTarget = false;

            visitedPoints.Clear();

            currentState = CatState.FollowingPlayer;

            Debug.Log("Kedi butonu bulamadý, oyuncuya geri dönüyor.");
        }
    }
    void MovingState()
    {
        if (!buttonDetected)
        {
            currentState =
                CatState.Searching;

            return;
        }

        agent.SetDestination(
            detectedButtonPosition
        );

        if (!agent.pathPending &&
            agent.remainingDistance <= 0.5f)
        {
            currentState =
                CatState.ActivatingButton;

            Debug.Log(
                "Kedi butona ulaþtý."
            );
        }
    }

    void ActivatingState()
    {
        missionCompleted = true;

        taskCompleteTime = Time.time;

        currentState = CatState.TaskComplete;

        if (cameraFollow != null)
        {
            cameraFollow.WatchPlayer();
        }

        Debug.Log("Kedi butona bastý, görev tamamlandý.");
    }
    void TaskCompleteState()
    {
        agent.ResetPath();

        if (Time.time >= taskCompleteTime + followAgainDelay)
        {
            currentState = CatState.FollowingPlayer;

            Debug.Log("Kedi tekrar oyuncuyu takip ediyor.");
        }
    }
}