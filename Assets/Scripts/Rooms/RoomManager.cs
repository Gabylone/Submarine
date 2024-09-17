using Cinemachine.Utility;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;


/*
 * problemes:

- ladder/stairs flat bridges
- bridge to bridge => sort by distance ( trop de pont se construisent sur eux meme )
- setup bridge priority when cleanup ( make prioritaty bridges colored )*/


/// <summary>
/// this generates a room with a seed
/// </summary>
public class RoomGenerator : MonoBehaviour {

    

    Color[] randomColors = new Color[6] {
        Color.yellow,
        Color.cyan,
        Color.red,
        Color.green,
        Color.blue,
        Color.magenta,
    };

    /// <summary>
    /// DATA
    /// </summary>
    [Header("[DATA]")]
    public int start_Seed;
    private RoomData _data;
    public RoomData GetData {
        get {
            return _data;
        }
    }

    public LayerMask layerMask;
    public SplineContainer splineContainer;

    public Transform floor_Transform;
    public Transform ceiling_Transform;

    public bool moveRoomOnFinishGeneration;
    public Vector3 nextRoomPosition;
    public Vector3 nextRoomForward;
    public bool generation_active = false;

    [Header("[DEBUG]")]
    public bool debug_generatePlatforms = false;


    private void Start() {
        Invoke("StartDelay", 0f);
    }

    void StartDelay() {
        GenerateNewRoom(Vector3.zero, Vector3.forward, start_Seed);
    }

    public void GenerateNewRoom(Vector3 pos, Vector3 frw, int seed) {
        nextRoomPosition = pos;
        nextRoomForward = frw;
        generation_active = true;
        // init random
        Random.InitState(seed);

        Debug.Log("Generating Room Data");
        // generate base data
        _data = new RoomData();
        GetData.Generate();

        Bridge.Parent = null;
        PoolManager.Instance.NewRoom($"[ROOM] ({seed})");

        // room position & rotation
        Side entrance = GetData.sides[GetData.entranceLevel][0];
        PoolManager.Instance.PlaceRoom(entrance.BaseMid, entrance.Normal);

        StartCoroutine(GenerateRoom_Coroutine(seed));
    }

    IEnumerator GenerateRoom_Coroutine(int seed) {

        // Instantiate cases & balconies
        yield return GenerateWalls();
        yield return GenerateFloor();

        // balconies
        for (int lvl = GetData.levels_count - 1; lvl >= 1; lvl--)
            yield return GenerateBalconies(GetData.sides[lvl]);

        for (int lvl = GetData.levels_count-1; lvl >= 1; lvl--) {
            yield return GenerateAllPlatformData(lvl);
            yield return BuildPlatforms(lvl);
            yield return new WaitForEndOfFrame();
        }


        /*foreach (var platform in GetData.platforms) {
            if (platform.tmpSides == null) continue;
            for (int lvl = 1; lvl < platform._level; lvl++)
                GetData.sides[lvl].AddRange(platform.tmpSides[lvl]);
        }*/
        // TEMPS
        //yield return BuildSideBridges();
        //yield return CleanSideBridges();
        
        /*foreach (var platform in GetData.platforms) {
            platform.DrawMesh();
        }*/

        //yield return UpdateBalconyMeshes();
        //yield return GenerateRamps();

        // move the parent to the target location

        if (moveRoomOnFinishGeneration) {
            PoolManager.Instance.PlaceRoom(nextRoomPosition, nextRoomForward);
            yield return new WaitForEndOfFrame();
        }
        Player.Instance.GetTransform.position = nextRoomPosition;

        Debug.Log($"Finished Room.");
        generation_active = false;
    }

