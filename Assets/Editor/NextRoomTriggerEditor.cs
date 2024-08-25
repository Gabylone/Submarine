using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NextRoomTrigger))]
public class NextRoomTriggerEditor : Editor {
    public override void OnInspectorGUI() {
        NextRoomTrigger nextRoomTrigger = (NextRoomTrigger)target;

        if (GUILayout.Button("Generate Next Room")) {
            nextRoomTrigger.GenerateNextRoom();
        }

        base.OnInspectorGUI();

    }
}