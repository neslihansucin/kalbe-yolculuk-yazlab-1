using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public Transform player;
    public Transform[] spawnPoints;

    void Start()
    {
        int randomIndex = Random.Range(0, spawnPoints.Length);

        player.position = spawnPoints[randomIndex].position;
    }
}