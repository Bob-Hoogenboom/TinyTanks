using UnityEditor;
using UnityEngine;

public class RemoveColliderFromSelected : MonoBehaviour
{
    [MenuItem("Tools/Remove Colliders From Selected %&c")]
    // (optional shortcut: Ctrl+Alt+C)

    private static void RemoveColliders()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("No GameObjects selected.");
            return;
        }

        int removedCount = 0;

        foreach (GameObject go in selectedObjects)
        {
            Collider[] colliders = go.GetComponents<Collider>();

            foreach (Collider c in colliders)
            {
                Undo.DestroyObjectImmediate(c);
                removedCount++;
            }
        }

        Debug.Log($"Removed {removedCount} collider(s) from {selectedObjects.Length} selected object(s).");
    }
}
