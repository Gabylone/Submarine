using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

/// <summary>
/// this generates a room with a seed
/// </summary>
public class RoomManager : MonoBehaviour {


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

    public bool moveRoomOnFinishGeneration;
    public Vector3 nextRoomPosition;
    public Vector3 nextRoomForward;

    public bool generation_active = false;

    [Header("[DEBUG]")]
    public bool debug_generatePlatforms = false;

    [Header("[GIZMOS]")]
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
    public bool debugAngles = false;
    public bool debugInvalidBalconies = false;
    public bool debugBlockedBalconies = false;
    public bool debugBridges = false;
    public bool debugLadders = false;
    public bool debugHexes = false;
    public bool debug_Camera = false;


    private void Start() {
        GenerateNewRoom(Vector3.zero, Vector3.forward, start_Seed);
    }

    public void GenerateNewRoom(Vector3 pos, Vector3 frw, int seed) {
        nextRoomPosition = pos;
        nextRoomForward = frw;
        StartCoroutine(GenerateRoom_Coroutine(seed));
    }

    IEnumerator GenerateRoom_Coroutine(int seed) {

        generation_active = true;
        yield return new WaitForEndOfFrame();
        
        // init random
        Random.InitState(seed);

        Debug.Log("Generating Room Data");
        // generate base data
        _data = new RoomData();
        GetData.Generate();

        Bridge.Parent = null;
        PoolManager.Instance.NewGroup($"[ROOM] ({seed})");

        // room position & rotation
        Side entrance = GetData.sides[GetData.entranceLevel][0];
        PoolManager.Instance.PlaceGroup(entrance.BaseMid, entrance.Normal);

        // Instantiate cases & balconies
        Debug.Log("Generating Walls");
        yield return new WaitForEndOfFrame();
        yield return GenerateWalls();

        // get all potential platform positions
        if (debug_generatePlatforms) {
            Debug.Log($"Generating Platforms Positions");
            yield return new WaitForEndOfFrame();
            yield return GeneratePlatforms();

            Debug.Log($"Building Platforms");
            yield return new WaitForEndOfFrame();
            yield return BuildPlatforms();

           
        }

        Debug.Log($"Building Bridge Links");
        yield return BuildSideBridges();
        yield return CleanSideBridges();

        yield return new WaitForEndOfFrame();
        Debug.Log($"Generating Ramps");
        yield return GenerateRamps();
        yield return new WaitForEndOfFrame();

        // move the parent to the target location

        if (moveRoomOnFinishGeneration) {
            PoolManager.Instance.PlaceGroup(nextRoomPosition, nextRoomForward);
            yield return new WaitForEndOfFrame();

        }
        Player.Instance.GetTransform.position = nextRoomPosition;

        Debug.Log($"Finished Room.");
        generation_active = false;
    }

    IEnumerator GenerateWalls() {
        for (int lvl = 0; lvl < GetData.sides.Length; lvl++) {
            Side[] sides = GetData.sides[lvl];
            /// generate sides

            for (int i = 0; i < sides.Length; i++) {
                Side side = sides[i];
                if (lvl == 0) {
                    Case.NewCases(side);
                    continue;
                }
                // can have balcony
                var dirA = side.GetBalconyPoint(0) - side.GetBasePoint(0);
                var dirB = side.GetBalconyPoint(1) - side.GetBasePoint(1);
                var a = side.GetBalconyPoint(0) + dirA.normalized * 0.01f - Vector3.up * GlobalRoomData.Get.balconyHeight / 2F;
                var b = side.GetBalconyPoint(1) + dirB.normalized * 0.01f - Vector3.up * GlobalRoomData.Get.balconyHeight / 2F;

                if (
                    Physics.Raycast(a, (b - a).normalized,(b - a).magnitude, layerMask)
                    ||
                    Physics.Raycast(b, (a - b).normalized,  (a - b).magnitude, layerMask)
                    ) {
                    side.exit = false;
                    
                } else {
                    if (side.exit || Random.value < GlobalRoomData.Get.balconyChance)
                        side.balcony = true;
                    yield return new WaitForEndOfFrame();
                    Case.NewBalcony(GetData, side);
                }

                Case.NewCases(side);

                if (side.balcony) {
                    
                }

                // create cases and balconies for each sides
            }
        }

        for (int lvl = 0; lvl < GetData.sides.Length; lvl++) {
            Side[] sides = GetData.sides[lvl];
            for (int i = 0; i < sides.Length; i++) {
                Side side = sides[i];
                if (lvl == 0 || side.balcony) {
                    InitBalconyBridges(side);
                }
            }
        }
    }

