using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomManager : MonoBehaviour {
    private static RoomManager _inst;
    public static RoomManager Instance {
        get {
            if (_inst == null) {
                _inst = GameObject.FindObjectOfType<RoomManager>();
            }

            return _inst;
        }
    }

    public List<ConcaveHull.Line> rest = new List<ConcaveHull.Line>();
    public ConcaveHull.Line lastLine;
    public float intersectionDecal = 0f;

    public Transform anchor;
    public Platform[] platforms;
    public RoomData data;
    public int bridgeID;

    public List<Vector3> poses = new List<Vector3>();
    private void Start() {
        StartCoroutine(NewRoomCoroutine());
    }

    IEnumerator NewRoomCoroutine() {
        RoomCreator.NewRoom(data);

        yield return new WaitForEndOfFrame();
        yield return BuildBridgesCoroutine();
    }

    IEnumerator BuildBridgesCoroutine() {
        // get potential platform positsion
        List<Vector3> platformPositions = RoomCreator.GetPlatformPositions(data);
        yield return new WaitForEndOfFrame();

        for (int i = 0; i < platformPositions.Count; i++) {
            // skip platforms 
            Platform p = new Platform(platformPositions[i]);
            if (p.radius < GlobalRoomData.Get.platform_minRadius)
                continue;
            data.platforms.Add(p);
            poses.Add(platformPositions[i]);
        }
        // first make links and build briges for priorities
        for (int i = 0; i < data.platforms.Count; i++)
            data.platforms[i].linkBetweenPlatforms(data.platforms, i);
        // set link with balconies
        foreach (var item in data.platforms)
            item.linkWithBalconies();
        // removing double
        foreach (var item in data.platforms)
            item.removeDoubles();
        foreach (var item in data.platforms)
            item.initPlatformMesh();
        // build bridges with pause
        foreach (var item in data.platforms) {
            item.buildBridges();
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForEndOfFrame();

        foreach (var item in data.platforms)
            item.buildRamps();

        yield return new WaitForEndOfFrame();

        RoomCreator.CreateBalconyRamps(data);

        UnityEngine.Debug.Log("finished bridge generation");
    }

    // GIZMOS
    private void OnDrawGizmos() {
        Side entrance = data.Sides[data.entranceLevel][0];
        anchor.position = entrance.Mid;
        anchor.forward = entrance.Normal;

        if (data != null && data.Sides != null) {
            Hex[] hexes = data.hexes;
            List<Vector3> points = new List<Vector3>();
            for (int i = 0; i < hexes.Length; i++)
                points.AddRange(hexes[i].GetPositions());

            Gizmos.color = Color.grey;

            // DRAW CONCAVE
            List<ConcaveHull.Line> concave = ConcaveHull.Hull.GetConcave(points.ToArray(), GlobalRoomData.Get.hullConcavity, GlobalRoomData.Get.hullScale);
            foreach (var item in concave)
                Gizmos.DrawLine(new Vector3((float)item.nodes[0].x, -1f, (float)item.nodes[0].y), new Vector3((float)item.nodes[1].x, -1f, (float)item.nodes[1].y));

            // DRAW HEX POINTS
            Gizmos.color = Color.cyan;
            for (int i = 0; i < data.hexes.Length; i++) {
                Hex hex = data.hexes[i];
                Vector3[] ps = hex.GetPositions();
                for (int j = 0; j < ps.Length; ++j)
                    Gizmos.DrawSphere(ps[j], 0.1f);
            }

            // DRAW BRIDGES POINTS
            for (int lvl = 0; lvl < data.Sides.Length; lvl++) {
                for (int i = 0; i < data.Sides[lvl].Length; i++) {
                    Side side = data.Sides[lvl][i];
                    if (side.bridgeSides == null)
                        continue;

                    foreach (var part in side.bridgeSides)
                        part.Draw();
                }
            }

            foreach (var item in poses) {
                Gizmos.DrawWireSphere(item, GlobalRoomData.Get.platform_maxRadius);
            }

            return;
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
