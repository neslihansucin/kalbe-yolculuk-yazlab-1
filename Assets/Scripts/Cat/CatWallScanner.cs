
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Duvari Raycast ile algilar ve NavMesh
// uzerinden gecerli gecidi belirler.
// MonoBehaviour degildir;
// CatStateMachine tarafindan olusturulur.

public class CatWallScanner
{
    private readonly Transform catTransform;

    public CatWallScanner(Transform catTransform)
    {
        this.catTransform = catTransform;
    }


    public bool TryDetectWall(
        LayerMask wallLayer,
        float wallDetectionDistance,
        float wallDetectionAngle,
        int wallRayCount,
        out Transform detectedWall)
    {
        detectedWall = null;

        Vector3 origin =
            catTransform.position + Vector3.up * 0.35f;

        float nearestDistance = Mathf.Infinity;

        for (int i = 0; i < wallRayCount; i++)
        {
            float angle =
                -wallDetectionAngle / 2f
                + wallDetectionAngle * i / (wallRayCount - 1f);

            Vector3 direction =
                Quaternion.Euler(0f, angle, 0f)
                * catTransform.forward;

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


    public bool TryFindGap(
        Transform obstacleWall,
        out Vector3 selectedGapEntrance,
        out Vector3 selectedGapExit)
    {
        selectedGapEntrance = Vector3.zero;
        selectedGapExit = Vector3.zero;

        Renderer[] renderers =
            obstacleWall.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return false;

        Bounds wallBounds = renderers[0].bounds;

        foreach (Renderer rend in renderers)
        {
            wallBounds.Encapsulate(rend.bounds);
        }

        // Duvarin gercek sag-sol yonu
        Vector3 sideDirection = obstacleWall.right;

        sideDirection.y = 0;

        sideDirection.Normalize();

        // Duvara dik yon
        Vector3 wallDirection =
            Vector3.Cross(
                sideDirection,
                Vector3.up
            ).normalized;

        // Yon ters tarafa bakiyorsa duzelt
        Vector3 catToWall =
            wallBounds.center - catTransform.position;

        catToWall.y = 0;

        if (Vector3.Dot(
            wallDirection,
            catToWall) < 0)
        {
            wallDirection = -wallDirection;
        }

        float wallWidth =
            Mathf.Max(
                wallBounds.size.x,
                wallBounds.size.z
            );

        Vector3 rayOriginBase =
            wallBounds.center - wallDirection * 1f;

        rayOriginBase.y =
            catTransform.position.y + 0.35f;

        // Her deligin merkezini burada tutacagiz
        List<Vector3> gapCenters =
            new List<Vector3>();

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

            bool blocked = Physics.Raycast(
                rayOrigin,
                wallDirection,
                2f,
                ~0,
                QueryTriggerInteraction.Ignore
            );

            // Aciklik bulundu
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
                // Acik bolge bittiyse bir delik olustur
                if (insideGap)
                {
                    Vector3 gapCenter =
                        (gapStart + gapEnd) / 2f;

                    gapCenters.Add(gapCenter);

                    insideGap = false;
                }
            }
        }

        // Duvarin sonunda aciklik devam ediyorsa
        if (insideGap)
        {
            Vector3 gapCenter =
                (gapStart + gapEnd) / 2f;

            gapCenters.Add(gapCenter);
        }

        Debug.Log(
            "Algilanan delik sayisi: " +
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

            // Iki taraf NavMesh ile bagli mi?
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
                    entranceHit.position -
                    catTransform.position,
                    sideDirection
                )
            );

            Debug.Log(
                "Delik merkezi: " +
                gapCenter +
                " | Yatay mesafe: " +
                lateralDistance.ToString("F2")
            );

            // En yakin gecerli deligi sec
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

        return validGapFound;
    }
}
