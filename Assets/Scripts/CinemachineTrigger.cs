using Cinemachine;
using UnityEngine;

public class CinemachineTrigger : MonoBehaviour {
    public CinemachineVirtualCamera VirtualCamera;
    public bool triggerOnStart = false;
    public bool loop = false;
    public bool triggered = false;
    public static CinemachineTrigger current;

    private void Awake() {

    }

    private void Start() {
        if (triggerOnStart) {
            Trigger();
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.T)) {
            if (current != this)
                Trigger();
        }
    }
    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Player")
            Trigger();
    }

    void Trigger() {
        if (triggered)
            return;
        if (current != null)
            current.triggered = false;

        triggered = true;
        current = this;
    }
}
