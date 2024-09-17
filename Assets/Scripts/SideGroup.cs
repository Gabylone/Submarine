using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

public static class SideGroup
{
    public static Vector2 bounds_X;
    public static Vector2 bounds_Y;
    public static Vector2 bounds_Z;

    public static List<Side>[] CreateSides(List<Vector3> sidePoints, int level_count, int entranceLevel = -1, bool smooth = false) {

        var targetPoints = smooth ? SmoothPoints(sidePoints) : sidePoints;

        // create floor level
        var sides = new List<Side>[level_count];

        for (int i = 0; i < sides.Length; i++)
            sides[i] = new List<Side>();

        // set floor level sides
        for (int i = 0; i < targetPoints.Count; i++) {
            var start = targetPoints[i];
            var end = targetPoints[(i + 1) % targetPoints.Count];
            Side newSide = new Side();
            newSide.id = i;
            newSide.lvl = 0;
            newSide.SetBasePoint(0, start);
            newSide.SetBasePoint(1, end);
            if ( smooth)
                newSide.exit = (Random.value < GlobalRoomData.Get.exitChance) || i == 0 && 0 == entranceLevel;

            sides[0].Add(newSide);
        }
        // set floor level details
        int floorLvl = 0;
        for (int i = 0; i < sides[0].Count; i++) {
            var side = sides[floorLvl][i];
            int previous_index = i == 0 ? sides[floorLvl].Count - 1 : (i - 1);
            int nextIndex = (i + 1) % sides[floorLvl].Count;
            var previous_side = sides[floorLvl][previous_index];
            var next_side = sides[floorLvl][nextIndex];

            for (int j = 0; j < 2; j++) {
                var targetDir = j == 0 ? -previous_side.BaseDirection : next_side.BaseDirection;
                var currDir = j == 0 ? side.BaseDirection : -side.BaseDirection;
                var dot = Vector3.Dot(targetDir, Vector3.Cross(currDir, Vector3.up));
                bool flat = dot < 0.01f && dot > -0.01f;
                var innerNormal = Vector3.Cross(side.BaseDirection, Vector3.up);
                if (!flat) {
                    // get average of the two vectors
                    innerNormal = dot > 0 ? Vector3.Lerp(currDir, targetDir, 0.5f).normalized : -Vector3.Lerp(currDir, targetDir, 0.5f).normalized;
                    if (j == 1)
                        innerNormal = -innerNormal;
                }
                var balconyPoint = side.GetBasePoint(j) + (innerNormal * GlobalRoomData.Get.balconyDepth);
                side.SetBalconyPoint(j, balconyPoint);
            }
        }

        // copy floor level to upper levels
        for (int lvl = 1; lvl < level_count; lvl++) {
            for (int i = 0; i < sides[0].Count; i++) {
                var newSide = new Side();
                newSide.id = i;
                newSide.lvl = lvl;
                var refSide = sides[0][i];
                newSide.SetBasePoint(0, refSide.GetBasePoint(0) + Vector3.up * (lvl * GlobalRoomData.Get.sideHeight));
                newSide.SetBasePoint(1, refSide.GetBasePoint(1) + Vector3.up * (lvl * GlobalRoomData.Get.sideHeight));
                newSide.SetBalconyPoint(0, refSide.GetBalconyPoint(0) + Vector3.up * (lvl * GlobalRoomData.Get.sideHeight));
                newSide.SetBalconyPoint(1, refSide.GetBalconyPoint(1) + Vector3.up * (lvl * GlobalRoomData.Get.sideHeight));
                if(smooth)
                    newSide.exit = (Random.value < GlobalRoomData.Get.exitChance) || i == 0 && lvl == entranceLevel;
                sides[lvl].Add(newSide);
            }
        }
        return sides;
    }

