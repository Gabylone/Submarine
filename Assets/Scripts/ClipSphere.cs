using UnityEngine;

[ExecuteInEditMode]
public class ClipSphere : MonoBehaviour {
    public string centerName;

    void Update() {
        Shader.SetGlobalVector(centerName, transform.position);
    }
}