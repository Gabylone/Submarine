using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.Analytics;

[System.Serializable]
public class Platform {
    public Vector3 origin;
    public float radius;
    public Vector3 hitPoint;
    public List<Bridge.Side> bridgeSides = new List<Bridge.Side>();

    public List<Vector3> debug_LinkedPlatforms = new List<Vector3>();

    float angle;

    float Angle {
        get {
            if (angle == 0) {
                Bridge.Side bs = CreateNewBridgeSide(origin + Vector3.forward);
                Vector3 aDir1 = bs.left - origin;
                Vector3 aDir2 = bs.mid - origin;
                angle = Vector3.Angle(aDir1, aDir2) * 2f;
            }

            return angle;
        }
    }

    public void CreatLinksWithFloor() {
        float w = GlobalRoomData.Get.bridgeWidth;
        RoomData data = RoomManager.Instance.debug_data;
        int max = 1;
        for (int i = 0; i < data.Sides[0].Length; i++) {
            Side side = data.Sides[0][i];

            for (int b = 0; b < side.bridgeSides.Count; b++) {

                Bridge.Side endSide = side.bridgeSides[b];
                Bridge.Side startSide = CreateNewBridgeSide(endSide.mid);

                if (startSide.Blocked(endSide))
                    continue;

                if (!CanFitBridgeSide(startSide))
                    continue;


                startSide.end = endSide;
                bridgeSides.Add(startSide);
                --max;
                break;
            }

            if (max <= 0)
                break;
        }
    }

    public void CreateLinks_Balconies_Floor() {
        float w = GlobalRoomData.Get.bridgeWidth;

        RoomData data = RoomManager.Instance.debug_data;
        List<Bridge.Side> balconySides = new List<Bridge.Side>();
        for (int lvl = 0; lvl < data.Sides.Length; lvl++) {
            for (int i = 0; i < data.Sides[lvl].Length; i++) {
                Side side = data.Sides[lvl][i];

                if (lvl == 0 || side.balcony) {
                    // floor link
                    foreach (var bSide in side.bridgeSides)
                        balconySides.AddRange(side.bridgeSides.FindAll(x => !x.used && Vector3.Distance(x.mid, origin) < GlobalRoomData.Get.platform_maxDis));
                }
            }
        }

        if (balconySides.Count <= 0)
            return;

        while (balconySides.Count > 0) {
            int i = Random.Range(0, balconySides.Count);
            Bridge.Side balconySide = balconySides[i];

            balconySides.RemoveAt(i);

            Bridge.Side platformBridgeSide = CreateNewBridgeSide(balconySide.mid);

            if (platformBridgeSide.Blocked(balconySide))
                continue;

            if (!CanFitBridgeSide(platformBridgeSide))
                continue;
            platformBridgeSide.end = balconySide;
            bridgeSides.Add(platformBridgeSide);
        }
    }

    public void CheckLinksWithOtherPlatforms(List<Platform> platforms, int c) {
        float w = GlobalRoomData.Get.bridgeWidth;

        for (int i = 0; i < platforms.Count; i++) {
            // skip current plt
            if (i == c) continue;

            Platform targetPlatform = platforms[i];

            // create bridge sides
            Bridge.Side bs_Start = CreateNewBridgeSide(targetPlatform.origin);
            Bridge.Side bs_End = targetPlatform.CreateNewBridgeSide(origin);

            if (bs_Start.Blocked(bs_End))
                continue;
            // check if other plt already has side at this location
            Bridge.Side dup = targetPlatform.bridgeSides.Find(
                x =>
                x.left == bs_End.left && x.right == bs_End.right);
            if (dup == null) {
                // if it doesn't, create a link ( which will result in a bridge )
                bs_Start.end = bs_End;
                bs_Start.link = true;

                if (!CanFitBridgeSide(bs_Start))
                    continue;
                if (!targetPlatform.CanFitBridgeSide(bs_End))
                    continue;
                if (Vector3.Dot(bs_Start.normal, bs_Start.dir) < 0)
                    continue;

                bridgeSides.Add(bs_Start);
                targetPlatform.bridgeSides.Add(bs_End);
                debug_LinkedPlatforms.Add(targetPlatform.origin);
            }
        }

    }

    public Bridge.Side CreateNewBridgeSide(Vector3 otherPlatform) {
        Vector3 dir = otherPlatform - origin;
        dir.y = 0f;

        float w = GlobalRoomData.Get.bridgeWidth;
        Vector3 normal = Vector3.Cross(dir, Vector3.up).normalized;
        Vector3 decal = (dir.normalized * radius);
        Vector3 left = origin + decal + (normal * w / 2f);
        Vector3 right = origin + decal - (normal * w / 2f);
        return new Bridge.Side(left, right);
    }

    public bool CanFitBridgeSide(Bridge.Side side) {
        for (int i = 0; i < bridgeSides.Count; i++) {
            Bridge.Side bs = bridgeSides[i];

            Vector3 dir1 = side.mid - origin;
            Vector3 dir2 = bs.mid - origin;

            if (Vector3.Angle(dir1, dir2) < Angle)
                return false;
        }

        return true;
    }

    public void HandleLonely() {
        if (bridgeSides.Count == 1) {
            Debug.Log($"handled loney");
            Bridge.Side s = bridgeSides[0];
            //CreateNewBridgeSide(origin - (s.mid-origin), Vector3.forward);
        }
    }

