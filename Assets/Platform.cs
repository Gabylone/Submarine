using System.Collections.Generic;
using UnityEngine;

public class Platform {
    public Vector3 pos;
    public float radius;
    public List<Bridge.Side> bridgeSides = new List<Bridge.Side>();

    float angle;

    float Angle {
        get {
            if (angle == 0) {
                Bridge.Side bs = newSide(pos, Vector3.forward);
                Vector3 aDir1 = bs.left - pos;
                Vector3 aDir2 = bs.mid - pos;
                angle = Vector3.Angle(aDir1, aDir2) * 2f;
            }

            return angle;
        }
    }

    public Platform(Vector3 _pos) {
        pos = _pos;

        Collider[] colliders = Physics.OverlapSphere(pos, GlobalRoomData.Get.platform_maxRadius, LayerMask.GetMask("Wall") | LayerMask.GetMask("Floor"));
        if (colliders.Length > 0) {
            Vector3 closestPoint = colliders[0].ClosestPoint(pos);
            for (int i = 1; i < colliders.Length; i++) {
                var tmp = colliders[i].ClosestPoint(pos);
                var dis1 = Vector3.Distance(tmp, pos);
                var dis2 = Vector3.Distance(closestPoint, pos);
                if (dis1 < dis2)
                    closestPoint = tmp;
            }

            radius = (closestPoint - pos).magnitude - GlobalRoomData.Get.balconyDepth;
        } else {
            radius = Random.Range(GlobalRoomData.Get.platform_minRadius, GlobalRoomData.Get.platform_maxRadius);
        }
    }

    public void linkWithBalconies() {
        float w = GlobalRoomData.Get.bridgeWidth;

        RoomData data = RoomManager.Instance.data;
        List<Bridge.Side> tmpSides = new List<Bridge.Side>();
        for (int lvl = 0; lvl < data.Sides.Length; lvl++) {
            for (int i = 0; i < data.Sides[lvl].Length; i++) {
                Side side = data.Sides[lvl][i];
                if (side.bridgeSides == null)
                    continue;
                foreach (var bSide in side.bridgeSides)
                    tmpSides.AddRange(side.bridgeSides.FindAll(x => !x.used && Vector3.Distance(x.mid, pos) < GlobalRoomData.Get.platform_maxDis));
            }
        }

        if (tmpSides.Count <= 0)
            return;

        int b = 0;
        while (tmpSides.Count > 0) {
            int i = Random.Range(0, tmpSides.Count);
            Bridge.Side bSide = tmpSides[i];

            tmpSides.RemoveAt(i);

            ++b;
            if (b >= GlobalRoomData.Get.platform_maxCount)
                break;

            Bridge.Side nSide = newSide(pos, bSide.mid);
            if (!canFit(nSide))
                continue;
            nSide.end = bSide;
            bridgeSides.Add(nSide);
        }
    }

    public void linkBetweenPlatforms(List<Platform> platforms, int c) {
        float w = GlobalRoomData.Get.bridgeWidth;

        for (int i = 0; i < platforms.Count; i++) {
            // skip current plt
            if (i == c) continue;

            Platform p = platforms[i];

            // pos outside radius circle in same Y
            Vector3 dir = p.pos - pos;
            dir.y = 0f;
            Ray ray = new Ray(pos, dir);

            // check if path is free (consider two rays for left & right, with buffer)
            if (Physics.Raycast(ray, dir.magnitude, LayerMask.GetMask("Wall")))
                continue;

            // place side towards other plt
            Bridge.Side start = newSide(pos, p.pos);

            // check if other plt already has side

            Bridge.Side end = newSide(p.pos, pos);

            Bridge.Side dup = p.bridgeSides.Find(
                x =>
                x.left == end.left && x.right == end.right);
            if (dup == null) {
                // if it doesn't, create a link ( which will result in a bridge )
                start.end = end;
                start.link = true;

                if (!canFit(start))
                    continue;
                if (!p.canFit(end))
                    continue;
                if (Vector3.Dot(start.normal, start.dir) < 0)
                    continue;
                bridgeSides.Add(start);
                p.bridgeSides.Add(end);
            }
            // else, add side anyway for platform creation 
        }

    }

    public Bridge.Side newSide(Vector3 origin, Vector3 target) {
        Vector3 dir = target - origin;
        dir.y = 0f;

        float w = GlobalRoomData.Get.bridgeWidth;
        Vector3 normal = Vector3.Cross(dir, Vector3.up).normalized;
        Vector3 decal = (dir.normalized * radius);
        Vector3 left = origin + decal + (normal * w / 2f);
        Vector3 right = origin + decal - (normal * w / 2f);
        return new Bridge.Side(left, right);
    }

