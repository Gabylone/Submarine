using TMPro;
using UnityEngine;

public static class MeshControl {
    // r d f 0 13 23
    // l d f 1 14 16
    // r t f 2 8 22
    // l t f 3 9 17

    // r t b 4 10 21
    // l t b 5 11 18
    // r d b 6 12 20
    // l d b 7 15 19

    private static int[][] verts;
    public static int[] GetVerts(int i) {
        if (verts == null) {
            verts = new int[8][];

            verts[0] = new int[3] { 0, 13, 23 };
            verts[1] = new int[3] { 1, 14, 16 };
            verts[2] = new int[3] { 2, 8, 22 };
            verts[3] = new int[3] { 3, 9, 17 };
            verts[4] = new int[3] { 4, 10, 21 };
            verts[5] = new int[3] { 5, 11, 18 };
            verts[6] = new int[3] { 6, 12, 20 };
            verts[7] = new int[3] { 7, 15, 19 };
        }

        return verts[i];
    }

    public enum Vertices {
        Right_Bottom_Front,
        Left_Bottom_Front,
        Right_Top_Front,
        Left_Top_Front,

        Right_Top_Back,
        Left_Top_Back,
        Right_Bottom_Back,
        Left_Bottom_Back,
    }

    static bool prout = false;
    public static void Update(MeshFilter meshFilter, Vector3[] vertices) {
        Mesh mesh = meshFilter.mesh;

        Vector3[] tmp_Vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++) {
            for (int v = 0; v < GetVerts(i).Length; v++) {
                tmp_Vertices[GetVerts(i)[v]] = vertices[i];
            }
        }

        prout = true;

        mesh.vertices = tmp_Vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        meshFilter.GetComponentInChildren<MeshCollider>().sharedMesh = meshFilter.mesh;
    }
}