    IEnumerator GenerateFloor() {
        yield return new WaitForEndOfFrame();
        var topLeft = new Vector3(SideGroup.bounds_Y.x, 0f, SideGroup.bounds_Y.y); ;
        var topRight = new Vector3(SideGroup.bounds_X.y, 0f, SideGroup.bounds_Y.y);
        var bottomLeft = new Vector3(SideGroup.bounds_X.x, 0f, SideGroup.bounds_Y.x);
        var bottomRight = new Vector3(SideGroup.bounds_X.y, 0f, SideGroup.bounds_Y.x);

        floor_Transform.position = topLeft + (bottomRight - topLeft) / 2F;
        floor_Transform.localScale = new Vector3((bottomRight - bottomLeft).magnitude, (bottomLeft - topLeft).magnitude, 1f);
        ceiling_Transform.position = topLeft + (bottomRight - topLeft) / 2F + Vector3.up * GlobalRoomData.Get.sideHeight * (_data.levels_count);
        ceiling_Transform.localScale = new Vector3((bottomRight - bottomLeft).magnitude, (bottomLeft - topLeft).magnitude, 1f);
        yield return new WaitForEndOfFrame();
    }

    IEnumerator GenerateWalls() {
        SideGroup.DrawSides(GetData.sides);
        yield return new WaitForEndOfFrame();
    }

    IEnumerator GenerateBalconies(List<Side> side_level, bool debug = false) {
        int balconiesCreated = 0;
        for (int i = 0; i < side_level.Count; i++) {
            Side side = side_level[i];
            if (side.Blocked())
                continue;

            if ((side.exit || Random.value < GlobalRoomData.Get.balconyChance) || debug) {
                side.balcony = true;
                Case.NewBalcony(side);
                ++balconiesCreated;
                yield return new WaitForEndOfFrame();
            }
        }
        yield return new WaitForEndOfFrame();


        for (int i = 0; i < side_level.Count; i++) {
            Side side = side_level[i];
            int sideCount = side_level.Count;
            int pi = side.id == 0 ? sideCount - 1 : side.id - 1;
            if (!side_level[pi].balcony)
                side.bridges.AddRange(InitBridgeSide(side.GetBasePoint(0), side.GetBalconyPoint(0), 0));

            side.bridges.AddRange(InitBridgeSide(side.GetBalconyPoint(0), side.GetBalconyPoint(1), 1));

            if (!side_level[(side.id + 1) % sideCount].balcony)
                side.bridges.AddRange(InitBridgeSide(side.GetBalconyPoint(1), side.GetBasePoint(1), 2));
        }
    }

    List<Bridge> InitBridgeSide(Vector3 start, Vector3 end, int side) {

        var tmpSides = new List<Bridge>();

        float bridgeWidth = GlobalRoomData.Get.bridgeWidth;
        float bridgeDecal = GlobalRoomData.Get.decalBetweenBridges;

        float xpos = bridgeDecal + bridgeWidth / 2f;

        var dir = (end - start);
        var lenght = dir.magnitude;
        while (xpos + bridgeWidth / 2F < lenght) {
            var origin = start + dir.normalized * xpos;
            var left = origin - dir.normalized * bridgeWidth / 2F;
            var right = origin + dir.normalized * bridgeWidth / 2F;
            xpos += bridgeWidth + bridgeDecal;

            var newBridge = new Bridge(left, right);
            newBridge.side = side;
            tmpSides.Add(newBridge);
        }

        if ( tmpSides.Count == 0 ) {
            var newBridge = new Bridge(start, end);
            newBridge.side = side;
            tmpSides.Add(newBridge);
        }

        return tmpSides;
    }

    IEnumerator GenerateAllPlatformData(int lvl) {
        // iterate through sides
        for (int cSideIndex = 0; cSideIndex < GetData.sides[lvl].Count; cSideIndex++) {
            var newPlatform = GetData.sides[lvl][cSideIndex].CreatePlatform(GetData);
           if ( newPlatform != null) {
                GetData.platforms.Add(newPlatform);
                yield return new WaitForEndOfFrame();
            }
        }
    }

