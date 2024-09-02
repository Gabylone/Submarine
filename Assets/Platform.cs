using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Platform {
    public int index;
    public Vector3 origin;
    public int _level;
    public float radius;
    public Vector3 hitPoint;
    public List<Bridge> bridges = new List<Bridge>();
    public Transform mesh_Transform;

    public List<Vector3> debug_LinkedPlatforms = new List<Vector3>();

    float angle;

    float Angle {
        get {
            if (angle == 0) {
                Bridge bs = CreateNewBridge(origin + Vector3.forward);
                Vector3 aDir1 = bs.left - origin;
                Vector3 aDir2 = bs.mid - origin;
                angle = Vector3.Angle(aDir1, aDir2) * 2f;
            }

            return angle;
        }
    }
    public IEnumerator CreateNecessaryLinks() {
        float w = GlobalRoomData.Get.bridgeWidth;
        RoomData data = RoomManager.Instance.GetData;
        var levels = new int[3]{_level,_level-1,_level+1};
        for (int i = 0; i < levels.Length; ++i) {
            var lvl = levels[i];
            if (lvl < 0 || lvl >= data.sides.Length)
                continue;
            for (int sideIndex = 0; sideIndex < data.sides[lvl].Length; sideIndex++) {
                Side side = data.sides[lvl][sideIndex];
                foreach (var balconyBridge in side.bridges.FindAll(x=>!x.built)) {
                    var newBridge = TryAddBridge(balconyBridge);
                    if (newBridge != null) {
                        yield return new WaitForEndOfFrame();
                        newBridge.Build();
                        goto NextLevel;
                    }
                }
            }
            NextLevel:
            continue;
        }

    }

    public Bridge TryAddBridge(Bridge targetBridge) {
        var newBridge = CreateNewBridge(targetBridge.mid);
        // check if platform has room for new bridge
        if (!CanFitBridgeSide(newBridge))
            return null;

        if (!Bridge.LinkBridges(newBridge, targetBridge))
            return null;

        // generate link
        newBridge.linkedPlatform = this;
        bridges.Add(newBridge);
        return newBridge;
    }


    public IEnumerator CreateLinks_Balconies_Floor(bool keepTrack= false) {
        float w = GlobalRoomData.Get.bridgeWidth;
        RoomData data = RoomManager.Instance.GetData;
        List<List<Bridge>> bridges = new List<List<Bridge>>();


        for (int lvl = 0; lvl < data.sides.Length; lvl++) {
            bridges.Add(new List<Bridge>());
            for (int i = 0; i < data.sides[lvl].Length; i++) {
                Side side = data.sides[lvl][i];
                if ( lvl>0 && !side.balcony) continue;
                bridges[lvl].AddRange(side.bridges.FindAll(x => (x.mid - origin).magnitude < 10f));
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
            Bridge balconyBridge = bridges[lvlIndex][bridgeIndex];
            /// loop through
            bridges[lvlIndex].RemoveAt(bridgeIndex);
            if (bridges[lvlIndex].Count == 0) {
                bridges.RemoveAt(lvlIndex);
                if (bridges.Count == 0)
                    break;
                lvlIndex = lvlIndex % bridges.Count;
            }

            var newBridge = TryAddBridge(balconyBridge);
            if (newBridge != null) {
                // loop through levels
                yield return new WaitForEndOfFrame();
                newBridge.Build();
                lvlIndex = (lvlIndex + 1) % bridges.Count;
                if (keepTrack) {
                    Debug.Log("new bridge after clean");
                }
            }
        }
    }

    public IEnumerator CheckLinksWithOtherPlatforms() {
        float w = GlobalRoomData.Get.bridgeWidth;
        RoomData data = RoomManager.Instance.GetData;
        var tmpPlatforms = data.platforms.OrderBy((p) => (p.origin - origin).sqrMagnitude).ToList();

        for (int i = 0; i < tmpPlatforms.Count; i++) {
            // skip current plt
            if (i == index) continue;
            Platform targetPlatform = tmpPlatforms[i];
            var targetPlatformBridge = targetPlatform.CreateNewBridge(origin);
            var newBridge = TryAddBridge(targetPlatformBridge);
            if (newBridge != null) {
                if (!targetPlatform.CanFitBridgeSide(targetPlatformBridge))
                    continue;
                targetPlatformBridge.linkedPlatform = targetPlatform;
                targetPlatform.bridges.Add(targetPlatformBridge);
                debug_LinkedPlatforms.Add(targetPlatform.origin);
                yield return new WaitForEndOfFrame();
                newBridge.Build();
            }

            
        }

    }

    public Bridge CreateNewBridge(Vector3 otherPlatform) {
        Vector3 dir = otherPlatform - origin;
        dir.y = 0f;

        float w = GlobalRoomData.Get.bridgeWidth;
        Vector3 normal = Vector3.Cross(dir, Vector3.up).normalized;
        Vector3 mid = (dir.normalized * radius);
        Vector3 left = origin + mid + (normal * w / 2f);
        Vector3 right = origin + mid - (normal * w / 2f);
        return new Bridge(left, right);
    }

    public bool CanFitBridgeSide(Bridge newBridge) {
        for (int i = 0; i < bridges.Count; i++) {
            Bridge bridge = bridges[i];

            Vector3 dir1 = newBridge.mid - origin;
            Vector3 dir2 = bridge.mid - origin;

            if (Vector3.Angle(dir1, dir2) < Angle)
                return false;
        }

        return true;
    }

    public void HandleLonely() {
        /*switch (bridges.Count) {
            case 0:
                Debug.Log($"NO LINK : {index}");
                break;
            case 1:
                Debug.Log($"Only 1 Link : {index}");
                break;
            default:
                break;
        }*/
    }

    public void BuildBridges() {
        if (bridges.Count == 0)
            return;

        for (int i = 0; i < bridges.Count; i++) {
            if (bridges[i].built)
                continue;
            bridges[i].Build();
        }
    }

    public void DrawMesh() {
        if (bridges.Count == 0) {
            Debug.Log($"ERROR : Platform with no bridge ?");
            return;
        }

        // sort all bridge sides
        ClockwiseBridgeSidesComparer comp = new ClockwiseBridgeSidesComparer();
        comp.origin = origin;
        comp.dir = bridges[0].left - origin;
        comp.n = Vector3.Cross(comp.dir, Vector3.up);
        bridges.Sort((Bridge b1, Bridge b2) => comp.Compare(b1, b2));

        // get all points (sorted from bridge sort)
        List<Vector3> vertices = new List<Vector3>();
        foreach (var bSide in bridges) {
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

        Mesh mesh;
        MeshFilter meshFilter;

        if (mesh_Transform == null) {
            mesh_Transform = PoolManager.Instance.RequestObject("platform");
            meshFilter = mesh_Transform.GetComponentInChildren<MeshFilter>();
            mesh = new Mesh() {
                name = "Platform Mesh"
            };
        } else {
            meshFilter = mesh_Transform.GetComponentInChildren<MeshFilter>();
            mesh = meshFilter.sharedMesh;
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();

        mesh.uv = uvs;
        mesh.normals = normals;
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshFilter.GetComponentInChildren<MeshCollider>().convex = false;
        meshFilter.GetComponentInChildren<MeshCollider>().sharedMesh = meshFilter.mesh;
    }

    public void BuildRamps() {
        foreach (var bridge in bridges) {
            if (bridge.built) {
               /* if (bridge.type == Bridge.Type.Ladder)
                    continue;
                // create bridge ramp
                Case.NewRamp(bridge.left, bridge.GetTargetBridge().right);
                Case.NewRamp(bridge.right, bridge.GetTargetBridge().left);*/
            } else {
                Case.NewRamp(bridge.left, bridge.right);
            }
        }

        if (bridges.Count < 2)
            return;
        Vector3 o = bridges[0].right;

        for (int i = 1; i < bridges.Count; i++) {
            Case.NewRamp(o, bridges[i].left);
            o = bridges[i].right;
        }

        Case.NewRamp(o, bridges[0].left);

    }

    public void GizmosLinks() {
        float w = GlobalRoomData.Get.bridgeWidth;
        RoomData data = RoomManager.Instance.GetData;
        // bridges per level
        List<List<Bridge>> bridges = new List<List<Bridge>>();
        for (int lvl = 0; lvl < data.sides.Length; lvl++) {
            bridges.Add(new List<Bridge>());
            for (int i = 0; i < data.sides[lvl].Length; i++) {
                Side side = data.sides[lvl][i];
                if (lvl > 0 && !side.balcony) continue;
                bridges[lvl].AddRange(side.bridges.FindAll(x => (x.mid - origin).magnitude < 10f));
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
            Bridge balconyBridge = bridges[lvlIndex][bridgeIndex];

            /// loop through
            bridges[lvlIndex].RemoveAt(bridgeIndex);
            if (bridges[lvlIndex].Count == 0) {
                bridges.RemoveAt(lvlIndex);
                if (bridges.Count == 0)
                    break;
                lvlIndex = lvlIndex % bridges.Count;
            }

            Bridge newBridge = CreateNewBridge(balconyBridge.mid);
            

            // check if path is blocked
            if (newBridge.Blocked(balconyBridge)) {
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
            Gizmos.DrawSphere(balconyBridge.right, 0.1f);
            Gizmos.DrawSphere(balconyBridge.left, 0.1f);
            Gizmos.DrawSphere(newBridge.right, 0.1f);
            Gizmos.DrawSphere(newBridge.left, 0.1f);
            Gizmos.DrawLine(newBridge.right, balconyBridge.left);
            Gizmos.DrawLine(newBridge.left, balconyBridge.right);

            // loop through levels
            lvlIndex = (lvlIndex + 1) % bridges.Count;
        }
    }
}
