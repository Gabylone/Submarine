using UnityEngine;

public class RotationRef : MonoBehaviour {
    private static RotationRef _instance;
    public static RotationRef Instance {
        get {
            if (_instance == null) {
                _instance = GameObject.FindObjectOfType<RotationRef>();
            }

            return _instance;
        }
    }

    private Transform _transform;

    public Transform GetTransform {
        get {
            if (_transform == null) {
                _transform = GetComponent<Transform>();
            }

            return _transform;
        }
    }

    public Vector3 GetUpDirection() {
        return GetTransform.up;
    }
}
