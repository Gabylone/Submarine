using System;
using UnityEngine;

public class BodyModifier : MonoBehaviour {
    public SkinnedMeshRenderer tmpMesh;

    public Mesh base_mesh;

    [Serializable]
    public class MeshGroup {
        [Range(0, 1)]
        public float lerp = 0f;
        public Mesh mesh;
        public Vector3[] difs;
    }

    [Serializable]
    public class BoneGroup {
        public Transform target;
        public bool decalModel = false;
    }
    [Range(0, 1)]
    public float boneMult;

    public Transform overall;

    public BoneGroup[] boneGroups;
    public MeshGroup[] meshGroups;
    Vector3[] vertices;


    private void Start() {
        foreach (var group in meshGroups) {
            group.difs = new Vector3[base_mesh.vertices.Length];
            for (int i = 0; i < group.difs.Length; i++) {
                group.difs[i] = group.mesh.vertices[i] - base_mesh.vertices[i];
            }
        }
    }

    private void LateUpdate() {
        UpdateMesh();
        UpdateBones();
    }

    void UpdateBones() {
        foreach (var boneGroup in boneGroups) {
            boneGroup.target.Translate(Vector3.up * boneMult);
            if (boneGroup.decalModel) {
                overall.Translate(Vector3.up * boneMult, Space.World);
            }
        }
    }

    void UpdateMesh() {
        Vector3[] vertices = new Vector3[base_mesh.vertices.Length];
        for (int i = 0; i < vertices.Length; i++) {
            vertices[i] = base_mesh.vertices[i];
        }

        Mesh targetMesh = tmpMesh.sharedMesh;

        foreach (var meshGroup in meshGroups) {
            for (int i = 0; i < targetMesh.vertices.Length; i++) {
                vertices[i] += meshGroup.difs[i] * meshGroup.lerp;
            }
        }

        targetMesh.SetVertices(vertices);
        targetMesh.RecalculateBounds();
        targetMesh.RecalculateNormals();
        targetMesh.RecalculateTangents();
        targetMesh.UploadMeshData(false);
    }
}
