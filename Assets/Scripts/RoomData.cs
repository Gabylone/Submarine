using System.Collections.Generic;
using UnityEngine;
using ConcaveHull;
using Mono.Cecil.Cil;

[System.Serializable]
public class RoomData {
    public Vector3 origin;
    public Vector3 direction;

    public int seed = 0;
    public int lvls;
    public float size;
    public Hex[] hexes;
    public int entranceLevel;
    public Side[][] sides;
    public List<Platform> platforms = new List<Platform>();


    public void Generate() {
        GlobalRoomData global = GlobalRoomData.Get;

        lvls = Random.Range(global.levelCount.x, global.levelCount.y);
        entranceLevel = Random.Range(0, lvls);
        size = Random.Range(global.size.x, global.size.y);

        hexes = new Hex[Random.Range(global.hexCount.x, global.hexCount.y)];
        for (int i = 0; i < hexes.Length; i++) {
            hexes[i] = new Hex();
            hexes[i].Randomize();
        }
        InitSides();

    }



    public void InitSides() {
        // CREATE POINT CLOUD
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < hexes.Length; i++)
            points.AddRange(hexes[i].GetPositions());

        var totalX = 0f;
        var totalY = 0f;
        foreach (var p in points) {
            totalX += p.x;
            totalY += p.z;
        }
        var center = new Vector3(totalX / points.Count, 0f, totalY / points.Count);

        // CREATE CONCAVE
        List<Line> concave = Hull.GetConcave(points.ToArray(), GlobalRoomData.Get.hullConcavity, GlobalRoomData.Get.hullScale);

        // SORT CONCABE
        List<Line> lines = new List<Line>() { concave[0] };
        concave.RemoveAt(0);

        Line line = lines[0];
        Vector3 left = new Vector3((float)line.nodes[0].x, 0f, (float)line.nodes[0].y);
        Vector3 right = new Vector3((float)line.nodes[1].x, 0f, (float)line.nodes[1].y);
        Vector3 range = right - left;
        Vector3 normal = Vector3.Cross(range, Vector3.up);
        if (Vector3.Dot(left - center, normal) > 0)
            lines[0] = new Line(line.nodes[1], line.nodes[0]);

        while (concave.Count > 0) {
            for (int i = 0; i < concave.Count; i++) {
                Line concLine = concave[i];
                Line sortedLine = lines[lines.Count - 1];

                if (sortedLine.nodes[1].x == concLine.nodes[0].x
                    &&
                    sortedLine.nodes[1].y == concLine.nodes[0].y) {
                    lines.Add(concLine);
                    concave.RemoveAt(i);
                    break;
                }

                if (sortedLine.nodes[1].x == concLine.nodes[1].x
                    &&
                    sortedLine.nodes[1].y == concLine.nodes[1].y) {
                    Line newLine = new Line(concLine.nodes[1], concLine.nodes[0]);
                    lines.Add(newLine);
                    concave.RemoveAt(i);
                    break;
                }
            }
        }

        sides = new Side[lvls][];

        // SET EXITS & BALCONY
        for (int lvl = 0; lvl < lvls; lvl++) {
            sides[lvl] = new Side[lines.Count];
            for (int i = 0; i < sides[lvl].Length; i++) {
                Side newSide = new Side();
                newSide.id = i;
                newSide.lvl = lvl;
                float y = lvl * GlobalRoomData.Get.caseHeight;

                for (int s = 0; s < 2; s++) {
                    Vector3 p = new Vector3((float)lines[i].nodes[s].x, y, (float)lines[i].nodes[s].y);
                    newSide.SetBasePoint(s, p);
                }

                newSide.exit = (Random.value < GlobalRoomData.Get.exitChance) || i == 0 && lvl == entranceLevel;
                sides[lvl][i] = newSide;
            }
        }

        // SET INNER POINTS
        for (int lvl = 0; lvl < lvls; lvl++) {
            for (int i = 0; i < sides[lvl].Length; i++) {
                Side side = sides[lvl][i];

                int pi = i == 0 ? sides[lvl].Length - 1 : (i - 1);
                int ni = i == sides[lvl].Length - 1 ? 0 : (i + 1);
                Side pSide = sides[lvl][pi];
                Side cSide = sides[lvl][i];
                Side nSide = sides[lvl][ni];

                // prev
                Vector3 cV = -cSide.BaseDirection;
                Vector3 pV = pSide.BaseDirection;
                float pdot = Vector3.Dot(pV, Vector3.Cross(cV, Vector3.up));
                Vector3 v1 = Vector3.Lerp(cV, pV, 0.5f).normalized;
                v1 = pdot > 0 ? -v1 : v1;

                Vector3 iLeft = cSide.GetBasePoint(0) + (v1 * GlobalRoomData.Get.balconyDepth);

                // next
                Vector3 nV = nSide.BaseDirection;
                float ndot = Vector3.Dot(nV, Vector3.Cross(cV, Vector3.up));
                Vector3 v2 = Vector3.Lerp(cV, nV, 0.5f).normalized;
                v2 = ndot > 0 ? -v2 : v2;

                Vector3 iRight = cSide.GetBasePoint(1) + (v2 * GlobalRoomData.Get.balconyDepth);

                side.SetBalconyPoint(0, iLeft);
                side.SetBalconyPoint(1, iRight);

            }
        }

        
    }

    

    public Side GetSide(Vector3 p, int lvl) {
        Side closestSide = sides[lvl][0];
        foreach (Side side in sides[lvl]) {
            float curr = (closestSide.BaseMid - p).magnitude;
            float it = (side.BaseMid - p).magnitude;

            if (it < curr) {
                closestSide = side;
            }
        }

        return closestSide;
    }




}
