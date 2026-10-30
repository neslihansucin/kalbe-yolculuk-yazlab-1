
using UnityEngine;
using System.Collections;
using Unity.AI.Navigation;

public class BreakableWall : MonoBehaviour
{
    [Header("Duvar Parcalari")]
    public Rigidbody[] wallPieces;
    public Collider[] oldWallColliders;

    public float force = 5f;
    public float destroyDelay = 1.5f;

    [Header("Patlama Efekti")]
    [SerializeField] private ParticleSystem explosionParticles;

    [Header("NavMesh")]
    [SerializeField] private NavMeshSurface navMeshSurface;

    private bool isBroken = false;

    public void BreakWall()
    {
        if (isBroken)
            return;

        isBroken = true;

        Debug.Log("BreakWall calisti!");

        // Eski duvar colliderlarini kapat
        if (oldWallColliders != null)
        {
            foreach (Collider wallCollider in oldWallColliders)
            {
                if (wallCollider != null)
                    wallCollider.enabled = false;
            }
        }

        // Patlama efektini oynat
        if (explosionParticles != null)
        {
            explosionParticles.Play();
            Debug.Log("Patlama efekti calisti!");
        }
        else
        {
            Debug.LogWarning("Patlama efekti bagli degil!");
        }

        // Duvar parcalarini savur
        if (wallPieces != null)
        {
            foreach (Rigidbody piece in wallPieces)
            {
                if (piece == null)
                    continue;

                piece.isKinematic = false;

                Vector3 randomDirection = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(0.5f, 1.5f),
                    Random.Range(-1f, 1f)
                );

                piece.AddForce(
                    randomDirection.normalized * force,
                    ForceMode.Impulse
                );
            }
        }

        // Parcalari sil
        StartCoroutine(DestroyPieces());

        // Duvar yok edildikten sonra NavMesh'i yenile
        StartCoroutine(UpdateNavMeshAfterBreak());
    }

    private IEnumerator DestroyPieces()
    {
        yield return new WaitForSeconds(destroyDelay);

        if (wallPieces == null)
            yield break;

        foreach (Rigidbody piece in wallPieces)
        {
            if (piece != null)
                Destroy(piece.gameObject);
        }
    }

    private IEnumerator UpdateNavMeshAfterBreak()
    {
        // Duvar parcalarinin silinmesini bekle
        yield return new WaitForSeconds(destroyDelay);

        // Destroy islemlerinin tamamlanmasini bekle
        yield return null;

        if (navMeshSurface == null)
        {
            Debug.LogWarning(
                "NavMeshSurface baglanmamis!"
            );

            yield break;
        }

        if (navMeshSurface.navMeshData == null)
        {
            Debug.LogWarning(
                "NavMeshData bulunamadi!"
            );

            yield break;
        }

        Debug.Log("NavMesh guncelleniyor...");

        // Mevcut NavMesh'i asenkron guncelle
        yield return navMeshSurface.UpdateNavMesh(
            navMeshSurface.navMeshData
        );

        Debug.Log(
            "Duvar kirildi ve NavMesh guncellendi!"
        );
    }
}
