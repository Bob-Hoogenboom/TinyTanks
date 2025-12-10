using UnityEngine;
using UnityEditor;

public class MaterialApplier : EditorWindow
{
    private Material newMaterial;

    [MenuItem("Tools/Replace Materials on Selected Objects")]
    private static void ShowWindow()
    {
        GetWindow<MaterialApplier>("Replace Materials");
    }

    private void OnGUI()
    {
        GUILayout.Label("Material Replacement Tool", EditorStyles.boldLabel);

        newMaterial = (Material)EditorGUILayout.ObjectField(
            "New Material",
            newMaterial,
            typeof(Material),
            false
        );

        if (GUILayout.Button("Replace Materials"))
        {
            if (newMaterial == null)
            {
                Debug.LogWarning("Please assign a material before replacing.");
                return;
            }

            ReplaceMaterials(newMaterial);
        }
    }

    private static void ReplaceMaterials(Material newMat)
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        int replacedCount = 0;

        foreach (GameObject go in selectedObjects)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer in renderers)
            {
                Undo.RecordObject(renderer, "Replace Materials");
                renderer.sharedMaterial = newMat; // assign single material
                replacedCount++;
            }
        }

        Debug.Log($"Replaced materials on {replacedCount} renderer(s).");
    }
}
