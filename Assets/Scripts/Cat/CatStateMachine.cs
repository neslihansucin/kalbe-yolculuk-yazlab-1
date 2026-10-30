
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
    private CatWallScanner wallScanner;

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

    [SerializeField]
    private float wallSearchTimeout = 5f;

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

        wallScanner = new CatWallScanner(transform);

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
            Debug.LogWarning(
                "Kedi 5 saniyede geçit bulamadý. Tekrar denenebilir."
            );

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
        if (player == null ||
            agent == null ||
            !agent.isOnNavMesh)
            return;

        // Oyuncunun tam konumu yerine
        // biraz arkasý ve saðýný hedefle
        Vector3 followTarget =
            player.position
            - player.forward * followDistance
            + player.right * 0.8f;

        // Kediye uygun NavMesh noktasý bul
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

        // Yalnýzca yatay mesafeyi kontrol et
        Vector3 difference =
            hit.position - transform.position;

        difference.y = 0f;

        // Hedefe yeterince yaklaþtýysa dur
        if (difference.sqrMagnitude <= 0.36f)
        {
            if (agent.hasPath)
                agent.ResetPath();

            return;
        }

        // Oyuncunun yanýndaki noktaya ilerle
        agent.SetDestination(hit.position);
    }


    void CrossingObstacleState()
    {
        if (obstacleWall == null)
        {
            if (!wallScanner.TryDetectWall(
                wallLayer,
                wallDetectionDistance,
                wallDetectionAngle,
                wallRayCount,
                out obstacleWall))
            {
                Debug.Log("Kedi henüz bir duvar algýlayamadý.");
                return;
            }

            Debug.Log(
                "Kedi duvarý kendi sensörleriyle buldu: "
                + obstacleWall.name
            );
        }

        // Geçit bir kez seçildikten sonra
        // yeniden tarama yapma
        if (!gapSelected)
        {
            if (wallScanner.TryFindGap(
                obstacleWall,
                out selectedGapEntrance,
                out selectedGapExit))
            {
                gapSelected = true;

                agent.SetDestination(selectedGapEntrance);

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
            agent.SetDestination(selectedGapEntrance);

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
        agent.SetDestination(selectedGapExit);

        if (!agent.pathPending &&
            agent.remainingDistance <= 0.2f)
        {
            currentState = CatState.Searching;

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

        // Önce çevrede buton var mý kontrol et
        if (sensor.TryFindButton(out buttonPosition))
        {
            detectedButtonPosition = buttonPosition;
            buttonDetected = true;
            hasExplorationTarget = false;

            agent.ResetPath();

            currentState = CatState.MovingToButton;

            Debug.Log(
                "Buton bulundu. Kedi butona gidiyor."
            );

            return;
        }

        // Kedi zaten bir keþif noktasýna gidiyorsa
        // oraya ulaþmasýný bekle
        if (hasExplorationTarget)
        {
            if (!agent.pathPending &&
                agent.remainingDistance <= 0.3f)
            {
                visitedPoints.Add(explorationTarget);

                hasExplorationTarget = false;

                Debug.Log(
                    "Bu bölge araþtýrýldý."
                );
            }

            return;
        }

        // Buton görünmüyorsa yeni bir bölge ara
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

            Debug.Log(
                "Kedi yeni bir bölgeyi keþfetmeye gidiyor."
            );
        }
        else
        {
            Debug.Log(
                "Yeni keþif noktasý bulunamadý."
            );

            agent.ResetPath();

            gapSelected = false;
            reachedGapEntrance = false;

            selectedGapEntrance = Vector3.zero;
            selectedGapExit = Vector3.zero;

            buttonDetected = false;
            hasExplorationTarget = false;

            visitedPoints.Clear();

            currentState = CatState.FollowingPlayer;

            Debug.Log(
                "Kedi butonu bulamadý, oyuncuya geri dönüyor."
            );
        }
    }


    void MovingState()
    {
        if (!buttonDetected)
        {
            currentState = CatState.Searching;
            return;
        }

        agent.SetDestination(
            detectedButtonPosition
        );

        if (!agent.pathPending &&
            agent.remainingDistance <= 0.5f)
        {
            currentState = CatState.ActivatingButton;

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

        Debug.Log(
            "Kedi butona bastý, görev tamamlandý."
        );
    }


    void TaskCompleteState()
    {
        agent.ResetPath();

        if (Time.time >=
            taskCompleteTime + followAgainDelay)
        {
            currentState = CatState.FollowingPlayer;

            Debug.Log(
                "Kedi tekrar oyuncuyu takip ediyor."
            );
        }
    }
}
