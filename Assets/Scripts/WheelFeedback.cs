using UnityEngine;

public class WheelFeedback : MonoBehaviour {
    Wheel wheel;

    public Transform localAnchor;

    public bool global = true;
    public Submarine.Value value;
    public Transform globalAnchor;

    public Vector3 initAngles = Vector3.forward * -90f;
    public float maxAngle = 90f;

    public float lerpSpeed = 1f;

    private void Start() {
        wheel = GetComponentInParent<Wheel>();

        if (!global) {
            globalAnchor.gameObject.SetActive(false);
        }

        QuickUpdate();
    }

    private void Update() {
        float localValue = wheel.GetValue() / wheel.valueMultiplier;
        Vector3 localAngles = -Vector3.forward * localValue * maxAngle;

        Quaternion localRot = Quaternion.Euler(initAngles + localAngles);
        localAnchor.localRotation = Quaternion.Lerp(localAnchor.localRotation, localRot, lerpSpeed * Time.deltaTime);

        if (global) {
            float globalValue = Submarine.Instance.GetLerp(value);
            Vector3 globalAngles = -Vector3.forward * globalValue * maxAngle;

            Quaternion globalRot = Quaternion.Euler(initAngles + globalAngles);
            globalAnchor.localRotation = Quaternion.Lerp(globalAnchor.localRotation, globalRot, lerpSpeed * Time.deltaTime);
        }
    }

    void QuickUpdate() {
        float localValue = wheel.GetValue() / wheel.valueMultiplier;
        Vector3 localAngles = -Vector3.forward * localValue * maxAngle;
        localAnchor.localEulerAngles = initAngles + localAngles;

        if (global) {
            float globalValue = Submarine.Instance.GetLerp(value);
            Vector3 globalAngles = -Vector3.forward * globalValue * maxAngle;
            globalAnchor.localEulerAngles = initAngles + globalAngles;
        }
    }
}
