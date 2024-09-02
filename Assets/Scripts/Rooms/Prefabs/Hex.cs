using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Hex {
    public int sideCount;
    public float radius;
    public Vector3 sizeMult;
    public Vector3 origin;
    public Vector3[] points;
    public List<Vector3> camera_Anchors = new List<Vector3>();

    public Vector3 GetClosestAnchor(Vector3 p) {
        var anchor = camera_Anchors[0];
        for (int i = 1; i < camera_Anchors.Count; i++) {
            var item = camera_Anchors[i];
            if ((item - anchor).sqrMagnitude <
                (p - anchor).sqrMagnitude) {
                anchor = item;
            }
        }

        return anchor;
    }

    public Vector3 center;

    public Vector3[] GetPositions() {
        Vector3 frw = Vector3.forward;

        Vector3[] tmp_points = new Vector3[sideCount];

        Vector3 normal = Vector3.Cross(frw, Vector3.up);

        for (int a = 0; a < sideCount; a++) {
            float angle = 360 / sideCount;

            float z = (float)Mathf.Cos(a * angle * Mathf.PI / 180f) * radius;
            float x = (float)Mathf.Sin(a * angle * Mathf.PI / 180f) * radius;
            tmp_points[a] = origin + -frw * z + -normal * x;
            tmp_points[a] = Rotated(tmp_points[a], -Vector3.up * angle / 2f, origin);

        }

        float distanceToSide = Vector3.Distance(tmp_points[0] + (tmp_points[1] - tmp_points[0]) / 2f, origin);

        for (int a = 0; a < sideCount; a++) {
            tmp_points[a] = frw * distanceToSide + tmp_points[a];

            float dot = Vector3.Dot(normal, tmp_points[a] - origin);

            if (dot == 0f)
                tmp_points[a] = tmp_points[a];
            else if (dot < 0f)
                tmp_points[a] = -normal * sizeMult.x + tmp_points[a];
            else
                tmp_points[a] = normal * sizeMult.y + tmp_points[a];

            if (a > 1) {
                tmp_points[a] = frw * sizeMult.z + tmp_points[a];
            }

        }

        

        points = tmp_points;

        foreach (var p in points)
            center += p;
        center /= points.Length;
        return tmp_points;
    }

    public void Randomize() {
        GlobalRoomData global = GlobalRoomData.Get;
        float size = Random.Range(global.size.x, global.size.y);
        origin = Random.insideUnitSphere * size;
        origin.y = 0f;
        sideCount = Random.Range(global.hexSideCount.x, global.hexSideCount.y);
        radius = Random.Range(global.hexRadius.x, global.hexRadius.y);
        float x = Random.Range(0f, global.sizeMult_max.x);
        float y = Random.Range(0f, global.sizeMult_max.y);
        float z = Random.Range(0f, global.sizeMult_max.z);
        sizeMult = new Vector3(x, y, z);


    }

    public static Vector3 Rotated(Vector3 vector, Quaternion rotation, Vector3 pivot = default(Vector3)) {
        return rotation * (vector - pivot) + pivot;
    }

    public static Vector3 Rotated(Vector3 vector, Vector3 rotation, Vector3 pivot = default(Vector3)) {
        return Rotated(vector, Quaternion.Euler(rotation), pivot);
    }
}
