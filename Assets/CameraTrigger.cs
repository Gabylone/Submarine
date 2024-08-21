using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    private CameraGroup _cameraGroup;
    private void Start() {
        _cameraGroup = GetComponentInParent<CameraGroup>();
    }

    private void OnTriggerEnter(Collider other) {
        Player player = other.GetComponent<Player>();

        if (player != null ) {
            _cameraGroup.Trigger();
        }
    }
}
