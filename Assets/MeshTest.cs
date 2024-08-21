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

    [Range(0f, 1f)]
    public float h = 0f;

    private void OnDrawGizmos() {

        for (int i = 0; i < 4; i++) {
            positions[i] = anchors[i].position;
        }

        var mid = positions[0] + (positions[1] - positions[0]) / 2F;
        var topMid = positions[2] + (positions[3] - positions[2]) / 2F;
        var midDir = topMid - mid;

        var leftDir = positions[2] - positions[0];
        var rightDir = positions[3] - positions[1];

        Gizmos.color = Color.white;
        Gizmos.DrawLine(positions[0], positions[2]);
        Gizmos.DrawLine(positions[1], positions[3]);

        Gizmos.color = Color.gray;
        Gizmos.DrawLine(mid, topMid);

        var lerp = this.h * midDir.magnitude;

        var currLeft = positions[0] + leftDir * this.h;
        var currRight = positions[1] + rightDir * this.h;

        var sideDir = currRight - currLeft;

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(currLeft, 0.1f);
        Gizmos.DrawSphere(currRight, 0.1f);
        Gizmos.DrawSphere(currLeft + (currRight - currLeft) / 2F, 0.15f);

        Gizmos.color = Color.red;
        var normal = Vector3.Cross(midDir.normalized, sideDir.normalized);
        var curr = mid + midDir * this.h;
        Gizmos.DrawRay(curr, normal.normalized);

        rotation.position = curr;
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(curr - normal.normalized, 0.2f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(curr + midDir.normalized, 0.2f);

        rotation.rotation = Quaternion.LookRotation(-normal.normalized, midDir.normalized);

        var bottomNormal = Vector3.Cross(midDir.normalized, (positions[1] - positions[0]).normalized);
        float h = midDir.magnitude + 2f;
        var sidePos = positions[0] + (positions[1] - positions[0]) / 2f;
        var upPos = midDir.normalized * (h / 2f);
        var forPos = bottomNormal * 1f;
        trigger.position = sidePos + upPos + forPos;
        float w = (positions[1] - positions[0]).magnitude + 1f;
        trigger.localScale = new Vector3(w, h, 2f);


        trigger.rotation = Quaternion.LookRotation(bottomNormal, midDir);
    }

}