    public void RemoveDuplicateBridges() {
        for (int i = 0; i < bridgeSides.Count; i++) {
            Bridge.Side bs1 = bridgeSides[i];

            for (int j = i + 1; j < bridgeSides.Count; j++) {
                Bridge.Side bs2 = bridgeSides[j];

                Vector3 dir1 = bs1.mid - origin;
                Vector3 dir2 = bs2.mid - origin;

                if (Vector3.Angle(dir1, dir2) < Angle) {
                    if (bs1.link && bs2.link) {
                        if (bs1.end == null || bs2.end == null)
                            continue;
                        float d1 = Vector3.Distance(bs1.mid, bs1.end.mid);
                        float d2 = Vector3.Distance(bs1.mid, bs2.end.mid);
                        if (d1 < d2)
                            bs2.valid = false;
                        else
                            bs1.valid = false;
                    } else {
                        if (bs2.link)
                            bs1.valid = false;
                        else
                            bs2.valid = false;
                    }
                }

            }
        }

        for (int i = bridgeSides.Count - 1; i >= 0; i--) {
            if (!bridgeSides[i].valid) {
                // si t'as pas vu ça pendant un moment c'ets que la fonction sert à r
                Debug.LogError($"removed invalid bridge");
                bridgeSides.RemoveAt(i);
            }
        }
    }

    public void BuildBridges() {
        if (bridgeSides.Count == 0)
            return;

        for (int i = 0; i < bridgeSides.Count; i++) {
            if (bridgeSides[i].used)
                continue;
            bridgeSides[i].Build();
        }
    }

    public void DrawMesh() {
        if (bridgeSides.Count == 0) {
            Debug.Log($"no bridge platform ?");
            return;
        }

        Bridge.Side bs = bridgeSides.Find(x => !x.blocked);
        if (bs == null) {
            Debug.Log($"all bridges blocked ?");
            return;
        }

        // sort all bridge sides
        ClockwiseBridgeSidesComparer comp = new ClockwiseBridgeSidesComparer();
        comp.origin = origin;
        comp.dir = bridgeSides[0].left - origin;
        comp.n = Vector3.Cross(comp.dir, Vector3.up);
        bridgeSides.Sort((Bridge.Side b1, Bridge.Side b2) => comp.Compare(b1, b2));

        // get all points (sorted from bridge sort)
        List<Vector3> vertices = new List<Vector3>();
        foreach (var bSide in bridgeSides) {
            vertices.Add(bSide.left);
            vertices.Add(bSide.right);
        }

        // calc center
        var center = Vector3.zero;
        foreach (var vertex in vertices)
            center += vertex;
        center /= vertices.Count;
        vertices.Insert(0, center);

        List<int> triangles = new List<int>();
        for (int i = 1; i < vertices.Count; i++) {
            var nextIndex = i + 1 == vertices.Count ? 1 : i + 1;
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(nextIndex);
        }

        int c = vertices.Count;
        for (int i = 0; i < c; i++)
            vertices.Add(vertices[i] - Vector3.up * GlobalRoomData.Get.balconyHeight);


        for (int i = c; i < vertices.Count; i++) {
            var nextIndex = i + 1 == vertices.Count ? c + 1 : i + 1;
            triangles.Add(nextIndex);
            triangles.Add(i);
            triangles.Add(c);
        }

        for (int i = 1; i < c; i++) {

            if (i == c - 1) {
                triangles.Add(i + c);
                triangles.Add(1);
                triangles.Add(i);
                triangles.Add(i + c);
                triangles.Add(1 + c);
                triangles.Add(1);
                continue;
            }

            triangles.Add(i + c);
            triangles.Add(i + 1);
            triangles.Add(i);

            triangles.Add(i + c);
            triangles.Add(i + c + 1);
            triangles.Add(i + 1);

        }

        Vector3[] normals = new Vector3[vertices.Count];
        var t = center + Vector3.up * GlobalRoomData.Get.balconyHeight / 2f;
        for (int i = 0; i < vertices.Count; i++) {
            normals[i] = Vector3.up;
        }


        Vector2[] uvs = new Vector2[vertices.Count];
        uvs[0] = new Vector2(0f, 0f);


        var mesh = new Mesh() {
            name = "Platform Mesh"
        };

        mesh.Clear();
        mesh.vertices = vertices.ToArray();

        mesh.uv = uvs;
        mesh.normals = normals;
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();

        var platform_Transform = PoolManager.Instance.RequestObject("platform");
        var meshFilter = platform_Transform.GetComponentInChildren<MeshFilter>();

        meshFilter.mesh = mesh;
        meshFilter.GetComponentInChildren<MeshCollider>().convex = false;
        meshFilter.GetComponentInChildren<MeshCollider>().sharedMesh = meshFilter.mesh;
    }

    public void buildRamps() {
        foreach (var item in bridgeSides) {
            if (!item.used) {
                // create platform ramp
                Case.NewRamp(item.left, item.right);
            } else {
                // create bridge ramp
                Case.NewRamp(item.left, item.end.right);
                Case.NewRamp(item.right, item.end.left);
            }
        }

        if (bridgeSides.Count < 2)
            return;
        Vector3 o = bridgeSides[0].right;

        for (int i = 1; i < bridgeSides.Count; i++) {
            Case.NewRamp(o, bridgeSides[i].left);
            o = bridgeSides[i].right;
        }

        Case.NewRamp(o, bridgeSides[0].left);

    }
}