    public static void DrawSides(List<Side>[] sides, bool platform = false) {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var doorVertices = new List<Vector3>();

        for (int i = 0; i < sides[0].Count; i++) {
            var side = sides[0][i];
            var wallCollider = PoolManager.Instance.NewObject("wallCollider", "Walls (Collider)");
            var p = side.BaseMid + (Vector3.up * GlobalRoomData.Get.sideHeight * sides.Length) / 2F - side.Normal * (GlobalRoomData.Get.caseDepth / 2F);
            wallCollider.position = p;
            wallCollider.right = side.BaseDirection;
            wallCollider.localScale = new Vector3(side.BaseWidth, GlobalRoomData.Get.sideHeight * sides.Length, GlobalRoomData.Get.caseDepth);
        }

        for (int lvl = 0; lvl < sides.Length + 1; lvl++) {
            if (lvl == sides.Length) {
                for (int i = 0; i < sides[lvl - 1].Count; i++)
                    vertices.Add(sides[lvl - 1][i].GetBasePoint(0) + Vector3.up * (platform? GlobalRoomData.Get.sideHeight - GlobalRoomData.Get.bridgeHeight : GlobalRoomData.Get.sideHeight));
                break;
            }
            for (int i = 0; i < sides[lvl].Count; i++) {
                var side = sides[lvl][i];
                // vertices
                vertices.Add(side.GetBasePoint(0));

                if (side.exit) {
                    var dir = side.GetBasePoint(1) - side.GetBasePoint(0);
                    var mid = side.GetBasePoint(0) + dir / 2f;
                    // bottom left
                    doorVertices.Add(mid - dir.normalized * GlobalRoomData.Get.doorScale.x / 2F);
                    // top left
                    doorVertices.Add(mid - dir.normalized * GlobalRoomData.Get.doorScale.x / 2F + Vector3.up * GlobalRoomData.Get.doorScale.y);
                    // top right
                    doorVertices.Add(mid + dir.normalized * GlobalRoomData.Get.doorScale.x / 2F + Vector3.up * GlobalRoomData.Get.doorScale.y);
                    // bottom right(
                    doorVertices.Add(mid + dir.normalized * GlobalRoomData.Get.doorScale.x / 2F);
                }

            }
        }

        int doorCount = 0;
        for (int lvl = 0; lvl < sides.Length; lvl++) {
            var sideLevelCount = sides[lvl].Count;
            for (int i = 0; i < sides[lvl].Count; i++) {
                var bottomLeft = (lvl * sideLevelCount) + i;
                var bottomRight = (lvl * sideLevelCount) + ((i + 1) % sideLevelCount);
                var topLeft = ((lvl + 1) * sideLevelCount) + i;
                var topRight = (lvl + 1) * sideLevelCount + ((i + 1) % sideLevelCount);

                // "door"
                if (sides[lvl][i].exit) {
                    var door_bottomLeft = vertices.Count + (doorCount * 4) + 0;
                    var door_topLeft = vertices.Count + (doorCount * 4) + 1;
                    var door_topRight = vertices.Count + (doorCount * 4) + 2;
                    var door_bottomRight = vertices.Count + (doorCount * 4) + 3;

                    // left side
                    triangles.Add(door_bottomLeft);
                    triangles.Add(topLeft);
                    triangles.Add(bottomLeft);

                    triangles.Add(door_bottomLeft);
                    triangles.Add(door_topLeft);
                    triangles.Add(topLeft);

                    // top side
                    triangles.Add(door_topLeft);
                    triangles.Add(door_topRight);
                    triangles.Add(topLeft);

                    triangles.Add(door_topRight);
                    triangles.Add(topRight);
                    triangles.Add(topLeft);

                    // right side
                    triangles.Add(bottomRight);
                    triangles.Add(topRight);
                    triangles.Add(door_topRight);

                    triangles.Add(door_bottomRight);
                    triangles.Add(bottomRight);
                    triangles.Add(door_topRight);

                    ++doorCount;
                } else {
                    triangles.Add(bottomLeft);
                    triangles.Add(topRight);
                    triangles.Add(topLeft);

                    triangles.Add(bottomRight);
                    triangles.Add(topRight);
                    triangles.Add(bottomLeft);

                }
            }
        }

        vertices.AddRange(doorVertices);

        var walls = PoolManager.Instance.NewObject("wall", "Walls");
        var meshFilter = walls.GetComponentInChildren<MeshFilter>();
        var mesh = new Mesh() {
            name = "Walls"
        };

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    public static List<Vector3> SmoothPoints (List<Vector3> points) {
        var spline_c = RoomGenerator.Instance.splineContainer;
        spline_c.Spline.Clear();
        float concaveLenght = 0f;
        for (int i = 0; i < points.Count; i++) {
            var p = points[i];
            var np = points[(i + 1) % points.Count];
            var knot = new BezierKnot(new Unity.Mathematics.float3(p.x, 0f, p.z));
            spline_c.Spline.Add(knot, TangentMode.AutoSmooth, GlobalRoomData.Get.sideTension);
            // getting the lenght
            concaveLenght += (np - p).magnitude;
        }

        // get spline points
        var sideWidth = GlobalRoomData.Get.sideWidth;
        var splineDecal = sideWidth / concaveLenght;
        if (splineDecal <= 0) {
            Debug.LogError($"Side Spline Gen : Loop Break");
            return null;
        }
        var lerp = 0f;
        int a = 100;

        bounds_X = new Vector2(float.PositiveInfinity, float.NegativeInfinity);
        bounds_Y = new Vector2(float.PositiveInfinity, float.NegativeInfinity);
        bounds_Z = new Vector2(float.PositiveInfinity, float.NegativeInfinity);

        var splinePoints = new List<Vector3>();
        while (lerp < 1) {
            var p = spline_c.Spline.EvaluatePosition(lerp);
            splinePoints.Add(p);

            if (p.x < bounds_X.x)
                bounds_X.x = p.x;
            if (p.x > bounds_X.y)
                bounds_X.y = p.x;

            if (p.z < bounds_Y.x)
                bounds_Y.x = p.z;
            if (p.z > bounds_Y.y)
                bounds_Y.y = p.z;

            lerp += splineDecal;
            --a;
            if (a <= 0) {
                Debug.LogError("crotte");
                break;
            }
        }

        return splinePoints;
    }

    public static void DebugSides(List<Side>[] sides) {
        if (sides == null)
            return;
        for (int lvl = 0; lvl < sides.Length; lvl++) {
            for (int sideIndex = 0; sideIndex < sides[lvl].Count; sideIndex++) {
                Side side = sides[lvl][sideIndex];
                if (side.balcony) {
                    Gizmos.color = sideIndex % 2 > 0 ? Color.yellow: Color.magenta;
                    Gizmos.DrawLine(side.GetBasePoint(0), side.GetBalconyPoint(0));
                    Gizmos.DrawLine(side.GetBalconyPoint(0), side.GetBalconyPoint(1));
                    Gizmos.DrawLine(side.GetBalconyPoint(1), side.GetBasePoint(1));
                    Gizmos.DrawSphere(side.BalconyMid, 0.1f);
                } else {
                    Gizmos.color = sideIndex % 2 > 0 ? Color.blue : Color.cyan;
                    Gizmos.DrawLine(side.GetBasePoint(0), side.GetBasePoint(1));
                    Gizmos.DrawSphere(side.BaseMid, 0.1f);
                }
            }
        }
    }


}
