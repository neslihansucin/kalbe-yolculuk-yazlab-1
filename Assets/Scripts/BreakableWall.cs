using UnityEngine;
using System.Collections;

public class BreakableWall : MonoBehaviour
{
    public Rigidbody[] wallPieces;
    public Collider[] oldWallColliders;

    public float force = 5f;
    public float destroyDelay = 1.5f;

    [SerializeField] private ParticleSystem explosionParticles;

    private bool isBroken = false;

    public void BreakWall()
    {
        if (isBroken)
            return;

        isBroken = true;

        Debug.Log("BreakWall çalýþtý!");

        // Eski görünmez duvarlarýn Collider'larýný kapat
        foreach (Collider wallCollider in oldWallColliders)
        {
            if (wallCollider != null)
                wallCollider.enabled = false;
        }
        if (explosionParticles != null)
        {
            explosionParticles.Play();
            Debug.Log("Küçük taþ patlama efekti çalýþtýrýldý!");
        }
        else
        {
            Debug.LogWarning("Patlama efekti baðlý deðil!");
        }

        // Duvar parçalarýný savur
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

        StartCoroutine(DestroyPieces());
    }

    private IEnumerator DestroyPieces()
    {
        yield return new WaitForSeconds(destroyDelay);

        foreach (Rigidbody piece in wallPieces)
        {
            if (piece != null)
                Destroy(piece.gameObject);
        }
    }
}