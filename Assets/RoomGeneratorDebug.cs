using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

public class RoomGeneratorDebug : MonoBehaviour
{
    public LayerMask layerMask;

    [Header("[Bridge]")]
    public bool bridge_Active = false;
    public int bridge_SideLevel;
    public int bridge_SideIndex;
    public int bridge_Index;
    public int bridge_PlatformIndex = -1;


    [Header("[GIZMOS]")]
    /// <summary>
    /// debug
    /// </summary>
    public int debug_currentPlatformIndex = 0;
    public bool debugConcave = false;
    public bool debugSides = false;
    public bool displayBalconyRaycasts = false;
    public bool debugBlockedBalconies = false;
    public bool debugBridges = false;
    public bool debugLadders = false;
    public bool debugHexes = false;
    public bool debug_Camera = false;

    [Header("Platforms")]
    public bool platform_active = false;
    public bool platform_radius = false;
    public bool platform_PlatformToBridges = false;
    public bool platform_PlatformToPlatform = false;
    public bool platform_NecessaryLinks = false;
    public bool platform_Sides = false;
    public bool platform_Bridges = false;
    // ?
    public bool debugValidBalconies = false;
    public bool debugAngles = false;
    public bool debugPlatformGeneration = false;
    public bool debugInvalidBalconies = false;
    public bool debugTowers = false;

    private RoomData _data;

