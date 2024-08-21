using UnityEngine;
using Cinemachine;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour {

    public static CameraController Instance;

    public CameraGroup prefab;

    public List<CameraGroup> cameraGroups = new List<CameraGroup>();

    private void Awake() {
        Instance = this;
    }


    public void NewCameraGroup(List<Vector3> positions) {
        var newCameraGroup = Instantiate(prefab, transform);
        newCameraGroup.SetWaypoints(positions);
        cameraGroups.Add(newCameraGroup);
    }

    /*private void Start() {
        cameraPaths = GetComponentsInChildren<CameraPath>();
    }*/

    /*void Update() {
        var closestCamera = cameraPaths[0];
        var playerPos = Player.Instance.GetTransform.position;

        foreach (var cameraPath in cameraPaths) {
            cameraPath.virtualCamera.Priority = 0;
        }

        for (int i = 1; i < cameraPaths.Length; i++) {
            float dis1 = Vector3.Distance(playerPos, cameraPaths[i].virtualCamera.transform.position);
            float dis2 = Vector3.Distance(playerPos, closestCamera.virtualCamera.transform.position);
            if (dis1 < dis2)
                closestCamera = cameraPaths[i];
        }
        closestCamera.virtualCamera.Priority = 11;
    }*/
}
