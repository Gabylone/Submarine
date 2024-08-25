using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class MeshTest : MonoBehaviour {

    public Transform[] anchors;
    Vector3[] positions = new Vector3[4];

    public Transform rotation;
    public Transform trigger;

    public float width = 1f;

    public Vector3 rotateAngle;

    public float height = 1f;

    [Range(0f, 1f)]
    public float h = 0f;

    public Transform prout;
    private void OnDrawGizmos() {
        RaycastHit hit;
        if (Physics.Raycast(prout.position, -Vector3.up, out hit, height, LayerMask.GetMask("Stair Step"))) {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(prout.position, hit.point);
            Gizmos.DrawSphere(hit.point, 0.1f);
        } else {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(prout.position, -Vector3.up * height);
        }

        /*foreach (var item in ikMainParams) {
            item.DrawGizmos();
        }*/
    }
}