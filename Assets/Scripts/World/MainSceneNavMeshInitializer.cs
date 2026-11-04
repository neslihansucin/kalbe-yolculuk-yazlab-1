
using UnityEngine;
using Unity.AI.Navigation;

[DefaultExecutionOrder(-500)]
[RequireComponent(typeof(NavMeshSurface))]
public class MainSceneNavMeshInitializer : MonoBehaviour
{
    private NavMeshSurface navMeshSurface;

    private void Awake()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
    }

    private void Start()
    {
        if (navMeshSurface == null)
        {
            Debug.LogError("NavMeshSurface bulunamadý!");
            return;
        }

        Debug.Log("MainScene açýldý. NavMesh yenileniyor...");

        navMeshSurface.BuildNavMesh();

        Debug.Log("NavMesh Bake tamamlandý! Kedi hazýr.");
    }
}