    IEnumerator BuildPlatforms(int lvl) {

        for (int i = 0; i < GetData.platforms.Count; i++) {
            var platform = GetData.platforms[i];
            if (platform._level != lvl)
                continue;

            //yield return platform.CheckLinksWithOtherPlatforms();
            //yield return platform.CreateNecessaryLinks();
            //yield return platform.CreateLinks_Balconies_Floor(lvl);

            platform.CreateBridges();
            //platform.DrawMesh();

            // tower gen
            if (Random.value > GlobalRoomData.Get.platform_TowerChance) {
                platform.BuildTower();
                for (int lvlside = 1; lvlside < platform.tmpSides.Length; lvlside++) {
                    yield return new WaitForEndOfFrame();
                    var sideLevel = platform.tmpSides[lvlside];
                    yield return GenerateBalconies(sideLevel, true);
                }
            }
        }

        // destroy all invalid bridges
        /*yield return new WaitForEndOfFrame();
        yield return CleanPlatformBridges();*/
    }

    IEnumerator CleanPlatformBridges() {
        foreach (var platform in GetData.platforms) {
            for (int i = platform.bridges.Count - 1; i >= 0; i--) {
                var bridge = platform.bridges[i];
                if (bridge.GetTargetBridge() == null)
                    continue;
                if (bridge.Blocked(bridge.GetTargetBridge())) {
                    if ( bridge.bridgeParent == null) {
                        Debug.LogError($"no bridge parent error");
                        continue;
                    }
                    bridge.bridgeParent.gameObject.SetActive(false);
                    platform.bridges.RemoveAt(i);
                    if (bridge.GetTargetBridge().linkedPlatform != null) {
                        bridge.GetTargetBridge().linkedPlatform.bridges.Remove(bridge.GetTargetBridge());
                        yield return bridge.GetTargetBridge().linkedPlatform.CreateLinks_Balconies_Floor(platform._level);
                    } else {
                        bridge.GetTargetBridge().built = false;
                        bridge.GetTargetBridge().SetTargetBridge(null);
                    }
                    yield return platform.CreateLinks_Balconies_Floor(platform._level);
                }
            }
        }
    }

    IEnumerator CleanSideBridges() {
        Debug.Log($"Cleaning Side Bridges");
        for (int lvl = 0; lvl < _data.sides.Length; lvl++) {
            for (int i = 0; i < _data.sides[lvl].Count; i++) {
                var side = _data.sides[lvl][i];
                foreach (var bridge in side.bridges) {
                    if (bridge.bridgeParent != null) {
                        if (bridge.Blocked(bridge.GetTargetBridge())) {
                            bridge.Clean();
                            yield return new WaitForEndOfFrame();
                        }
                    }
                }
            }
        }
    }


    public IEnumerator BuildSideBridges() {
        yield return new WaitForEndOfFrame();
        for (int lvl = 1; lvl < GetData.sides.Length; lvl++) {
            for (int i = 0; i < GetData.sides[lvl].Count; i++) {
                Side side = GetData.sides[lvl][i];
                if (!side.balcony)
                    continue;
                foreach (var bridge in side.bridges.FindAll(x=>!x.built))
                    yield return CheckBridge(bridge, lvl);
            }
        }
        foreach (var platform in GetData.platforms) {
            foreach (var lvl in platform.tmpSides) {
                foreach (var side in lvl) {
                    foreach (var bridge in side.bridges) {
                        yield return CheckBridge(bridge, side.lvl);
                    }
                }
            }
        }
    }

    IEnumerator CheckBridge(Bridge bridge, int lvl) {
        var nearbyBridges = new List<Bridge>();
        foreach (var side in GetData.sides[lvl])
            nearbyBridges.AddRange(side.bridges.FindAll(x => !x.built && (x.mid - bridge.mid).magnitude < 20f));

        foreach (var nearbyBridge in nearbyBridges) {
            if (Bridge.LinkBridges(bridge, nearbyBridge)) {
                bridge.Build(true);
                yield return new WaitForEndOfFrame();
                break;
            }
        }

    }

