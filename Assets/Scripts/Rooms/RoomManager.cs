using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

/// <summary>
/// script for room EDITOR script generation ( includes coroutines, debug functions & data etc... )
/// </summary>
public class RoomManager : MonoBehaviour {
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

    public Transform debug_parent;
    public delegate void OnWait();
    public OnWait onWait;

    public int debug_currentPlatformIndex = 0;

    public bool debugBalconies = false;

    public bool debugPlatforms = false;
    public bool debugPlatformLinks = false;
    public bool debugPlatformRadius = false;
    public bool debugPlatformSides = false;
    public bool debugPlatformBridges = false;
    public bool debugBridges = false;
    public bool debugHexes = false;

    /// <summary>
    /// DATA
    /// </summary>
    public RoomData debug_data;
    private void Start() {
        Invoke("GenerateRoom", 0f);
    }

    void GenerateRoom() {
        Random.InitState(debug_data.seed);
        RoomGenerator.NewRoom(debug_data);
    }


    private void Update() {
        if (Input.GetKeyDown(KeyCode.N)) {
            canSkip = true;

        }
    }

    public void Wait() {
        Invoke("Delay", 0f);
    }
    void Delay() {
        if (onWait != null) {
            onWait();
        }
    }

    public void SetCameras() {

        // CAMERAS
        var positions = new List<Vector3>();
        float waypointRate = 1f;
        for (int i = 0; i < debug_data.Sides[0].Length; i++) {
            var side = debug_data.Sides[0][i];
            var l = side.BaseWidth * waypointRate;
            var pos = side.BaseMid + side.Normal * 0.25f;
            positions.Add(pos);
            /*for (int j = 0; j < l; j++)
                positions.Add(side.GetBasePoint(0) + side.BaseDirection / waypointRate * i);*/
        }
        CameraController.Instance.NewCameraGroup(positions);
    }


    public Bridge.Side debug_CurrentBridgeSide; 
    bool canSkip = false;

    public void BuildPlatform(int _currentPlatformIndex) {
        StartCoroutine(BuildPlatformsCoroutine(_currentPlatformIndex));
    }

    IEnumerator BuildPlatformsCoroutine(int _currentPlatformIndex) {

        var _handledData = debug_data;
        var currentPlatform = _handledData.platforms[_currentPlatformIndex];

        currentPlatform.CheckLinksWithOtherPlatforms(_handledData.platforms, _currentPlatformIndex);
        yield return new WaitForEndOfFrame();
        currentPlatform.CreatLinksWithFloor();
        currentPlatform.CreateLinks_Balconies_Floor();
        currentPlatform.RemoveDuplicateBridges();
        yield return new WaitForEndOfFrame();
        currentPlatform.HandleLonely();
        currentPlatform.DrawMesh();
        yield return new WaitForEndOfFrame();
        currentPlatform.BuildBridges();
        yield return new WaitForEndOfFrame();

        /*while (!canSkip)
            yield return null;
        canSkip = false;*/

        int nextPlatformIndex = _currentPlatformIndex + 1;
        if ( nextPlatformIndex == _handledData.platforms.Count) {
            //RoomGenerator.BuildRamps();S
            Debug.Log($"Finished all platforms");

            // check for finished bridges
            foreach (var platform in debug_data.platforms) {
                foreach (var bs in platform.bridgeSides) {
                    if ( bs.end != null ) {
                        if (bs.Blocked(bs.end)) {
                            bs.end = null;
                            bs.used = false;
                            bs.parent.gameObject.SetActive(false);
                            yield return new WaitForEndOfFrame();
                            Debug.Log($"Blocked on second pass");
                            /*if ( bs.parent != null) {
                                foreach (var item in bs.parent.GetComponentsInChildren<MeshRenderer>()) {
                                    item.material.color = Color.red;
                                }
                            }*/
                        }}
                }
            }

            // set Cameras
            


        } else {
            BuildPlatform(nextPlatformIndex);
        }


        // set camera
    }

    // GIZMOS
    private void OnDrawGizmos() {

        Transform parent = PoolManager.Instance.currentParent == null ? debug_parent : PoolManager.Instance.currentParent;
        Side entrance = debug_data.Sides[debug_data.entranceLevel][0];
        parent.position = entrance.BaseMid;
        parent.forward = entrance.Normal;

        if (debug_data != null && debug_data.Sides != null) {
            Hex[] hexes = debug_data.hexes;
            List<Vector3> points = new List<Vector3>();
            for (int i = 0; i < hexes.Length; i++)
                points.AddRange(hexes[i].GetPositions());

            Gizmos.color = Color.grey;

            // DRAW HEX POINTS
            if (debugHexes) {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < debug_data.hexes.Length; i++) {
                    Hex hex = debug_data.hexes[i];
                    Vector3[] ps = hex.GetPositions();
                    for (int j = 0; j < ps.Length; ++j) {
                        Gizmos.DrawSphere(ps[j], 0.1f);
                        Gizmos.DrawLine(ps[j], ps[j + 1 == ps.Length ? 0 : j + 1]);
                    }
                }
            }


            if (debugBalconies) {
                // DRAW BRIDGES POINTS
                for (int lvl = 1; lvl < debug_data.Sides.Length; lvl++) {
                    for (int i = 0; i < debug_data.Sides[lvl].Length; i++) {
                        Side side = debug_data.Sides[lvl][i];
                        if (!side.balcony) continue;

                        var a = side.GetBalconyPoint(0) + side.BaseBalconyDir * 0.1f;
                        var b = side.GetBalconyPoint(1) + side.BaseBalconyDir * 0.1f;
                        Gizmos.color =  Physics.Linecast(a, b) ? Color.red : Color.green;
                        Gizmos.DrawLine(a, b);
                    }
                }
            }

            if (debugBridges) {
                // DRAW BRIDGES POINTS
                for (int lvl = 0; lvl < debug_data.Sides.Length; lvl++) {
                    for (int i = 0; i < debug_data.Sides[lvl].Length; i++) {
                        Side side = debug_data.Sides[lvl][i];
                        foreach (var bridgeSide in side.bridgeSides) {
                            bridgeSide.Draw();
                        }
                    }
                }
            }


            if (debug_data.platforms.Count == 0)
                return;

            if (debugPlatforms) {
                if (debug_currentPlatformIndex < 0) {
                    int index = 0;
                    foreach (var platform in debug_data.platforms) {
                        DebugPlatform(index, platform);
                        ++index;
                    }
                } else {
                    debug_currentPlatformIndex = debug_currentPlatformIndex % debug_data.platforms.Count;
                    DebugPlatform(debug_currentPlatformIndex, debug_data.platforms[debug_currentPlatformIndex]);
                }
            }
            
        }
    }

    void DebugPlatform(int index, Platform currentPlatform) {
        Gizmos.color = Color.yellow;
       
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


        for (int i = 0; i < currentPlatform.bridgeSides.Count; i++) {
            var brideSide = currentPlatform.bridgeSides[i];
            int nextIndex = i == currentPlatform.bridgeSides.Count - 1 ? 0 : i + 1;
            var v1 = currentPlatform.bridgeSides[i].right;
            var v2 = currentPlatform.bridgeSides[nextIndex].left;

            if (debugPlatformSides) {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(v1, v2);
                Gizmos.color = Color.white;
                Gizmos.DrawLine(currentPlatform.bridgeSides[i].left, currentPlatform.bridgeSides[i].right);
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
