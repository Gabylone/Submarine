using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.AssetImporters;
using UnityEngine;

/// <summary>
/// contains tools for actual room generation ( geometry etc... )
/// à faire : bien séparer la data de la génération
/// la génération prend la data et calcule toutes ses variables
/// </summary>
public class RoomGenerator {
    public static RoomData _handledData;
    public static Transform room_Parent;

    public static LayerMask GenLayerMask = LayerMask.GetMask("Wall", "Floor", "Bridge");

    /// <summary>
    /// bridge ? why here ?
    /// </summary>
    public static Vector3 bridge_StartDecal = new Vector3(-0.1f, -0.1f, 0.1f);
    public static float bridge_LineDecal = -0.1f;
    public static List<Bridge.Side> bridge_List = new List<Bridge.Side>();
    static int _currentPlatformIndex = 0;

    public static void NewRoom(RoomData data) {

        _handledData = data;


        // create & set parent
        room_Parent = new GameObject().transform;
        PoolManager.Instance.currentParent = room_Parent;
        room_Parent.name = "Room";

        // place parent to room start point
        Side entrance = data.Sides[data.entranceLevel][0];
        room_Parent.position = entrance.BaseMid;
        room_Parent.forward = entrance.Normal;

        // Instantiate cases & balconies
        for (int lvl = 0; lvl < data.Sides.Length; lvl++) {
            Side[] sides = data.Sides[lvl];
            for (int i = 0; i < sides.Length; i++) {
                Side side = sides[i];
                // create cases and balconies for each sides
                Case.NewCases(side);
                if (side.balcony) {
                    Case.NewBalcony(data, side);
                }
            }
        }


        RoomManager.Instance.onWait += BuildPlaforms;
        RoomManager.Instance.Wait();

    }

    static void BuildPlaforms() {


        // unsubscribe
        RoomManager.Instance.onWait = null;

        _handledData.platforms = GetPotentialPlatforms();

        RoomManager.Instance.BuildPlatform(_currentPlatformIndex);


        // links between platforms
        /*for (int i = 0; i < _handledData.platforms.Count; i++) {
            var item = _handledData.platforms[i];
            _handledData.platforms[i].CheckLinksWithOtherPlatforms(_handledData.platforms, i);
            item.CreatLinksWithBalconies();
            item.RemoveDuplicateBridges();
            item.InitPlatformMesh();
        }*/


        // links between balconies
        /*foreach (var item in _handledData.platforms)
            item.linkWithBalconies();
        // removing double
        foreach (var item in _handledData.platforms)
            item.removeDoubles();
        // build platform meshes
        foreach (var item in _handledData.platforms)
            item.initPlatformMesh();

        // build bridges with pause

        _currentPlatformIndex = 0;
        RoomManager.Instance.onWait = BuildNextPlatforms;
        RoomManager.Instance.Wait();*/
    }

    public static void BuildNextPlatforms() {

        var currentPlatform = _handledData.platforms[_currentPlatformIndex];
        currentPlatform.CheckLinksWithOtherPlatforms(_handledData.platforms, _currentPlatformIndex);
        currentPlatform.CreatLinksWithFloor();
        currentPlatform.CreateLinks_Balconies_Floor();
        currentPlatform.HandleLonely();
        currentPlatform.RemoveDuplicateBridges();
        currentPlatform.DrawMesh();

        _handledData.platforms[_currentPlatformIndex].BuildBridges();
        ++_currentPlatformIndex;
        if ( _currentPlatformIndex == _handledData.platforms.Count ) {
            Debug.Log($"end of platform generations");
            RoomManager.Instance.onWait = BuildRamps;
        }
        RoomManager.Instance.Wait();

    }

    public static void BuildRamps() {
        foreach (var item in _handledData.platforms)
            item.buildRamps();

        for (int lvl = 0; lvl < _handledData.Sides.Length; lvl++) {
            for (int i = 0; i < _handledData.Sides[lvl].Length; i++) {
                Side side = _handledData.Sides[lvl][i];

                if (!side.balcony || side.bridgeSides.Count == 0)
                    continue;

                Vector3 p = side.GetBalconyPoint(0);
                side.bridgeSides.RemoveAll(x => !x.used);
                side.bridgeSides.Sort((Bridge.Side b1, Bridge.Side b2) => Vector3.Distance(side.GetBalconyPoint(0), b1.left).CompareTo(Vector3.Distance(side.GetBalconyPoint(0), b2.left)));
                for (int j = 0; j < side.bridgeSides.Count; ++j) {
                    Case.NewRamp(p, side.bridgeSides[j].left);
                    p = side.bridgeSides[j].right;
                }

                Case.NewRamp(p, side.GetBalconyPoint(1));
            }
        }
    }

    public static List<Platform> GetPotentialPlatforms() {

        float bridgeWidth = GlobalRoomData.Get.bridgeWidth;

        var potentialPlatforms = new List<Platform>();

        // iterate trough levels
        for (int cLvlIndex = 0; cLvlIndex < _handledData.Sides.Length; cLvlIndex++) {
            // iterate through sides
            for (int cSideIndex = 0; cSideIndex < _handledData.Sides[cLvlIndex].Length; cSideIndex++) {
                // pointing current side
                Side side = _handledData.Sides[cLvlIndex][cSideIndex];
                if (!side.balcony)
                    continue;
                foreach (var newBridgeSide in side.bridgeSides) {
                    // get mid point between this point and ther other side
                    Ray ray = new Ray(newBridgeSide.mid - Vector3.up* GlobalRoomData.Get.balconyHeight/2F + newBridgeSide.normal * 0.5f, newBridgeSide.normal);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 100f, RoomGenerator.GenLayerMask)) {
                        Vector3 platformOrigin = ray.origin + (hit.point - ray.origin) / 2f;
                        platformOrigin += Vector3.up * GlobalRoomData.Get.balconyHeight / 2F;

                        // check if platform colldes with nearby walls & rescale
                        Collider[] colliders = Physics.OverlapSphere(platformOrigin, GlobalRoomData.Get.platform_maxRadius, RoomGenerator.GenLayerMask);
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
                        for (int j = 0; j < potentialPlatforms.Count; j++) {
                            Vector3 otherPlatformOrigin  = potentialPlatforms[j].origin;
                            Vector3 dir = (platformOrigin- otherPlatformOrigin);
                            // alreaydy a platform within the radius
                            if (dir.magnitude < potentialPlatforms[j].radius + potRadius) {
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
                        potentialPlatforms.Add(newPlatform);
                    }
                }
            }

        }
        return potentialPlatforms;
    }
}
