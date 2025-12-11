using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SelectHue))]
public class HueChangerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SelectHue script = (SelectHue)target;

        if (GUILayout.Button("Apply Hue"))
        {
            script.ApplyHue();
        }
        else if (GUILayout.Button("Apply Random Hue"))
        {
            script.RandomHue();
        }
    }
}