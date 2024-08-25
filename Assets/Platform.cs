using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Schema;
using UnityEditor.Build;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Analytics;

[System.Serializable]
public class Platform {
    public int id;
    public Vector3 origin;
    public int _level;
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
    public IEnumerator CreateNecessaryLinks() {
        float w = GlobalRoomData.Get.bridgeWidth;
        RoomData data = RoomManager.Instance._data;
        for (int j = -1; j < 2; j++) {
            var lvl = _level + j;
            if (lvl < 0 || lvl >= data.sides.Length)
                continue;
            var bridges = new List<Bridge.Side>();
            for (int sideIndex = 0; sideIndex < data.sides[lvl].Length; sideIndex++) {
                Side side = data.sides[lvl][sideIndex];
                // skip if not floor and no balcony 
                if (lvl > 0 && !side.balcony) continue;

                foreach (var bridge in side.bridgeSides.FindAll(x=>!x.used)) {
                    var newBridge = CreateNewBridgeSide(bridge.mid);
                    // check if path is blocked
                    if (newBridge.Blocked(bridge))
                        continue;
                    // check if platform has room for new bridge
                    if (!CanFitBridgeSide(newBridge))
                        continue;

                    // generate link
                    newBridge.end = bridge;
                    bridgeSides.Add(newBridge);
                    yield return new WaitForEndOfFrame();
                    goto NextLevel;
                }
            }
            NextLevel:
            continue;
        }

    }

    public IEnumerator CreateLinks_Balconies_Floor() {
        float w = GlobalRoomData.Get.bridgeWidth;
        RoomData data = RoomManager.Instance._data;
        List<List<Bridge.Side>> bridges = new List<List<Bridge.Side>>();
        for (int lvl = 0; lvl < data.sides.Length; lvl++) {
            bridges.Add(new List<Bridge.Side>());
            for (int i = 0; i < data.sides[lvl].Length; i++) {
                Side side = data.sides[lvl][i];
                if ( lvl>0 && !side.balcony) continue;
                bridges[lvl].AddRange(side.bridgeSides.FindAll(x => (x.mid - origin).magnitude < 10f));
            }
        }
        int c = bridges.RemoveAll(x => x.Count == 0);
        int safe = 1000;
        int lvlIndex = 0;
        while (bridges.Count > 0) {
            --safe;
            if (safe <= 0) {
                Debug.LogError($"SAFE");
                break;
            }
            int bridgeIndex = Random.Range(0, bridges[lvlIndex].Count);
            Bridge.Side balconySide = bridges[lvlIndex][bridgeIndex];
            /// loop through
            bridges[lvlIndex].RemoveAt(bridgeIndex);
            if (bridges[lvlIndex].Count == 0) {
                bridges.RemoveAt(lvlIndex);
                if (bridges.Count == 0)
                    break;
                lvlIndex = lvlIndex % bridges.Count;
            }

            Bridge.Side newBridge = CreateNewBridgeSide(balconySide.mid);
            if (newBridge.Blocked(balconySide) || !CanFitBridgeSide(newBridge))
                continue;

            // generate link
            newBridge.end = balconySide;
            bridgeSides.Add(newBridge);

            // loop through levels
            lvlIndex = (lvlIndex + 1)%bridges.Count;
            yield return new WaitForEndOfFrame();
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
        Vector3 mid = (dir.normalized * radius);
        Vector3 left = origin + mid + (normal * w / 2f);
        Vector3 right = origin + mid - (normal * w / 2f);
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


        switch (bridgeSides.Count) {
            case 0:
                Debug.Log($"NO LINK : {id}");
                break;
            case 1:
                Debug.Log($"Only 1 Link : {id}");
                break;
            default:
                break;
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

    public void BuildRamps() {
        foreach (var bridgeSide in bridgeSides) {
            if (!bridgeSide.used) {
                // create platform ramp
                Case.NewRamp(bridgeSide.left, bridgeSide.right);
            } else {
                if (!bridgeSide.buildRamp)
                    continue;
                if ( bridgeSide.end == null) {
                    Debug.LogError($"empty bridge : {id}");
                    continue;
                }
                // create bridge ramp
                Case.NewRamp(bridgeSide.left, bridgeSide.end.right);
                Case.NewRamp(bridgeSide.right, bridgeSide.end.left);
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

    public void GizmosLinks() {
        float w = GlobalRoomData.Get.bridgeWidth;
        RoomData data = RoomManager.Instance._data;
        // bridges per level
        List<List<Bridge.Side>> bridges = new List<List<Bridge.Side>>();
        for (int lvl = 0; lvl < data.sides.Length; lvl++) {
            bridges.Add(new List<Bridge.Side>());
            for (int i = 0; i < data.sides[lvl].Length; i++) {
                Side side = data.sides[lvl][i];
                if (lvl > 0 && !side.balcony) continue;
                bridges[lvl].AddRange(side.bridgeSides.FindAll(x => (x.mid - origin).magnitude < 10f));
            }
        }

        // remove empty levels 
        int c = bridges.RemoveAll(x => x.Count == 0);

        // jic
        int safe = 1000;
        int lvlIndex = 0;
        while (bridges.Count > 0) {
            --safe;
            if (safe <= 0) {
                Debug.LogError($"SAFE");
                break;
            }
            int bridgeIndex = Random.Range(0, bridges[lvlIndex].Count);
            Bridge.Side balconySide = bridges[lvlIndex][bridgeIndex];

            /// loop through
            bridges[lvlIndex].RemoveAt(bridgeIndex);
            if (bridges[lvlIndex].Count == 0) {
                bridges.RemoveAt(lvlIndex);
                if (bridges.Count == 0)
                    break;
                lvlIndex = lvlIndex % bridges.Count;
            }

            Bridge.Side newBridge = CreateNewBridgeSide(balconySide.mid);
            

            // check if path is blocked
            if (newBridge.Blocked(balconySide)) {
                if (!RoomManager.Instance.debugBlockedBalconies)
                    continue;
                Gizmos.color = Color.red;
            } else if (!CanFitBridgeSide(newBridge)) {
                if (!RoomManager.Instance.debugInvalidBalconies)
                    continue;
                Gizmos.color = Color.yellow;
            } else {
                if (!RoomManager.Instance.debugValidBalconies)
                    continue;
                Gizmos.color = Color.green;
            }

            // check if platform has room for new bridge
            Gizmos.DrawSphere(balconySide.right, 0.1f);
            Gizmos.DrawSphere(balconySide.left, 0.1f);
            Gizmos.DrawSphere(newBridge.right, 0.1f);
            Gizmos.DrawSphere(newBridge.left, 0.1f);
            Gizmos.DrawLine(newBridge.right, balconySide.left);
            Gizmos.DrawLine(newBridge.left, balconySide.right);

            // loop through levels
            lvlIndex = (lvlIndex + 1) % bridges.Count;
        }
    }
}
