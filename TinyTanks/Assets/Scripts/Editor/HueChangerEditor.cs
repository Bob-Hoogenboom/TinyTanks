using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RandomHue))]
public class HueChangerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RandomHue script = (RandomHue)target;

        if (GUILayout.Button("Apply Hue"))
        {
            script.ApplyHue();
        }
    }
}