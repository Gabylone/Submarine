using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;

[System.Serializable]
public class Platform {
    public int index;
    public Vector3 origin;
    public int _level;
    public float radius;
    public Vector3 hitPoint;
    public List<Bridge> bridges = new List<Bridge>();
    public Transform mesh_Transform;

    public List<Side>[] tmpSides;


    public List<Vector3> debug_LinkedPlatforms = new List<Vector3>();

    float angle;

    public float Angle {
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
        RoomData data = RoomGenerator.Instance.GetData;
        var levels = new int[3]{_level,_level-1,_level+1};
        for (int i = 0; i < levels.Length; ++i) {
            var lvl = levels[i];
            if (lvl < 0 || lvl >= data.sides.Length)
                continue;
            for (int sideIndex = 0; sideIndex < data.sides[lvl].Count; sideIndex++) {
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

    public void NecessaryBridges_Debug() {
        float w = GlobalRoomData.Get.bridgeWidth;
        RoomData data = RoomGenerator.Instance.GetData;
        var levels = new int[3] { _level, _level - 1, _level + 1 };
        for (int i = 0; i < levels.Length; ++i) {
            var lvl = levels[i];
            if (lvl < 0 || lvl >= data.sides.Length)
                continue;
            for (int sideIndex = 0; sideIndex < data.sides[lvl].Count; sideIndex++) {
                Side side = data.sides[lvl][sideIndex];
                foreach (var balconyBridge in side.bridges.FindAll(x => !x.built)) {
                    var newBridge = CreateNewBridge(balconyBridge.mid);
                    // check if platform has room for new bridge
                    if (!CanFitBridgeSide(newBridge))
                        continue;

                    if (newBridge != null) {
                        newBridge.SetTargetBridge(balconyBridge);
                        newBridge.Draw();
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


    public IEnumerator CreateLinks_Balconies_Floor(int lvl) {
        float w = GlobalRoomData.Get.bridgeWidth;
        RoomData data = RoomGenerator.Instance.GetData;
        List<List<Bridge>> bridge_levels = new List<List<Bridge>>();

        // TEST essayé voir si on crée pas que des lien avec le meme niveau après avoir rajouté un nombre adéqua de lien entre étage
        // add wall bridges
        bridge_levels.Add(new List<Bridge>());
        for (int i = 0; i < data.sides[lvl].Count; i++) {
            Side side = data.sides[lvl][i];
            if (lvl > 0 && !side.balcony) continue;
            bridge_levels[lvl].AddRange(side.bridges.FindAll(x => (x.mid - origin).magnitude < 30f));
        }
        // ad platform bridges
        foreach (var platform in data.platforms) {
            if (platform.index == index) continue;
            if (platform.tmpSides != null && lvl < platform.tmpSides.Length) {
                foreach (var side in platform.tmpSides[lvl])
                    bridge_levels[lvl].AddRange(side.bridges);
            }
        }
        int crotte = 0;
        foreach (var bridgeGroup in bridge_levels) {
            crotte += bridgeGroup.Count;
        }
        int c = bridge_levels.RemoveAll(x => x.Count == 0);


        int safe = 1500;
        int lvlIndex = 0;
        while (bridge_levels.Count > 0) {
            --safe;
            if (safe <= 0) {
                Debug.LogError($"SAFE");
                break;
            }
            int bridgeIndex = Random.Range(0, bridge_levels[lvlIndex].Count);
            Bridge balconyBridge = bridge_levels[lvlIndex][bridgeIndex];
            /// loop through
            bridge_levels[lvlIndex].RemoveAt(bridgeIndex);
            if (bridge_levels[lvlIndex].Count == 0) {
                bridge_levels.RemoveAt(lvlIndex);
                if (bridge_levels.Count == 0)
                    break;
                lvlIndex = lvlIndex % bridge_levels.Count;
            }

            var newBridge = TryAddBridge(balconyBridge);
            if (newBridge != null) {
                // loop through levels
                yield return new WaitForEndOfFrame();
                newBridge.Build();
                lvlIndex = (lvlIndex + 1) % bridge_levels.Count;
            }
        }
    }

    public void Links_Debug() {

    }

    public IEnumerator CheckLinksWithOtherPlatforms() {
        float w = GlobalRoomData.Get.bridgeWidth;
        RoomData data = RoomGenerator.Instance.GetData;
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

    public void PlatformLinks_Debug() {
        float w = GlobalRoomData.Get.bridgeWidth;
        RoomData data = RoomGenerator.Instance.GetData;
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
                Gizmos.DrawLine(newBridge.left, targetPlatformBridge.right);
                Gizmos.DrawLine(newBridge.right, targetPlatformBridge.left);
            }
        }
    }

    public void CreateBridges() {
        Debug.LogWarningFormat($"{Angle}");
        var bridgeCount = 360f / Angle;
        Debug.Log($"bridge count : {bridgeCount}");
        for (int j = 0; j < bridgeCount; j++) {
            var dir = Vector3.forward;
            dir = Quaternion.AngleAxis(Angle * j, Vector3.up) * dir;
            bridges.Add(CreateNewBridge(origin + dir));
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

    public void HandleEmpty() {
        if (bridges.Count == 0) {
            var newBridge = CreateNewBridge(origin + (Vector3.forward).normalized * radius);
            bridges.Add(newBridge);
        }

        if (bridges.Count == 1) {
            var newBridge = CreateNewBridge(origin + (origin - bridges[0].mid).normalized * radius);
            bridges.Add(newBridge);
        }

        if (bridges.Count == 2) {
            var tmpMid = bridges[0].mid + (bridges[1].mid - bridges[0].mid) / 2f;
            var newBridge = CreateNewBridge(origin + (origin - tmpMid).normalized * radius);
            bridges.Add(newBridge);
        }
    }

    public void DrawMesh() {
        if ( bridges.Count == 0 ) return;
        // sort all bridge sides
        ClockwiseBridgeSidesComparer comp = new ClockwiseBridgeSidesComparer();
        comp.origin = origin;
        comp.dir = bridges[0].left - origin;
        comp.n = Vector3.Cross(comp.dir, Vector3.up);
        bridges.Sort((Bridge b1, Bridge b2) => comp.Compare(b1, b2));

        // get all points (sorted from bridge sort)
        Vector3[] points = new Vector3[bridges.Count*2];
        for (int i = 0, j = 0; i < bridges.Count; ++i, j+=2) {
            points[j] = bridges[i].left;
            points[j+1] = bridges[i].right;
        }

        Mesh mesh;
        MeshFilter meshFilter;
        if (mesh_Transform == null) {
            mesh_Transform = PoolManager.Instance.NewObject("floor", "Platforms");
            meshFilter = mesh_Transform.GetComponentInChildren<MeshFilter>();
            mesh = new Mesh() {
                name = "Platform Mesh"
            };
        } else {
            meshFilter = mesh_Transform.GetComponentInChildren<MeshFilter>();
            mesh = meshFilter.mesh;
        }

        meshFilter.mesh = PolyDrawer.GetMesh(mesh, points);
        meshFilter.GetComponentInChildren<MeshCollider>().sharedMesh = meshFilter.mesh;
        return;
    }

    public void BuildTower() {
        var points = new List<Vector3>();
        for (int b = 0; b < bridges.Count; b++) {
            var left = bridges[b].left;
            var right = bridges[b].right;
            left.y = 0f;
            right.y = 0f;
            points.Add(left);
            points.Add(right);
        }

        var newSides = SideGroup.CreateSides(points, _level);
        tmpSides = newSides;


        SideGroup.DrawSides(newSides, true);
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
}