    public bool canFit(Bridge.Side side) {
        for (int i = 0; i < bridgeSides.Count; i++) {
            Bridge.Side bs = bridgeSides[i];

            Vector3 dir1 = side.mid - pos;
            Vector3 dir2 = bs.mid - pos;

            if (Vector3.Angle(dir1, dir2) < Angle)
                return false;
        }

        return true;
    }

    public void removeDoubles() {
        for (int i = 0; i < bridgeSides.Count; i++) {
            Bridge.Side bs1 = bridgeSides[i];

            for (int j = i + 1; j < bridgeSides.Count; j++) {
                Bridge.Side bs2 = bridgeSides[j];

                Vector3 dir1 = bs1.mid - pos;
                Vector3 dir2 = bs2.mid - pos;

                if (Vector3.Angle(dir1, dir2) < Angle) {
                    if (bs1.link && bs2.link) {
                        if (bs1.end == null || bs2.end == null)
                            continue;
                        float d1 = Vector3.Distance(bs1.mid, bs1.end.mid);
                        float d2 = Vector3.Distance(bs1.mid, bs2.end.mid);
                        if (d1 < d2)
                            bs2.valid = false;
                        else
                            bs1.valid = false;
                    } else {
                        if (bs2.link)
                            bs1.valid = false;
                        else
                            bs2.valid = false;
                    }
                }

            }
        }

        for (int i = bridgeSides.Count - 1; i >= 0; i--) {
            if (!bridgeSides[i].valid)
                bridgeSides.RemoveAt(i);
        }
    }

    public void buildBridges() {
        if (bridgeSides.Count == 0)
            return;

        for (int i = 0; i < bridgeSides.Count; i++) {
            if (bridgeSides[i].used)
                continue;
            bridgeSides[i].Build();
        }
    }

    public void initPlatformMesh() {
        if (bridgeSides.Count == 0)
            return;

        Bridge.Side bs = bridgeSides.Find(x => !x.blocked);
        if (bs == null)
            return;

        ClockwiseBridgeSidesComparer comp = new ClockwiseBridgeSidesComparer();
        comp.origin = pos;
        comp.dir = bridgeSides[0].left - pos;
        comp.n = Vector3.Cross(comp.dir, Vector3.up);
        bridgeSides.Sort((Bridge.Side b1, Bridge.Side b2) => comp.Compare(b1, b2));

        List<Vector3> pts = new List<Vector3>();
        foreach (var bSide in bridgeSides) {
            pts.Add(bSide.left);
            pts.Add(bSide.right);
        }

        if (bridgeSides.Count == 1) {
            Bridge.Side s = bridgeSides[0];
            Vector3 nLeft = pos - (s.left - pos);
            Vector3 nRight = pos - (s.right - pos);
            pts.Add(nLeft);
            pts.Add(nRight);
            Case.NewRamp(nLeft, nRight);
        }

        int o = 1;
        List<Vector3[]> squares = new List<Vector3[]>();
        while (o < pts.Count - 1) {
            Vector3[] square = new Vector3[4];
            square[0] = pts[0];

            for (int j = 0; j < 3; j++)
                square[j + 1] = pts[o + j];
            squares.Add(square);
            o += 2;
        }

        float balconyHeight = GlobalRoomData.Get.balconyHeight;
        foreach (var item in squares) {
            Transform platform_Transform = PoolManager.Instance.RequestObject("bridge");
            MeshFilter meshFilter = platform_Transform.GetComponentInChildren<MeshFilter>();

            Vector3[] vertices = new Vector3[8]
            {

                item[0],
                item[1],
                item[0] - Vector3.up * balconyHeight,
                item[1]- Vector3.up * balconyHeight,
                item[3] - Vector3.up * balconyHeight,
                item[2] - Vector3.up * balconyHeight,
                item[3],
                item[2],

            };

            MeshControl.Update(meshFilter, vertices);
            platform_Transform.GetComponentInChildren<MeshCollider>().sharedMesh = meshFilter.mesh;
        }

    }

    public void buildRamps() {
        foreach (var item in bridgeSides) {
            if (!item.used) {
                Case.NewRamp(item.left, item.right);
            }
        }

        if (bridgeSides.Count < 2)
            return;
        Vector3 o = bridgeSides[0].right;

        for (int i = 1; i < bridgeSides.Count; i++) {
            Case.NewRamp(o, bridgeSides[i].left);
            o = bridgeSides[i].right;
        }

        Case.NewRamp(o, bridgeSides[0].left);

    }
}
