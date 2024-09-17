using System.Collections.Generic;
using UnityEngine;
using ConcaveHull;
using Unity.VisualScripting;
using UnityEngine.Splines;

[System.Serializable]
public class RoomData {
    public Vector3 origin;
    public Vector3 direction;

    public int seed = 0;
    public int levels_count;
    public float size;
    public Hex[] hexes;
    public int entranceLevel;

    public Vector2 bounds_X;
    public Vector2 bounds_Y;
    public Vector2 bounds_Z;
    public Vector3[] floorPoints;
    public List<Side>[] sides = new List<Side>[0];
    public List<Platform> platforms = new List<Platform>();
    public List<Line> lines = new List<Line>();

    public Side GetSide(int lvl, int index) {
        if ( index < 0)
            return sides[lvl][sides[lvl].Count + index];
        return sides[lvl][index%sides[lvl].Count];
    }

    public void Generate() {
        GlobalRoomData global = GlobalRoomData.Get;

        levels_count = Random.Range(global.levelCount.x, global.levelCount.y);
        entranceLevel = Random.Range(0, levels_count);
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

        // SORT CONCAVE / Mais c'est quoi ça ?
        lines = new List<Line>() { concave[0] };
        concave.RemoveAt(0);
        Line line = lines[0];
        Vector3 left = new Vector3((float)line.nodes[0].x, 0f, (float)line.nodes[0].y);
        Vector3 right = new Vector3((float)line.nodes[1].x, 0f, (float)line.nodes[1].y);
        Vector3 range = right - left;
        Vector3 normal = Vector3.Cross(range, Vector3.up);
        if (Vector3.Dot(left - center, normal) > 0) {
            // what the fuck
            Debug.LogWarning($"bah alors");
            lines[0] = new Line(line.nodes[1], line.nodes[0]);
        }
        //


        // transforming concave into sorted lines ( concave isn't sorted )
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

        var sidePoints = new List<Vector3>();
        for (int i = 0; i < lines.Count; i++) {
            var p = new Vector3((float)lines[i].nodes[0].x, 0f, (float)lines[i].nodes[0].y);
            sidePoints.Add(p);
        }

        sides = SideGroup.CreateSides(sidePoints, levels_count, entranceLevel, true);
        
    }
    public void InitSides_Straight() {
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
        lines = new List<Line>() { concave[0] };
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

        var sideWidth = GlobalRoomData.Get.sideWidth;
        sides = new List<Side>[levels_count];

        // SET EXITS & BALCONY
        for (int lvl = 0; lvl < levels_count; lvl++) {
            float y = lvl * GlobalRoomData.Get.sideHeight;
            var sides_list = new List<Side>();
            int currSideCount = 0;
            for (int i = 0; i < lines.Count; i++) {
                var start = new Vector3((float)lines[i].nodes[0].x, y, (float)lines[i].nodes[0].y);
                var end = new Vector3((float)lines[i].nodes[1].x, y, (float)lines[i].nodes[1].y);
                var dir = (end - start);
                int sideCount = Mathf.Clamp((int)(dir.magnitude / sideWidth), 1, 20);
                var dif = dir.magnitude - (sideWidth * sideCount);
                var w = dif / sideCount;
                // maybe add like a change to have a squigly / sincos side ? like a arc side ? zigzag side? 
                for (int sideIndex = 0; sideIndex < sideCount; sideIndex++) {
                    Side newSide = new Side();
                    newSide.id = currSideCount;
                    newSide.lvl = lvl;
                    var a = start + dir.normalized * (sideWidth + w) * sideIndex;
                    newSide.SetBasePoint(0, a);
                    var b = a + dir.normalized * (sideWidth + w);
                    newSide.SetBasePoint(1, b);
                    newSide.exit = (Random.value < GlobalRoomData.Get.exitChance) || i == 0 && lvl == entranceLevel;
                    sides_list.Add(newSide);
                    ++currSideCount;
                }
            }

            sides[lvl] = sides_list;
        }

        // SET INNER POINTS
        for (int levelIndex = 0; levelIndex < levels_count; levelIndex++) {
            for (int i = 0; i < sides[levelIndex].Count; i++) {
                Side side = sides[levelIndex][i];

                var current_side = sides[levelIndex][i];
                int previous_index = i == 0 ? sides[levelIndex].Count - 1 : (i - 1);
                int nextIndex = (i + 1) % sides[levelIndex].Count;
                var previous_side = sides[levelIndex][previous_index];
                var next_side = sides[levelIndex][nextIndex];

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
