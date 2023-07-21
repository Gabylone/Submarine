using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(CameraRoom))]
public class CameraRoomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CameraRoom myTarget = (CameraRoom)target;

        if (GUILayout.Button("Test Camera"))
        {
            myTarget.TestCamera();
        }

        DrawDefaultInspector();
    }
}
