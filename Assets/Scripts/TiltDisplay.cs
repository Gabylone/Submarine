using UnityEngine;
using UnityEngine.UI;

public class TiltDisplay : MonoBehaviour {
    public Transform _transform;

    public Text uiText;

    public float displayAngle = 85f;

    // Update is called once per frame
    void Update() {
        float l = -1 + Submarine.Instance.sideTiltAngle * 2;

        float f = Mathf.InverseLerp(-displayAngle, displayAngle, l);

        _transform.localEulerAngles = Vector3.right * f;

        uiText.text = "" + Mathf.Round(f) + "°";
    }
}
