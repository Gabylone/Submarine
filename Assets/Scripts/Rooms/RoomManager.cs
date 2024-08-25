using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Analytics;

/// <summary>
/// this generates a room with a seed
/// </summary>
public class RoomManager : MonoBehaviour {


    /// <summary>
    /// DATA
    /// </summary>
    [Header("[DATA]")]
    public int start_Seed;
    public RoomData _data;
    public LayerMask layerMask;

    public bool moveRoomOnFinishGeneration;
    public Vector3 nextRoomPosition;
    public Vector3 nextRoomForward;

    [Header("[DEBUG]")]
    /// <summary>
    /// debug
    /// </summary>
    public int debug_currentPlatformIndex = 0;
    public bool debugBalconies = false;
    public bool debugPlatforms = false;
    public bool debugPlatformLinks = false;
    public bool debugPlatformRadius = false;
    public bool debugPlatformSides = false;
    public bool debugPlatformBridges = false;

    public bool debugPlatformPotentialBalconies = false;
    public bool debugValidBalconies = false;
    public bool debugInvalidBalconies = false;
    public bool debugBlockedBalconies = false;
    public bool debugBridges = false;
    public bool debugLadders = false;
    public bool debugHexes = false;


    private void Start() {
        GenerateNewRoom(Vector3.zero, Vector3.forward, start_Seed);
    }

    public void GenerateNewRoom(Vector3 pos, Vector3 frw, int seed) {
        nextRoomPosition = pos;
        nextRoomForward = frw;
        StartCoroutine(GenerateRoom_Coroutine(seed));
    }

    IEnumerator GenerateRoom_Coroutine(int seed) {

        yield return new WaitForEndOfFrame();
        
        // init random
        Random.InitState(seed);

        // generate base data
        _data = new RoomData();
        _data.Generate();

        Bridge.Side.Parent = null;
        PoolManager.Instance.NewGroup($"[ROOM] ({seed})");

        // room position & rotation
        Side entrance = _data.sides[_data.entranceLevel][0];
        PoolManager.Instance.PlaceGroup(entrance.BaseMid, entrance.Normal);

        // Instantiate cases & balconies
        yield return new WaitForEndOfFrame();
        yield return GenerateCases();

        // get all potential platform positions
        yield return new WaitForEndOfFrame();
        yield return GeneratePlatforms();

        yield return new WaitForEndOfFrame();
        yield return BuildPlatforms();

        yield return new WaitForEndOfFrame();
        GenerateRamps();
        yield return new WaitForEndOfFrame();

        // move the parent to the target location

        if (moveRoomOnFinishGeneration) {
            PoolManager.Instance.PlaceGroup(nextRoomPosition, nextRoomForward);
            yield return new WaitForEndOfFrame();

        }
        Player.Instance.GetTransform.position = nextRoomPosition;

        Debug.Log($"Finished Room.");
    }

    IEnumerator GenerateCases() {
        for (int lvl = 0; lvl < _data.sides.Length; lvl++) {
            Side[] sides = _data.sides[lvl];
            /// generate sides

            for (int i = 0; i < sides.Length; i++) {
                Side side = sides[i];
                Case.NewCases(side);
                if (lvl == 0)
                    continue;
                // can have balcony
                var a = side.GetBalconyPoint(0) + side.BaseBalconyDir * 0.1f;
                var b = side.GetBalconyPoint(1) + side.BaseBalconyDir * 0.1f;
                if (!Physics.Linecast(a, b, RoomManager.Instance.layerMask)) {
                    if (side.exit || Random.value > GlobalRoomData.Get.balconyChance) {
                        yield return new WaitForEndOfFrame();
                        // can generate balcony
                        side.balcony = true;
                        Case.NewBalcony(_data, side);
                        yield return new WaitForEndOfFrame();
                    }
                } else {
                    // if not, remove exit, dont generate sides
                    side.exit = false;
                }

                // create cases and balconies for each sides
            }
        }

        for (int lvl = 0; lvl < _data.sides.Length; lvl++) {
            Side[] sides = _data.sides[lvl];
            for (int i = 0; i < sides.Length; i++) {
                Side side = sides[i];
                if (lvl == 0 || side.balcony) {
                    InitBridgeSides(side);
                }
            }
        }
    }

