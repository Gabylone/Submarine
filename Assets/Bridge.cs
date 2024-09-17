using System.Collections.Generic;
using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class Bridge {

    public Bridge(Vector3 left, Vector3 right) {
        this.left = left;
        this.right = right;
    }

    public enum Type {
        Normal,
        Stairs,
        Ladder,
    }

    public static Transform Parent;

    public Type type;
    private Bridge targetBridge;
    public int side;
    public Platform linkedPlatform;

    public void SetTargetBridge(Bridge b) {
        if (b == null) {
            targetBridge = null;
            return;
        }
        if (targetBridge != null) {
            Debug.Log("double setting the bridge");
            if ( targetBridge == b) {
                Debug.Log($"with the same bridge");
            }
        }
        targetBridge = b;
    }
    public Bridge GetTargetBridge() {
        return targetBridge;
    }

    public enum State {
        Pending,
        Built,
        Finished
    }

    public bool built = false;
    public Transform bridgeParent;
    public bool link = false;

    public Vector3 left;
    public Vector3 right;

    public static bool LinkBridges(Bridge b1, Bridge b2) {
        // check if path is blocked
        if (b1.Blocked(b2))
            return false;

        if ( b1.GetTargetBridge() != null || b2.GetTargetBridge() != null) {
            Debug.Log("WARNING : Bridge already connected");
            return false;
        }

        b1.SetTargetBridge(b2);
        b2.SetTargetBridge(b1);
        return true;
    }

    public void Clean() {
        GetTargetBridge().SetTargetBridge(null);
        GetTargetBridge().built = false;
        SetTargetBridge(null);
        bridgeParent.gameObject.SetActive(false);
        bridgeParent = null;
        built = false;
    }

    public bool Blocked(Bridge targetBridge, bool debug = false) {
        float sideBuffer = GlobalRoomData.Get.bridgeSideBuffer;
        float lenghtBuffer = GlobalRoomData.Get.bridgeLenghtBuffer;
        float upDecal = GlobalRoomData.Get.bridgeUpDecal;

        // check if bridge direction doesn't point behind bridge 
        var dirToBridge = targetBridge.mid - mid;
        float dot = Vector3.Dot(dirToBridge.normalized, normal);
        if (dot < 0.2f)
            return true;

        //
        dot = Vector3.Dot(normal, targetBridge.normal);
        if (dot > 0)
            return true;

        Vector3[] start = new Vector3[4] {
                left - GetSideDir * sideBuffer,
                right + GetSideDir * sideBuffer,
                left - GetSideDir * sideBuffer + GetSideDir * upDecal,
                right + GetSideDir * sideBuffer - GetSideDir * upDecal,
            };

        Vector3[] end = new Vector3[4] {
                targetBridge.right + targetBridge.GetSideDir * sideBuffer,
                targetBridge.left - targetBridge.GetSideDir * sideBuffer,
                targetBridge.right + targetBridge.GetSideDir * sideBuffer+ GetSideDir * upDecal,
                targetBridge.left - targetBridge.GetSideDir * sideBuffer - GetSideDir * upDecal,
            };

        bool block = false;
        for (int i = 0; i < 4; i++) {
            var dir = (end[i] - start[i]).normalized;

            var a = start[i] + dir * lenghtBuffer - Vector3.up * GlobalRoomData.Get.balconyHeight / 2F;
            var b = end[i] - dir * lenghtBuffer - Vector3.up * GlobalRoomData.Get.balconyHeight / 2F;

            if (i > 1) {
                var leftMid = left + (targetBridge.right - left) / 2F;
                var rightMid = right + (targetBridge.left - right) / 2F;
                var normalUp = Vector3.Cross(dir, (rightMid - leftMid).normalized);
                a += normalUp * GlobalRoomData.Get.bridgeHeight;
                b += normalUp * GlobalRoomData.Get.bridgeHeight;
            }

            RaycastHit hit;
            bool cast = Physics.Linecast(a, b, out hit, RoomGenerator.Instance.layerMask);
            if (cast) {
                block = true;
            }
            if (debug) {
                if (cast) {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(a, hit.point);
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(hit.point, b);
                } else {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(a, b);
                }
            }
        }
        return block;
    }

    public void DecalPositions() {
        var fDot = Vector3.Dot(GetDir, normal);
        float maxDecal = 3f;
        // > 0 : going right / < 0 goin left
        var goingRight = Vector3.Dot(GetDir, GetSideDir) > 0;
        if (goingRight) {
            left += normal * fDot * maxDecal;
        } else {
            right += normal * fDot * maxDecal;
        }
    }

    public void Build(bool flat = false) {
        if (targetBridge == null) {
            Debug.LogError($"On Build Bridge: No target bridge");
            return;
        }

        if (Parent == null) {
            Parent = new GameObject().transform;
            Parent.name = "Bridges";
            Parent.SetParent(PoolManager.Instance.currentRoom.parent);
        }

        bridgeParent = new GameObject().transform;
        bridgeParent.SetParent(Parent);
        bridgeParent.name = "Bridge";

        // set built 
        built = true;
        targetBridge.built = true;

        if ( targetBridge.linkedPlatform == null ) {
            if (linkedPlatform == null) {
                DecalPositions();
                targetBridge.DecalPositions();

            } else {
                // platform to side
                if (Vector3.Dot(targetBridge.mid, -GetDir.normalized) > 0) {
                    targetBridge.left = right + (targetBridge.right - left);
                } else {
                    targetBridge.right = left + (targetBridge.left - right);
                }
            }
        }


        float angle = Vector3.Angle(normal, GetDir);
        type = angle > GlobalRoomData.Get.angleToLadder ? Type.Ladder : angle > GlobalRoomData.Get.angleToStairs ? Type.Stairs : Type.Normal;
        if (flat) type = Type.Normal;

        // check ladder angle
        switch (type) {
            case Type.Normal:
                BuildFloor();
                Case.NewRamp(left, targetBridge.right, bridgeParent);
                Case.NewRamp(right, targetBridge.left, bridgeParent);
                break;
            case Type.Stairs:
                BuildFloor();
                BuildStairs();
                Case.NewRamp(left, targetBridge.right, bridgeParent);
                Case.NewRamp(right, targetBridge.left, bridgeParent);
                break;
            case Type.Ladder:
                BuildLadder();
                break;
            default:
                break;
        }

        switch (type) {
            case Type.Normal:
            case Type.Stairs:
                
                
                break;
            default:
                break;
        }
    }

    public void BuildFloor() {
        Transform bridge_Tr = PoolManager.Instance.NewObject("floor", "bridge", bridgeParent);
        float balconyHeight = GlobalRoomData.Get.balconyHeight;

        // mesh
        MeshFilter meshFilter = bridge_Tr.GetComponentInChildren<MeshFilter>();
        Vector3[] vertices = new Vector3[8]
        {
                left - Vector3.up * balconyHeight,
                right- Vector3.up * balconyHeight,
                left,
                right,
                targetBridge.right,
                targetBridge.left,
                targetBridge.right - Vector3.up * balconyHeight,
                targetBridge.left - Vector3.up * balconyHeight,
        };
        MeshControl.Update(meshFilter, vertices);
        bridge_Tr.GetComponentInChildren<MeshCollider>().sharedMesh = meshFilter.mesh;
        if (type == Type.Stairs)
            bridge_Tr.GetComponentInChildren<MeshRenderer>().enabled = false;

    }
    public void BuildLadder() {
        var ladder = PoolManager.Instance.NewObject("ladder", "Ladders", bridgeParent).GetComponent<Ladder>();
        var ladderWidth = GlobalRoomData.Get.ladderWidth;

        type = Type.Stairs;

        var positions = new List<Vector3>() {
                    targetBridge.mid - targetBridge.GetSideDir * ladderWidth,
                    targetBridge.mid + targetBridge.GetSideDir * ladderWidth,
                    mid + GetSideDir * ladderWidth,
                    mid - GetSideDir * ladderWidth
                };
        if (targetBridge.right.y > right.y) {
            positions.Reverse();
            positions[2] += GetDir;
            positions[3] += GetDir;
        } else {
            positions[2] -= GetDir;
            positions[3] -= GetDir;
        }

        ladder.Init(positions.ToArray());
    }

    public void BuildStairs() {
        float stairWidth = GlobalRoomData.Get.stairWidth;
        float dis = (targetBridge.mid - mid).magnitude;
        int stairCount = (int)(dis / stairWidth);
        for (int i = 0; i < (stairCount + 1); i++) {
            Vector3 stair_Left = Vector3.Lerp(left, targetBridge.right, (float)i / stairCount);
            Vector3 stair_Right = Vector3.Lerp(right, targetBridge.left, (float)i / stairCount);
            Transform stairStep = PoolManager.Instance.NewObject("stair step", "steps", bridgeParent);
            stairStep.position = stair_Left + (stair_Right - stair_Left) / 2f;
            stairStep.right = (stair_Left - stair_Right).normalized;
            stairStep.localScale = new Vector3((stair_Left - stair_Right).magnitude, stairWidth, stairWidth);
        }
    }


    public void Draw() {
        Gizmos.color = GetTargetBridge() != null ? Color.magenta : Color.white;
        Gizmos.DrawLine(left, right);
        if (GetTargetBridge() != null) {
            Gizmos.DrawLine(left, targetBridge.right);
            Gizmos.DrawLine(right, targetBridge.left);
        }
    }

    public Vector3 normal {
        get { return Vector3.Cross(GetSideDir, Vector3.up); }
    }


    public Vector3 mid {
        get {
            return left + (right - left) / 2f;
        }
    }

    public Vector3 GetDir {
        get {
            return (targetBridge.mid - mid).normalized;
        }
    }

    public Vector3 ldir {
        get {
            return targetBridge.mid - mid;
        }
    }

    public Vector3 GetSideDir {
        get {
            return (right - left).normalized;
        }
    }

    public Vector3 lsdir {
        get {
            return right - left;
        }
    }
}