using UnityEngine;
using System.Collections;

public class BreakableWall : MonoBehaviour
{
    public Rigidbody[] wallPieces;
    public float force = 5f;
    public float destroyDelay = 1.5f;

    public void BreakWall()
    {
        Debug.Log("BreakWall çalýþtý!");

        foreach (Rigidbody piece in wallPieces)
        {
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