using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

public class Side {
    public int id;
    public int lvl;
    public bool exit;
    public bool balcony;
    private Vector3[] _basePoints = new Vector3[2];
    private Vector3[] _balconyPoints = new Vector3[2];
    public List<Bridge> bridges = new List<Bridge>();

    public Platform CreatePlatform(RoomData data) {
        Ray ray = new Ray(BaseMid - Vector3.up * GlobalRoomData.Get.balconyHeight / 2F + Normal * 0.5f, Normal);
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, 100f, RoomGenerator.Instance.layerMask))
            return null;

        Vector3 platformOrigin = ray.origin + (hit.point - ray.origin) / 2f;
        platformOrigin += Vector3.up * GlobalRoomData.Get.balconyHeight / 2F;

        // check if platform colldes with nearby walls & rescale
        Collider[] colliders = Physics.OverlapSphere(platformOrigin, GlobalRoomData.Get.platform_maxRadius, RoomGenerator.Instance.layerMask);
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
        } else {
            potRadius = Random.Range(GlobalRoomData.Get.platform_minRadius, GlobalRoomData.Get.platform_maxRadius);
        }

        if (potRadius < GlobalRoomData.Get.platform_minRadius)
            return null;

        // check for nearby points and merge them
        bool foundNearby = false;
        for (int j = 0; j < data.platforms.Count; j++) {
            Vector3 otherPlatformOrigin = data.platforms[j].origin;
            Vector3 dir = (platformOrigin - otherPlatformOrigin);
            // alreaydy a platform within the radius
            if (dir.magnitude < data.platforms[j].radius + potRadius) {
                foundNearby = true;
                break;
            }
        }
        if (foundNearby)
            return null;

        // add new platform position if found none
        var newPlatform = new Platform();
        newPlatform.radius = potRadius;
        newPlatform.origin = platformOrigin;
        newPlatform.hitPoint = hit.point;
        newPlatform._level = lvl;
        newPlatform.index = data.platforms.Count;
        return newPlatform;
    }

    public void DebugBalcony() {
        Side side = this;
        // can have balcony
        var a = side.GetBalconyPoint(0) + side.BalconyDirection * 0.01f - Vector3.up * GlobalRoomData.Get.balconyHeight / 2F;
        var b = side.GetBalconyPoint(1) - side.BalconyDirection * 0.01f - Vector3.up * GlobalRoomData.Get.balconyHeight / 2F;
        var c = side.GetBasePoint(0) + (side.GetBalconyPoint(0) - side.GetBasePoint(0)).normalized * 0.5f - Vector3.up * GlobalRoomData.Get.balconyHeight / 2F;
        var d = side.GetBasePoint(1) + (side.GetBalconyPoint(1) - side.GetBasePoint(1)).normalized * 0.5f - Vector3.up * GlobalRoomData.Get.balconyHeight / 2F;

        if (
            Physics.Raycast(a, (b - a).normalized, (b - a).magnitude, RoomGenerator.Instance.layerMask)
            ||
            Physics.Raycast(b, (a - b).normalized, (a - b).magnitude, RoomGenerator.Instance.layerMask)
            ||
            Physics.Raycast(side.GetBasePoint(0), (side.GetBalconyPoint(0) - side.GetBasePoint(0)).normalized, (side.GetBalconyPoint(0) - side.GetBasePoint(0)).magnitude, RoomGenerator.Instance.layerMask)
            ||
            Physics.Raycast(side.GetBasePoint(1), (side.GetBalconyPoint(1) - side.GetBasePoint(1)).normalized, (side.GetBalconyPoint(1) - side.GetBasePoint(1)).magnitude, RoomGenerator.Instance.layerMask)
            ) {
            Gizmos.color = Color.red;
        } else {
            Gizmos.color = Color.green;
        }

        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(side.GetBasePoint(0), side.GetBasePoint(0) + (side.GetBalconyPoint(0) - side.GetBasePoint(0)) / 2F);
        Gizmos.DrawLine(side.GetBasePoint(1), side.GetBasePoint(1) + (side.GetBalconyPoint(1) - side.GetBasePoint(1)) / 2F);

    }

    public void DebugPlatformGen() {

        Side side = this;
        foreach (var newBridgeSide in side.bridges) {
            // get mid point between this point and ther other side
            Ray ray = new Ray(newBridgeSide.mid - Vector3.up * GlobalRoomData.Get.balconyHeight / 2F + newBridgeSide.normal * 0.5f, newBridgeSide.normal);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f, RoomGenerator.Instance.layerMask)) {
                // get middle of raycast
                Vector3 platformOrigin = ray.origin + (hit.point - ray.origin) / 2f;
                platformOrigin += Vector3.up * GlobalRoomData.Get.balconyHeight / 2F;

                // check if platform colldes with nearby walls & rescale
                Collider[] colliders = Physics.OverlapSphere(platformOrigin, GlobalRoomData.Get.platform_maxRadius, RoomGenerator.Instance.layerMask);
                float potRadius;
                if (colliders.Length > 0) {
                    Vector3 closestPoint = colliders[0].ClosestPoint(platformOrigin);
                    for (int i = 1; i < colliders.Length; i++) {
                        var tmp = colliders[i].ClosestPoint(platformOrigin);
                        var dis1 = Vector3.Distance(tmp, platformOrigin);
                        var dis2 = Vector3.Distance(closestPoint, platformOrigin);
                        if (dis1 < dis2) {
                            closestPoint = tmp;
                        }
                    }

                    potRadius = (closestPoint - platformOrigin).magnitude - 0.25f;
                    
                    // skip the platform if it's too small (after nearby wall rescaling)
                    if (potRadius < GlobalRoomData.Get.platform_minRadius) {
                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(platformOrigin, closestPoint);
                        Gizmos.DrawWireSphere(closestPoint, potRadius);
                        continue;
                    }
                } else {
                    // default random or default max 
                    //potRadius = Random.Range(GlobalRoomData.Get.platform_minRadius, GlobalRoomData.Get.platform_maxRadius);
                    potRadius = GlobalRoomData.Get.platform_maxRadius;
                }


                var data = RoomGenerator.Instance.GetData;
                // check for nearby points and merge them
                bool foundNearby = false;
                for (int j = 0; j < data.platforms.Count; j++) {
                    Vector3 otherPlatformOrigin = data.platforms[j].origin;
                    Vector3 dir = (platformOrigin - otherPlatformOrigin);
                    // alreaydy a platform within the radius
                    if (dir.magnitude < data.platforms[j].radius + potRadius) {
                        foundNearby = true;
                        break;
                    }
                }
                if (foundNearby) {
                    Gizmos.color = Color.grey;
                    Gizmos.DrawWireSphere(platformOrigin, potRadius);
                    continue;
                }

                Gizmos.color = Color.green;
                Gizmos.DrawLine(ray.origin, platformOrigin);
                Gizmos.DrawSphere(platformOrigin, potRadius);
                var debug_pl = new Platform();
                debug_pl.origin = platformOrigin;
                debug_pl.radius = potRadius;
            }
        }
    }

    public void RemeshBalcony() {
        // recuperation des points

        // on peut enlever eventuellement non ?
        bridges.RemoveAll(x => !x.built);
        var points = new List<Vector3>() {
            GetBasePoint(0),
        };
        for (int i = 0; i < 3; i++) {
            foreach (var item in bridges.FindAll(x => x.side == i)) {
                points.Add(item.left);
                points.Add(item.right);
            }
            if ( i == 0)
                points.Add(GetBalconyPoint(0));
            else if (i == 1)
                points.Add(GetBalconyPoint(1));
            else
                points.Add(GetBasePoint((1)));
        }

        // make mesh
        var balcony = PoolManager.Instance.NewObject("floor", "Balconies");
        var meshFilter = balcony.GetComponentInChildren<MeshFilter>();
        var mesh = PolyDrawer.GetMesh(meshFilter.mesh, points.ToArray());
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        meshFilter.GetComponent<MeshCollider>().sharedMesh = mesh;
        meshFilter.mesh = mesh;
    }

    public bool Blocked(bool drawGizmos = false) {
        var balcony_left = GetBalconyPoint(0) + BalconyDirection * 0.1f - Vector3.up * GlobalRoomData.Get.balconyHeight / 2F;
        var balcony_right = GetBalconyPoint(1) - BalconyDirection * 0.1f - Vector3.up * GlobalRoomData.Get.balconyHeight / 2F;
        var dir_left = GetBalconyPoint(0) - GetBasePoint(0);
        var base_left = GetBasePoint(0) + dir_left * 0.1f + BaseDirection * 0.1f - Vector3.up * GlobalRoomData.Get.balconyHeight / 2F;
        var dir_right = GetBalconyPoint(1) - GetBasePoint(1); 
        var base_right = GetBasePoint(1) + dir_right * 0.1f - BaseDirection * 0.1f - Vector3.up * GlobalRoomData.Get.balconyHeight / 2F;

        bool balconyBlocked = Physics.Linecast(balcony_left, balcony_right, RoomGenerator.Instance.layerMask)
            ||
            Physics.Linecast(balcony_right, balcony_left, RoomGenerator.Instance.layerMask);

        bool leftBlocked = Physics.Linecast(base_left, balcony_left, RoomGenerator.Instance.layerMask)
            ||
            Physics.Linecast(balcony_left, base_left, RoomGenerator.Instance.layerMask);

        bool rightBlocked = Physics.Linecast(base_right, balcony_right, RoomGenerator.Instance.layerMask)
            ||
            Physics.Linecast(balcony_right, base_right, RoomGenerator.Instance.layerMask);

        if ( drawGizmos) {
            Gizmos.color = balconyBlocked ? Color.red : Color.green;
            Gizmos.DrawLine(balcony_left, balcony_right);

            Gizmos.DrawSphere(balcony_left, 0.1f);
            Gizmos.DrawSphere(balcony_right, 0.1f);

            Gizmos.color = leftBlocked ? Color.red : Color.green;
            Gizmos.DrawLine(base_left, balcony_left);

            Gizmos.DrawSphere(base_left, 0.1f);

            Gizmos.color = rightBlocked ? Color.red : Color.green;
            Gizmos.DrawLine(base_right, balcony_right);

            Gizmos.DrawSphere(base_right, 0.1f);

        }
        return balconyBlocked || leftBlocked || rightBlocked;
    }

    public Vector3 GetBasePoint(int i) {
        return _basePoints[i];
    }
    public void SetBasePoint(int i, Vector3 v) {
        _basePoints[i] = v;
    }
    public Vector3 GetBalconyPoint(int i) {
        return _balconyPoints[i];
    }
    public void SetBalconyPoint(int i, Vector3 v) {
        _balconyPoints[i] = v;
    }

    public Vector3 Normal {
        get {
            return Vector3.Cross(BaseDirection, Vector3.up);
        }
    }

    public Vector3 BaseDirection {
        get {
            return (GetBasePoint(1) - GetBasePoint(0)).normalized;
        }
    }

    public Vector3 BalconyDirection {
        get {
            return (GetBalconyPoint(1) - GetBalconyPoint(0)).normalized;
        }
    }


    public Vector3 BaseBalconyDir{
        get {
            return (BalconyMid - BaseMid).normalized;
        }
    }

    public float BaseBalconyDistance {
        get {
            return (BalconyMid - BaseMid).magnitude;
        }
    }

    public float BalconyWidth {
        get {
            return (GetBalconyPoint(1) - GetBalconyPoint(0)).magnitude;
        }
    }

    public float BaseWidth {
        get {
            return (GetBasePoint(1) - GetBasePoint(0)).magnitude;
        }
    }

    public Vector3 BaseMid {
        get {
            return GetBasePoint(0) + BaseDirection * BaseWidth / 2f;
        }
    }

    public Vector3 BalconyMid {
        get {
            return GetBalconyPoint(0) + BalconyDirection * BalconyWidth / 2f;
        }
    }

}
