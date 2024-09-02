using UnityEngine;
using Cinemachine;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour {

    public static CameraController Instance;

    public CameraGroup prefab;

    public List<Vector3> points = new List<Vector3>();

    public List<CameraGroup> cameraGroups = new List<CameraGroup>();

    private void Awake() {
        Instance = this;
    }


    public void NewCameraGroup(List<Vector3> positions) {
        var newCameraGroup = Instantiate(prefab, transform);
        newCameraGroup.SetWaypoints(positions);
        cameraGroups.Add(newCameraGroup);
    }
}