    IEnumerator GenerateRamps() {
        foreach (var item in GetData.platforms)
            item.BuildRamps();

        yield return new WaitForEndOfFrame();

        // generate ramps on balconies
        for (int lvl = 0; lvl < GetData.sides.Length; lvl++) {
            for (int i = 0; i < GetData.sides[lvl].Count; i++) {
                Side side = GetData.sides[lvl][i];

                if (!side.balcony )
                    continue;

                // balcony side ramps
                int sideCount = GetData.sides[lvl].Count;

                side.bridges.RemoveAll(x => !x.built);

                if (!GetData.GetSide(lvl, i-1).balcony) {
                    GenerateBalconyRamp(side, side.GetBasePoint(0), side.GetBalconyPoint(0), 0);
                }

                GenerateBalconyRamp(side, side.GetBalconyPoint(0), side.GetBalconyPoint(1), 1);

                if (!GetData.GetSide(lvl, i + 1).balcony) {
                    GenerateBalconyRamp(side, side.GetBalconyPoint(1), side.GetBasePoint(1), 2);
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator UpdateBalconyMeshes() {
        yield return new WaitForEndOfFrame();
        PoolManager.Instance.ClearGroup("Balconies");
        for (int levelIndex = 0; levelIndex < GetData.sides.Length; levelIndex++) {
            for (int sideIndex = 0; sideIndex < GetData.sides[levelIndex].Count; sideIndex++) {
                Side side = GetData.sides[levelIndex][sideIndex];
                if (!side.balcony)
                    continue;
                yield return new WaitForEndOfFrame();
                side.RemeshBalcony();
            }
        }

        foreach (var platform in _data.platforms) {
            foreach (var lvl in platform.tmpSides) {
                foreach (var side in lvl) {
                    if (!side.balcony)
                        continue;
                    yield return new WaitForEndOfFrame();
                    side.RemeshBalcony();
                }
            }
        }
    }


    void GenerateBalconyRamp(Side side, Vector3 start, Vector3 end, int place) {

        // balcony front ramps
        Vector3 p = start;
        for (int j = 0; j < side.bridges.Count; ++j) {
            var bridge = side.bridges[j];
            if (bridge.side != place) continue;
            Case.NewRamp(p, bridge.left);
            p = bridge.right;
        }
        Case.NewRamp(p, end);
    }


    /// <summary>
    /// SINGLETON
    /// </summary>
    private static RoomGenerator _inst;
    public static RoomGenerator Instance {
        get {
            if (_inst == null) {
                _inst = GameObject.FindObjectOfType<RoomGenerator>();
            }

            return _inst;
        }
    }
}

public class ClockwiseVector3Comparer : IComparer<Vector3> {
    public Vector3 origin;
    public Vector3 dir;
    public Vector3 n;
    public int Compare(Vector3 v1, Vector3 v2) {
        Vector3 dir1 = v1 - origin;
        float angle1 = Vector3.Angle(dir1, dir);
        if (Vector3.Dot(dir1, n) < 0)
            angle1 = 360f - angle1;

        Vector3 dir2 = v2 - origin;
        float angle2 = Vector3.Angle(dir2, dir);
        if (Vector3.Dot(dir2, n) < 0)
            angle2 = 360f - angle2;

        return angle1.CompareTo(angle2);

    }
}

public class ClockwiseBridgeSidesComparer : IComparer<Bridge> {
    public Vector3 origin;
    public Vector3 dir;
    public Vector3 n;
    public int Compare(Bridge b1, Bridge b2) {
        Vector3 dir1 = b1.left - origin;
        float angle1 = Vector3.Angle(dir1, dir);
        if (Vector3.Dot(dir1, n) < 0)
            angle1 = 360f - angle1;

        Vector3 dir2 = b2.left - origin;
        float angle2 = Vector3.Angle(dir2, dir);
        if (Vector3.Dot(dir2, n) < 0)
            angle2 = 360f - angle2;

        return angle2.CompareTo(angle1);

    }

}
