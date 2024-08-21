using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEditor.MemoryProfiler;

public class CameraGroup : MonoBehaviour {
    public static void NewCamera(Vector3[] ts) {
        
    }

    public CinemachineVirtualCamera virtualCamera;
    public CinemachineSmoothPath path;

    public float zoomDistance = 0f;
    public float playerDecal = 0f;

    private void Start() {
        virtualCamera.Follow = Player.Instance.GetTransform;
        virtualCamera.LookAt = Player.Instance.camera_LookAt;
    }

    private void Update() {

        var dir = Player.Instance.GetTransform.position - virtualCamera.transform.position;
        var trackedDolly = virtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
        var y = Player.Instance.transform.position.y + playerDecal;
        trackedDolly.m_PathOffset = new Vector3(0,y,0);
    }

    public void Trigger() {

    }

    public void SetWaypoints(List<Vector3> positions) {
        var waypoints = new List<CinemachineSmoothPath.Waypoint>();
        for (int i = 0; i < positions.Count; i++) {
            var newWP = new CinemachineSmoothPath.Waypoint();
            newWP.position= positions[i];
            waypoints.Add(newWP);
        }

        path.m_Waypoints = waypoints.ToArray();
    }
     
}
