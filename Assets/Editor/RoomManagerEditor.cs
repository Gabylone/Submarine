using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoomGenerator))]
public class RoomManagerEditor : Editor {

    public enum State {
        InitRooms,
        Walls_Floors,
        Main_Balconies,
        Platforms_Init,
        Plaforms_Buil,
    }

    public override void OnInspectorGUI() {
        RoomGenerator rm = (RoomGenerator)target;

        if (GUILayout.Button("randomize room data")) {
            rm.GetData.Generate();
        }

        base.OnInspectorGUI();

    }
}
