using System.Collections.Generic;
using UnityEngine;

public class RoomCreator {
    public static LayerMask bridge_LayerMask = LayerMask.GetMask("Bridge");
    public static Vector3 bridge_StartDecal = new Vector3(-0.1f, -0.1f, 0.1f);
    public static float bridge_LineDecal = -0.1f;
    public RoomData data;
    public static List<Bridge.Side> bridges = new List<Bridge.Side>();

    public static void NewRoom(RoomData data) {
        // create & set parent
        GameObject parent_obj = new GameObject();
        data.parent = parent_obj.transform;
        data.parent.name = "Room";
        Side entrance = data.Sides[data.entranceLevel][0];
        data.parent.position = entrance.Mid;
        data.parent.forward = entrance.Normal;

        // make cases
        for (int lvl = 0; lvl < data.Sides.Length; lvl++) {
            Side[] sides = data.Sides[lvl];
            for (int i = 0; i < sides.Length; i++) {
                Side side = sides[i];
                Case.NewCases(data, side);

                if (side.balcony) {
                    Case.NewBalcony(data, side);
                }
            }
        }

        /*room.position = Vector3.zero;
        room.forward = Vector3.forward;*/

    }

    public static List<Vector3> GetPlatformPositions(RoomData data) {
        float w = GlobalRoomData.Get.bridgeWidth;

        List<Vector3> positions = new List<Vector3>();

        for (int lvl = 0; lvl < data.Sides.Length; lvl++) {
            for (int i = 0; i < data.Sides[lvl].Length; i++) {
                Side side = data.Sides[lvl][i];
                if (!side.balcony)
                    continue;

                int c = (int)Mathf.Clamp(side.InnerLenght / 3f, 0, 20);

                for (int k = 0; k < c; ++k) {
                    float r = Random.Range(GlobalRoomData.Get.bridgeWidth, side.InnerLenght - GlobalRoomData.Get.bridgeWidth);
                    Vector3 origin = side.GetInner(0) + (side.InnerDir * r);
                    Vector3 v1 = origin - side.Dir * w / 2f;
                    Vector3 v2 = origin + side.Dir * w / 2f;

                    bool superposition = false;
                    if (side.bridgeSides != null) {
                        foreach (var item in side.bridgeSides) {
                            float dis = (item.mid - origin).magnitude;
                            if (dis < GlobalRoomData.Get.bridgeWidth * 2f)
                                superposition = true;
                        }
                    }

                    if (superposition)
                        continue;

                    var newBridgeSide = new Bridge.Side(v1, v2);
                    side.AddBridgeSide(newBridgeSide);

                    // get platform position ( à changer eventuellement )
                    Ray ray = new Ray(newBridgeSide.mid, newBridgeSide.normal);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Wall"))) {
                        Vector3 newPos = ray.origin + (hit.point - ray.origin) / 2f;

                        // check for nearby points and merge them
                        bool foundNearby = false;
                        for (int j = 0; j < positions.Count; j++) {
                            Vector3 currPos = positions[j];
                            Vector3 dir = (currPos - newPos);
                            if (dir.magnitude < GlobalRoomData.Get.platform_maxRadius * 2f) {
                                Vector3 mid = newPos + dir / 2f;
                                //positions[j] = mid;
                                foundNearby = true;
                            }
                        }

                        // add new platform position if found none
                        if (!foundNearby)
                            positions.Add(newPos);
                    }
                }
            }

        }

        return positions;
    }

    public static void CreateBalconyRamps(RoomData data) {
        for (int lvl = 0; lvl < data.Sides.Length; lvl++) {
            for (int i = 0; i < data.Sides[lvl].Length; i++) {
                Side side = data.Sides[lvl][i];

                if (!side.balcony)
                    continue;

                Vector3 p = side.GetInner(0);
                if (side.bridgeSides != null) {
                    side.bridgeSides.RemoveAll(x => !x.used);
                    side.bridgeSides.Sort((Bridge.Side b1, Bridge.Side b2) => Vector3.Distance(side.GetInner(0), b1.left).CompareTo(Vector3.Distance(side.GetInner(0), b2.left)));
                    for (int j = 0; j < side.bridgeSides.Count; ++j) {
                        Case.NewRamp(p, side.bridgeSides[j].left);
                        p = side.bridgeSides[j].right;
                    }
                }

                Case.NewRamp(p, side.GetInner(1));
            }
        }
    }
}
