
using UnityEngine;
using UnityEditor;

public class HeartWallCoverEditor : EditorWindow
{
    private Material heartMaterial;

    [MenuItem("Tools/Heart Wall Cover")]
    public static void OpenWindow()
    {
        GetWindow<HeartWallCoverEditor>("Heart Wall Cover");
    }

    private void OnGUI()
    {
        GUILayout.Label("Kalp Duvar Kaplama", EditorStyles.boldLabel);

        heartMaterial = (Material)EditorGUILayout.ObjectField(
            "Damar Materyali",
            heartMaterial,
            typeof(Material),
            false
        );

        if (GUILayout.Button("Secili Duvarlari Kapla"))
        {
            CoverSelectedWalls();
        }
    }

    private void CoverSelectedWalls()
    {
        if (heartMaterial == null)
        {
            Debug.LogWarning("Once damar materyalini sec!");
            return;
        }

        foreach (GameObject wall in Selection.gameObjects)
        {
            MeshFilter originalMesh = wall.GetComponent<MeshFilter>();
            MeshRenderer originalRenderer = wall.GetComponent<MeshRenderer>();

            if (originalMesh == null ||
                originalMesh.sharedMesh == null ||
                originalRenderer == null)
            {
                Debug.LogWarning(wall.name + " uygun bir duvar degil.");
                continue;
            }

            // Daha once kaplandiysa tekrar olusturma.
            if (wall.transform.Find("HeartCover") != null)
            {
                Debug.LogWarning(wall.name + " zaten kaplanmis.");
                continue;
            }

            GameObject cover = new GameObject("HeartCover");
            Undo.RegisterCreatedObjectUndo(cover, "Kalp Kaplamasi Olustur");

            cover.transform.SetParent(wall.transform, false);
            cover.transform.localPosition = Vector3.zero;
            cover.transform.localRotation = Quaternion.identity;
            cover.transform.localScale = Vector3.one;

            MeshFilter coverMesh = cover.AddComponent<MeshFilter>();
            coverMesh.sharedMesh = originalMesh.sharedMesh;

            MeshRenderer coverRenderer = cover.AddComponent<MeshRenderer>();
            coverRenderer.sharedMaterial = heartMaterial;

            // Eski duvarin sadece goruntusunu gizle.
            Undo.RecordObject(originalRenderer, "Eski Duvari Gizle");
            originalRenderer.enabled = false;

            Debug.Log(wall.name + " damar kaplamasi eklendi.");
        }
    }
}