    void InitBalconyBridges(Side side) {

        int sideCount = GetData.sides[0].Length;

        int pi = side.id == 0 ? sideCount - 1 : side.id - 1;
        if (!GetData.sides[side.lvl][pi].balcony) {
            side.bridges.AddRange(GetBridgeSides(side.GetBasePoint(0), side.GetBalconyPoint(0), 0));
        }

        side.bridges.AddRange(GetBridgeSides(side.GetBalconyPoint(0), side.GetBalconyPoint(1), 1));

        if (!GetData.sides[side.lvl][(side.id + 1) % sideCount].balcony) {
            side.bridges.AddRange(GetBridgeSides(side.GetBalconyPoint(1), side.GetBasePoint(1), 2));
        }

        if (side.bridges.Count == 0) {
            Debug.Log($"no bridge created");
        }
    }

    List<Bridge> GetBridgeSides(Vector3 start, Vector3 end, int side) {

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

    IEnumerator GeneratePlatforms() {
        // iterate trough levels
        for (int cLvlIndex = 0; cLvlIndex < GetData.sides.Length; cLvlIndex++) {
            // iterate through sides
            for (int cSideIndex = 0; cSideIndex < GetData.sides[cLvlIndex].Length; cSideIndex++) {
                // pointing current side
                Side side = GetData.sides[cLvlIndex][cSideIndex];
                if (!side.balcony)
                    continue;
                foreach (var newBridgeSide in side.bridges) {
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
                        for (int j = 0; j < GetData.platforms.Count; j++) {
                            Vector3 otherPlatformOrigin = GetData.platforms[j].origin;
                            Vector3 dir = (platformOrigin - otherPlatformOrigin);
                            // alreaydy a platform within the radius
                            if (dir.magnitude < GetData.platforms[j].radius + potRadius) {
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
                        newPlatform.index = GetData.platforms.Count;
                        GetData.platforms.Add(newPlatform);
                        yield return new WaitForEndOfFrame();
                    }
                }
            }

        }
        
    }

    IEnumerator BuildPlatforms() {

        for (int i = 0; i < GetData.platforms.Count; i++) {
            var currentPlatform = GetData.platforms[i];
            yield return currentPlatform.CheckLinksWithOtherPlatforms();
            yield return currentPlatform.CreateNecessaryLinks();
            yield return currentPlatform.CreateLinks_Balconies_Floor();
            //currentPlatform.HandleLonely();
            currentPlatform.DrawMesh();
            yield return new WaitForEndOfFrame();
        }
        // destroy all invalid bridges
        yield return new WaitForEndOfFrame();
        yield return CleanPlatformBridges();



        // set camera
    }

    IEnumerator CleanPlatformBridges() {
        foreach (var platform in GetData.platforms) {
            for (int i = platform.bridges.Count - 1; i >= 0; i--) {
                var bridge = platform.bridges[i];
                if (bridge.Blocked(bridge.GetTargetBridge())) {
                    if ( bridge.group == null) {
                        Debug.LogError($"no bridge parent error");
                        continue;
                    }
                    bridge.group.gameObject.SetActive(false);
                    platform.bridges.RemoveAt(i);
                    if (bridge.GetTargetBridge().linkedPlatform != null) {
                        bridge.GetTargetBridge().linkedPlatform.bridges.Remove(bridge.GetTargetBridge());
                        yield return bridge.GetTargetBridge().linkedPlatform.CreateLinks_Balconies_Floor(true);
                    } else {
                        bridge.GetTargetBridge().built = false;
                        bridge.GetTargetBridge().SetTargetBridge(null);
                    }
                    yield return platform.CreateLinks_Balconies_Floor(true);
                    platform.DrawMesh();
                    yield return new WaitForEndOfFrame();
                }
            }
        }
    }

    IEnumerator CleanSideBridges() {

        yield return new WaitForEndOfFrame();
        for (int lvl = 0; lvl < _data.sides.Length; lvl++) {
            for (int i = 0; i < _data.sides[lvl].Length; i++) {
                var side = _data.sides[lvl][i];
                foreach (var bridge in side.bridges) {
                    //if ( bridge.GetTargetBridge() != null && bridge.GetTargetBridge().linkedPlatform != null) {
                    if (bridge.group != null) {
                        if (bridge.Blocked(bridge.GetTargetBridge())) {
                            bridge.GetTargetBridge().SetTargetBridge(null);
                            bridge.GetTargetBridge().built = false;
                            bridge.SetTargetBridge(null);
                            bridge.group.gameObject.SetActive(false);
                            bridge.group = null;
                            bridge.built = false;
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
            for (int i = 0; i < GetData.sides[lvl].Length; i++) {
                Side side = GetData.sides[lvl][i];
                if (!side.balcony)
                    continue;
                foreach (var bridge in side.bridges.FindAll(x=>!x.built))
                    yield return CheckBridges(bridge, lvl);
                yield return new WaitForEndOfFrame();
            }
        }
    }

    IEnumerator CheckBridges(Bridge bridge, int lvl) {
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

        for (int lvl = 0; lvl < GetData.sides.Length; lvl++) {
            for (int i = 0; i < GetData.sides[lvl].Length; i++) {
                Side side = GetData.sides[lvl][i];

                if (!side.balcony )
                    continue;

                // balcony side ramps
                int sideCount = GetData.sides[lvl].Length;

                side.bridges.RemoveAll(x => !x.built);

                int pi = side.id == 0 ? sideCount - 1 : side.id - 1;
                if (!GetData.sides[side.lvl][pi].balcony)
                    GenerateBalconyRamp(side, side.GetBasePoint(0), side.GetBalconyPoint(0), 0);
                GenerateBalconyRamp(side, side.GetBalconyPoint(0), side.GetBalconyPoint(1), 1);

                if (!GetData.sides[side.lvl][(side.id + 1) % sideCount].balcony)
                    GenerateBalconyRamp(side, side.GetBalconyPoint(1), side.GetBasePoint(1), 2);
            }
            yield return new WaitForEndOfFrame();
        }
    }
    public SplineContainer container;


    void GenerateBalconyRamp(Side side, Vector3 start, Vector3 end, int place) {
        // balcony front ramps
        Vector3 p = start;
        for (int j = 0; j < side.bridges.Count; ++j) {
            var bridge = side.bridges[j];
            if (bridge.side != place) continue;
            Case.NewRamp(p, bridge.left);
            p = bridge.right;
            /*if (bridge.link) {
                Case.NewRamp(bridge.left, bridge.GetTargetBridge().right);
                Case.NewRamp(bridge.right, bridge.GetTargetBridge().left);
            }*/
        }
        Case.NewRamp(p, end);
    }

    #region gizmos
    // GIZMOS
    private void OnDrawGizmos() {

        if (debug_Camera) {
            DebugCamera();
        }

        if (generation_active)
            return;

        if (GetData == null || GetData.sides == null)
            return;

        if (debugAngles) {
            DebugAngles();
        }

        if (debugBalconies) {
            Gizmos.color = Color.white;
            foreach (var line in _data.lines) {
                Vector3 left = new Vector3((float)line.nodes[0].x, 0f, (float)line.nodes[0].y);
                Vector3 right = new Vector3((float)line.nodes[1].x, 0f, (float)line.nodes[1].y);
                Gizmos.DrawLine(left, right);
            }

            // DRAW BRIDGES POINTS
            for (int lvl = 1; lvl < GetData.sides.Length; lvl++) {
                for (int i = 0; i < GetData.sides[lvl].Length; i++) {
                    Side side = GetData.sides[lvl][i];

                    var dirA = side.GetBalconyPoint(0) - side.GetBasePoint(0);
                    var dirB = side.GetBalconyPoint(1) - side.GetBasePoint(1);
                    var a = side.GetBalconyPoint(0) + dirA.normalized * 0.01f - Vector3.up * GlobalRoomData.Get.balconyHeight / 2F;
                    var b = side.GetBalconyPoint(1) + dirB.normalized * 0.01f - Vector3.up * GlobalRoomData.Get.balconyHeight/2F ;
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(a, 0.05f);
                    Gizmos.DrawWireSphere(b, 0.05f);
                    RaycastHit hit;
                    if (
                        Physics.Raycast(a, (b-a).normalized, out hit, (b-a).magnitude, layerMask)
                        ||
                        Physics.Raycast(b, (a - b).normalized, out hit, (a - b).magnitude, layerMask
                        )) {
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(hit.point, 0.1f);
                        Gizmos.DrawLine(a, hit.point);
                    } else {
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(a, b);
                    }
                }
            }
        }

        if (debugBridges) {
            // DRAW BRIDGES POINTS
            for (int lvl = 0; lvl < GetData.sides.Length; lvl++) {
                for (int i = 0; i < GetData.sides[lvl].Length; i++) {
                    Side side = GetData.sides[lvl][i];
                    foreach (var bridgeSide in side.bridges) {
                        bridgeSide.Draw();
                    }
                }
            }

        }

        if (debugPlatforms && GetData.platforms.Count > 0) {
            if (debug_currentPlatformIndex < 0) {
                int index = 0;
                foreach (var platform in GetData.platforms) {
                    DebugPlatform(index, platform);
                    ++index;
                }
            } else {
                debug_currentPlatformIndex = debug_currentPlatformIndex % GetData.platforms.Count;
                DebugPlatform(debug_currentPlatformIndex, GetData.platforms[debug_currentPlatformIndex]);
            }
        }

       
    }

    void DebugAngles() {
        int levelindex = 0;
        for (int i = 0; i < _data.sides[levelindex].Length; i++) {
            Side side = _data.sides[levelindex][i];

            var current_side = _data.sides[levelindex][i];
            int previous_index = i == 0 ? _data.sides[levelindex].Length - 1 : (i - 1);
            int nextIndex = (i + 1) % _data.sides[levelindex].Length;
            var previous_side = _data.sides[levelindex][previous_index];
            var next_side = _data.sides[levelindex][nextIndex];

            // prev
            Gizmos.color = Color.white;
            // direction

            for (int j = 0; j < 2; j++) {
                var targetDir = j == 0 ? -previous_side.BaseDirection : next_side.BaseDirection;
                var currDir = j == 0 ? current_side.BaseDirection : -current_side.BaseDirection;

                var dot = Vector3.Dot(targetDir, Vector3.Cross(currDir, Vector3.up));
                bool flat = dot < 0.01f && dot > -0.01f;
                var innerNormal = Vector3.Cross(current_side.BaseDirection, Vector3.up);
                if (!flat) {
                    // get average of the two vectors
                    innerNormal = dot > 0 ? Vector3.Lerp(currDir, targetDir, 0.5f).normalized : -Vector3.Lerp(currDir, targetDir, 0.5f).normalized;
                    if (j == 1)
                        innerNormal = -innerNormal;
                }
                var balconyPoint = current_side.GetBasePoint(j) + (innerNormal * GlobalRoomData.Get.balconyDepth);
                current_side.SetBalconyPoint(j, balconyPoint);
            }


            var angle = Vector3.Angle(-previous_side.BalconyDirection, current_side.BalconyDirection);
            var orientation = Vector3.Dot(-previous_side.BalconyDirection, Vector3.Cross(current_side.BalconyDirection, Vector3.up));
            // -1 : open angle / 1 = closed angle
            // dela merde ici, je voulais enlever les coins trop grands. mais là j'essaye le smooth, et de toutes maniere ça se fait tout seul jsp avé les raycast
            if (angle < GlobalRoomData.Get.angleToLadder) {
                if (orientation > 0) {
                    Gizmos.color = Color.red;
                } else {
                    Gizmos.color = Color.magenta;
                }
            } else {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawSphere(previous_side.GetBalconyPoint(1), 0.4f);
            Handles.Label(previous_side.GetBalconyPoint(1) + Vector3.up * 0.5f, $"{Mathf.RoundToInt(angle)}/{orientation}");

            Gizmos.color = Color.gray;
            Gizmos.DrawLine(current_side.GetBalconyPoint(0), current_side.GetBalconyPoint(1));
        }
    }
    void DebugPlatform(int index, Platform currentPlatform) {
        Gizmos.color = Color.red;
        if (currentPlatform.bridges.Count == 1)
            Gizmos.color = Color.magenta;
        else if (currentPlatform.bridges.Count > 1)
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

        for (int i = 0; i < currentPlatform.bridges.Count; i++) {
            var bridge = currentPlatform.bridges[i];
            int nextIndex = i == currentPlatform.bridges.Count - 1 ? 0 : i + 1;
            var v1 = currentPlatform.bridges[i].right;
            var v2 = currentPlatform.bridges[nextIndex].left;

            if (debugPlatformSides) {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(v1, v2);
                Gizmos.color = bridge.built ? Color.red : Color.green;
                Gizmos.DrawLine(bridge.left, bridge.right);
            }

            if (debugPlatformBridges) {
                if (bridge.GetTargetBridge() != null) {
                    bridge.Blocked(bridge.GetTargetBridge(), true);
                } else {
                    Gizmos.color = Color.red;
                }
            }
        }
    }

    public int currentHexIndex;
    void DebugCamera() {

        if (CameraController.Instance == null || GetData == null) return;
        var playerPos = Player.Instance.GetTransform.position;
        if (GetData.hexes.Length < 2) {
            return;
        }

        // set hex camera anchors
        for (int i = 0; i < GetData.hexes.Length; i++) {
            var hex = GetData.hexes[i];
            hex.camera_Anchors.Clear();
            for (int j = 0; j < hex.points.Length; j++) {
                var dc = (hex.center - hex.points[j]).normalized;
                var a = hex.points[j] - dc * 0.1f + Vector3.up * 0.1f;
                var b = hex.points[(j + 1) % hex.points.Length] + dc * 0.1f + Vector3.up * 0.1f;
                var d = (b - a).normalized;
                a += d * 0.5f;
                b -= d * 0.5f;
                bool blocked =
                    Physics.Raycast(a, (b - a).normalized, (b - a).magnitude, layerMask)
                    ||
                    Physics.Raycast(b, -(b - a).normalized, (b - a).magnitude, layerMask);

                Gizmos.color = blocked ? Color.white : Color.green;
                Gizmos.DrawLine(hex.points[j], hex.points[(j + 1) % hex.points.Length]);

                if (!blocked) {
                    var p = a + (b - a) / 2F;
                    hex.camera_Anchors.Add(p);
                    Gizmos.DrawSphere(p, 0.5f);
                }

            }
        }

        // init links
        var currentHex = GetData.hexes[currentHexIndex];
        for (int i = 0; i < GetData.hexes.Length; i++) {
            if ( i == currentHexIndex) {
                continue;
            }
            var targetHex = GetData.hexes[i];

            var currCameraAnchor = currentHex.GetClosestAnchor(targetHex.center);
            var targetCameraAnchor = targetHex.GetClosestAnchor(currCameraAnchor);
            currCameraAnchor = currentHex.GetClosestAnchor(targetCameraAnchor);

            var blocked = Physics.Linecast(currCameraAnchor, targetCameraAnchor, LayerMask.GetMask("Wall"));

            if (!blocked) {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(currCameraAnchor, targetCameraAnchor);
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
