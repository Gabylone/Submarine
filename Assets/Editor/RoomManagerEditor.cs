using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoomManager))]
public class RoomManagerEditor : Editor {
    public override void OnInspectorGUI() {
        RoomManager rm = (RoomManager)target;

        if (GUILayout.Button("randomize room data")) {
            rm.data.Randomize();
        }

        if (GUILayout.Button("Clear Pool")) {
            PoolManager poolManager = FindObjectOfType<PoolManager>();
            poolManager.DebugClear();
        }

        base.OnInspectorGUI();

    }
}