    void InitBridgeSides(Side side) {

        int sideCount = _data.sides[0].Length;

        int pi = side.id == 0 ? sideCount - 1 : side.id - 1;
        if (!_data.sides[side.lvl][pi].balcony) {
            side.bridgeSides.AddRange(GetBridgeSides(side.GetBasePoint(0), side.GetBalconyPoint(0), 0));
        }

        side.bridgeSides.AddRange(GetBridgeSides(side.GetBalconyPoint(0), side.GetBalconyPoint(1), 1));

        if (!_data.sides[side.lvl][(side.id + 1) % sideCount].balcony) {
            side.bridgeSides.AddRange(GetBridgeSides(side.GetBalconyPoint(1), side.GetBasePoint(1), 2));
        }

        if (side.bridgeSides.Count == 0) {
            Debug.Log($"no bridge created");
        }
    }

    List<Bridge.Side> GetBridgeSides(Vector3 start, Vector3 end, int place) {

        var tmpSides = new List<Bridge.Side>();

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

            var nb = new Bridge.Side(left, right);
            nb.place = place;
            tmpSides.Add(nb);
        }
        return tmpSides;
    }

    IEnumerator GeneratePlatforms() {
        // iterate trough levels
        for (int cLvlIndex = 0; cLvlIndex < _data.sides.Length; cLvlIndex++) {
            // iterate through sides
            for (int cSideIndex = 0; cSideIndex < _data.sides[cLvlIndex].Length; cSideIndex++) {
                // pointing current side
                Side side = _data.sides[cLvlIndex][cSideIndex];
                if (!side.balcony)
                    continue;
                foreach (var newBridgeSide in side.bridgeSides) {
                    // get mid point between this point and ther other side
                    Ray ray = new Ray(newBridgeSide.mid - Vector3.up * GlobalRoomData.Get.balconyHeight / 2F + newBridgeSide.normal * 0.5f, newBridgeSide.normal);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 100f, RoomManager.Instance.layerMask)) {
                        Vector3 platformOrigin = ray.origin + (hit.point - ray.origin) / 2f;
                        platformOrigin += Vector3.up * GlobalRoomData.Get.balconyHeight / 2F;

                        // check if platform colldes with nearby walls & rescale
                        Collider[] colliders = Physics.OverlapSphere(platformOrigin, GlobalRoomData.Get.platform_maxRadius, RoomManager.Instance.layerMask);
                        float potRadius;
                        if (colliders.Length > 0) {
                            Vector3 closestPoint = colliders[0].ClosestPoint(platformOrigin);
                            for (int i = 1; i < colliders.Length; i++) {
                                var tmp = colliders[i].ClosestPoint(platformOrigin);
                                var dis1 = Vector3.Distance(tmp, platformOrigin);
                                var dis2 = Vector3.Distance(closestPoint, platformOrigin);
                                if (dis1 < dis2)
                                    closestPoint = tmp;
                            }

                            potRadius = (closestPoint - platformOrigin).magnitude - 0.25f;
                            // skip the platform if it's too small (after nearby wall rescaling)
                            if (potRadius < GlobalRoomData.Get.platform_minRadius)
                                continue;
                        } else {
                            // default random or default max 
                            potRadius = Random.Range(GlobalRoomData.Get.platform_minRadius, GlobalRoomData.Get.platform_maxRadius);
                            //potRadius = GlobalRoomData.Get.platform_maxRadius;
                        }


                        // check for nearby points and merge them
                        bool foundNearby = false;
                        for (int j = 0; j < _data.platforms.Count; j++) {
                            Vector3 otherPlatformOrigin = _data.platforms[j].origin;
                            Vector3 dir = (platformOrigin - otherPlatformOrigin);
                            // alreaydy a platform within the radius
                            if (dir.magnitude < _data.platforms[j].radius + potRadius) {
                                foundNearby = true;
                                break;
                            }
                        }
                        if (foundNearby)
                            continue;

                        // add new platform position if found none
                        var newPlatform = new Platform();
                        newPlatform.radius = potRadius;
                        newPlatform.origin = platformOrigin;
                        newPlatform.hitPoint = hit.point;
                        newPlatform._level = cLvlIndex;
                        newPlatform.id = _data.platforms.Count;
                        _data.platforms.Add(newPlatform);
                        yield return new WaitForEndOfFrame();
                    }
                }
            }

        }
        
    }

    IEnumerator BuildPlatforms() {

        for (int i = 0; i < _data.platforms.Count; i++) {
            var currentPlatform = _data.platforms[i];
            currentPlatform.CheckLinksWithOtherPlatforms(_data.platforms, i);
            yield return new WaitForEndOfFrame();
            yield return currentPlatform.CreateNecessaryLinks();
            yield return currentPlatform.CreateLinks_Balconies_Floor();
            yield return new WaitForEndOfFrame();
            currentPlatform.HandleLonely();
            currentPlatform.DrawMesh();
            yield return new WaitForEndOfFrame();
            currentPlatform.BuildBridges();
            yield return new WaitForEndOfFrame();
        }

        // destroy all invalid bridges
        yield return new WaitForEndOfFrame();

        foreach (var platform in _data.platforms) {
            foreach (var bs in platform.bridgeSides) {
                if (bs.end != null) {
                    if (bs.Blocked(bs.end)) {
                        bs.end.used = false;
                        bs.end = null;
                        bs.used = false;
                        bs.parent.gameObject.SetActive(false);
                        yield return new WaitForEndOfFrame();
                    }
                }
            }
        }


        // set camera
    }

    public void GenerateRamps() {
        foreach (var item in _data.platforms)
            item.BuildRamps();

        for (int lvl = 0; lvl < _data.sides.Length; lvl++) {
            for (int i = 0; i < _data.sides[lvl].Length; i++) {
                Side side = _data.sides[lvl][i];

                if (!side.balcony )
                    continue;

                // balcony side ramps
                int sideCount = _data.sides[lvl].Length;

                //side.bridgeSides.RemoveAll(x => !x.used);

                int pi = side.id == 0 ? sideCount - 1 : side.id - 1;
                if (!_data.sides[side.lvl][pi].balcony)
                    GenerateBalconyRamp(side, side.GetBasePoint(0), side.GetBalconyPoint(0), 0);
                GenerateBalconyRamp(side, side.GetBalconyPoint(0), side.GetBalconyPoint(1), 1);

                if (!_data.sides[side.lvl][(side.id + 1) % sideCount].balcony)
                    GenerateBalconyRamp(side, side.GetBalconyPoint(1), side.GetBasePoint(1), 2);




            }
        }
    }

    void GenerateBalconyRamp(Side side, Vector3 start, Vector3 end, int place) {
        // balcony front ramps
        Vector3 p = start;
        for (int j = 0; j < side.bridgeSides.Count; ++j) {
            if (side.bridgeSides[j].place != place) continue;
            Case.NewRamp(p, side.bridgeSides[j].left);
            p = side.bridgeSides[j].right;
        }
        Case.NewRamp(p, end);
    }

    #region gizmos
    // GIZMOS
    private void OnDrawGizmos() {
        if (_data == null || _data.sides == null)
            return;

        if (_data != null && _data.sides != null) {
            Hex[] hexes = _data.hexes;
            List<Vector3> points = new List<Vector3>();
            for (int i = 0; i < hexes.Length; i++)
                points.AddRange(hexes[i].GetPositions());

            Gizmos.color = Color.grey;

            // DRAW HEX POINTS
            if (debugHexes) {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < _data.hexes.Length; i++) {
                    Hex hex = _data.hexes[i];
                    Vector3[] ps = hex.GetPositions();
                    for (int j = 0; j < ps.Length; ++j) {
                        Gizmos.DrawSphere(ps[j], 0.1f);
                        Gizmos.DrawLine(ps[j], ps[j + 1 == ps.Length ? 0 : j + 1]);
                    }
                }
            }


            if (debugBalconies) {
                // DRAW BRIDGES POINTS
                for (int lvl = 1; lvl < _data.sides.Length; lvl++) {
                    for (int i = 0; i < _data.sides[lvl].Length; i++) {
                        Side side = _data.sides[lvl][i];
                        if (!side.balcony) continue;

                        var a = side.GetBalconyPoint(0) + side.BaseBalconyDir * 0.1f;
                        var b = side.GetBalconyPoint(1) + side.BaseBalconyDir * 0.1f;
                        Gizmos.color = Physics.Linecast(a, b) ? Color.red : Color.green;
                        Gizmos.DrawLine(a, b);
                    }
                }
            }

            if (debugBridges) {
                // DRAW BRIDGES POINTS
                for (int lvl = 0; lvl < _data.sides.Length; lvl++) {
                    for (int i = 0; i < _data.sides[lvl].Length; i++) {
                        Side side = _data.sides[lvl][i];
                        foreach (var bridgeSide in side.bridgeSides) {
                            if (side.balcony) {
                                bridgeSide.Draw(Color.magenta);
                            } else {
                                bridgeSide.Draw(Color.white);
                            }
                        }
                    }
                }

            }

            if (debugPlatforms && _data.platforms.Count > 0) {
                if (debug_currentPlatformIndex < 0) {
                    int index = 0;
                    foreach (var platform in _data.platforms) {
                        DebugPlatform(index, platform);
                        ++index;
                    }
                } else {
                    debug_currentPlatformIndex = debug_currentPlatformIndex % _data.platforms.Count;
                    DebugPlatform(debug_currentPlatformIndex, _data.platforms[debug_currentPlatformIndex]);
                }
            }
        }

        void DebugPlatform(int index, Platform currentPlatform) {
            Gizmos.color = Color.red;
            if (currentPlatform.bridgeSides.Count == 1)
                Gizmos.color = Color.magenta;
            else if (currentPlatform.bridgeSides.Count > 1)
                Gizmos.color = Color.green;

            Handles.Label(currentPlatform.origin + Vector3.up * 0.5f, $"{index}");
            Gizmos.DrawSphere(currentPlatform.origin, 0.2f);

            if (debugPlatformRadius) {
                Gizmos.color = Color.gray;
                Gizmos.DrawWireSphere(currentPlatform.origin, currentPlatform.radius);
            }

            if (debugPlatformLinks) {
                foreach (var item in currentPlatform.debug_LinkedPlatforms) {
                    Gizmos.DrawLine(currentPlatform.origin, item);
                }
            }

            if (debugPlatformPotentialBalconies) {
                currentPlatform.GizmosLinks();
            }

            for (int i = 0; i < currentPlatform.bridgeSides.Count; i++) {
                var brideSide = currentPlatform.bridgeSides[i];
                int nextIndex = i == currentPlatform.bridgeSides.Count - 1 ? 0 : i + 1;
                var v1 = currentPlatform.bridgeSides[i].right;
                var v2 = currentPlatform.bridgeSides[nextIndex].left;

                if (debugPlatformSides) {
                    Gizmos.color = brideSide.used ? Color.red : Color.cyan;
                    Gizmos.DrawLine(v1, v2);
                }

                if (debugPlatformBridges) {
                    if (brideSide.end != null) {
                        brideSide.Blocked(brideSide.end, true);
                    } else {
                        Gizmos.color = Color.red;
                    }
                }
            }
        }
    }
    #endregion


    /// <summary>
    /// SINGLETON
    /// </summary>
    private static RoomManager _inst;
    public static RoomManager Instance {
        get {
            if (_inst == null) {
                _inst = GameObject.FindObjectOfType<RoomManager>();
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

public class ClockwiseBridgeSidesComparer : IComparer<Bridge.Side> {
    public Vector3 origin;
    public Vector3 dir;
    public Vector3 n;
    public int Compare(Bridge.Side b1, Bridge.Side b2) {
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
