using Cinemachine.Utility;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class stairTest : MonoBehaviour {

    public MeshFilter filter;
    public Transform parent;

    private void OnDrawGizmos() {
        var ts = parent.GetComponentsInChildren<Transform>();
        var points = new Vector3[ts.Length - 1];
        for (int i = 0; i < points.Length; i++) {
            points[i] = ts[i+1].position;
            Gizmos.DrawSphere(points[i], 0.1f);
        }

        filter.mesh = PolyDrawer.GetMesh(filter.mesh, points);

    }
}