    #region gizmos
    // GIZMOS
    private void OnDrawGizmos() {

        if (_data == null) {
            _data = RoomGenerator.Instance.GetData;
            return;
        }
        if (_data.sides == null)
            return;

        if (_data.lines == null)
            return;

        if (debugConcave) {
            Gizmos.color = Color.white;
            foreach (var line in _data.lines) {
                Vector3 left = new Vector3((float)line.nodes[0].x, 0f, (float)line.nodes[0].y);
                Vector3 right = new Vector3((float)line.nodes[1].x, 0f, (float)line.nodes[1].y);
                Gizmos.DrawLine(left, right);
            }

            // bounds
            var topLeft = new Vector3(_data.bounds_X.x, 0f, _data.bounds_Y.y); ;
            var topRight = new Vector3(_data.bounds_X.y, 0f, _data.bounds_Y.y);
            var bottomLeft = new Vector3(_data.bounds_X.x, 0f, _data.bounds_Y.x);
            var bottomRight  = new Vector3(_data.bounds_X.y, 0f, _data.bounds_Y.x);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
        }

        if (debugSides) {
            SideGroup.DebugSides(_data.sides);
        }

        if (bridge_Active) {
            DebugBridges();
        }


        if ( displayBalconyRaycasts) {
            for (int lvl = 1; lvl < _data.sides.Length; lvl++) {
                for (int sideIndex = 0; sideIndex < _data.sides[lvl].Count; sideIndex++) {
                    var side = _data.sides[lvl][sideIndex];
                    if (side.balcony == false)
                        continue;
                     side.Blocked(true);
                }
            }
            foreach (var platform in _data.platforms) {
                if (platform.tmpSides == null) continue;
                for (int lvl = 0; lvl < platform.tmpSides.Length; lvl++) {
                    for (int i = 0; i < platform.tmpSides[lvl].Count; i++) {
                        var side = platform.tmpSides[lvl][i];
                        if (!side.balcony) continue;
                        side.Blocked(true);
                    }
                }
            }
        }

        if (debugTowers) {
            for (int i = 0; i < _data.platforms.Count; i++) {
                var platform = _data.platforms[i];
                if (platform._level != currentHexIndex)
                    continue;
                foreach (var bridge in platform.bridges) {
                    for (int j = 0; j < 2; j++) {
                        RaycastHit hit;
                        var o = j == 0 ? bridge.left : bridge.right;
                        if ( Physics.Raycast(o, -Vector2.up, out hit, 100f, RoomGenerator.Instance.layerMask)) {
                            Gizmos.color = Color.green;
                            Gizmos.DrawLine(o, hit.point);
                        } else {
                            Gizmos.color = Color.red;
                            Gizmos.DrawLine(o, o - Vector3.up * 100f);
                        }
                    }

                }
            }
        }

        if (debugPlatformGeneration) {
            for (int lvl = 1; lvl < _data.sides.Length; lvl++) {
                for (int sideIndex = 0; sideIndex < _data.sides[lvl].Count; sideIndex++) {
                    _data.sides[lvl][sideIndex].DebugPlatformGen();
                }
            }
        }

        if (debug_Camera) {
            DebugCamera();
        }



        if (debugAngles) {
            DebugAngles();
        }

        if (debugBridges) {
            // DRAW BRIDGES POINTS
            for (int lvl = 0; lvl < _data.sides.Length; lvl++) {
                for (int i = 0; i < _data.sides[lvl].Count; i++) {
                    Side side = _data.sides[lvl][i];
                    foreach (var bridgeSide in side.bridges) {
                        bridgeSide.Draw();
                    }
                }
            }

        }

        if (platform_active && _data.platforms.Count > 0) {
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

    void DebugBridge(Bridge bridge) {
        Gizmos.color = bridge.built ? Color.magenta : Color.cyan;
        Gizmos.DrawLine(bridge.left, bridge.right);
        Gizmos.DrawSphere(bridge.left, 0.1f);
        Gizmos.DrawSphere(bridge.right, 0.1f);
        if ( bridge.GetTargetBridge() != null) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(bridge.left, bridge.GetTargetBridge().right);
        }
    }
    void DebugBridges() {
        var platformSideBridges = new List<Bridge>();
        foreach (var platform in _data.platforms.FindAll(x=> x.tmpSides != null)) {
            foreach (var row in platform.tmpSides) {
                foreach (var side in row) {
                if (!side.balcony) continue;
                    foreach (var bridge in side.bridges) {
                        DebugBridge(bridge);
                    }
                    //platformSideBridges.AddRange(side.bridges);

                }
            }
        }

        foreach (var platform in _data.platforms) {
            foreach(var bridge in platform.bridges) {
                DebugBridge(bridge);
            }
        }

        foreach (var row in _data.sides) {
            foreach (var side in row) {
                if (!side.balcony) continue;
                foreach (var bridge in side.bridges) {
                    DebugBridge(bridge);
                }
            }
        }
    }

    void DebugAngles() {
        int levelindex = 0;
        for (int i = 0; i < _data.sides[levelindex].Count; i++) {
            Side side = _data.sides[levelindex][i];

            var current_side = _data.sides[levelindex][i];
            int previous_index = i == 0 ? _data.sides[levelindex].Count - 1 : (i - 1);
            int nextIndex = (i + 1) % _data.sides[levelindex].Count;
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


        if (platform_radius) {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(currentPlatform.origin, currentPlatform.radius);
        }

        if (platform_PlatformToPlatform) {
            currentPlatform.PlatformLinks_Debug();
        }

        if (platform_NecessaryLinks) {
            currentPlatform.NecessaryBridges_Debug();
        }

        if (platform_PlatformToBridges) {
            currentPlatform.Links_Debug();
        }


        for (int i = 0; i < currentPlatform.bridges.Count; i++) {
            var bridge = currentPlatform.bridges[i];
            int nextIndex = i == currentPlatform.bridges.Count - 1 ? 0 : i + 1;
            var v1 = currentPlatform.bridges[i].right;
            var v2 = currentPlatform.bridges[nextIndex].left;

            

            if (platform_Bridges) {
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

        if (CameraController.Instance == null || _data == null) return;
        var playerPos = Player.Instance.GetTransform.position;
        if (_data.hexes.Length < 2) {
            return;
        }

        // set hex camera anchors
        for (int i = 0; i < _data.hexes.Length; i++) {
            var hex = _data.hexes[i];
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
        var currentHex = _data.hexes[currentHexIndex];
        for (int i = 0; i < _data.hexes.Length; i++) {
            if (i == currentHexIndex) {
                continue;
            }
            var targetHex = _data.hexes[i];

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
}
