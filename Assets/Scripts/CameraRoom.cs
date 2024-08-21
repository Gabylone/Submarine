using UnityEngine;

public class CameraRoom : MonoBehaviour {
    public static CameraRoom previous;
    public static CameraRoom current;

    public bool triggerAtStart = false;

    public Color color = Color.cyan;

    public bool active = false;
    public Transform target;

    [Header("Target")]
    public bool followPlayer_Active = false;
    public bool lookAtPoint_Active = false;
    public Transform lookAtPoint_Target;

    public float transitionSpeed = 1f;

    [Header("Zoom")]
    public bool zoom_Active = false;
    public float zoom_Distance = 5f;
    public Vector3 zoom_Direction = Vector3.forward;

    [Header("Rotate Around Point")]
    public bool rotateAroundPoint_Active = false;
    public Transform rotateAroundPoint_Center;
    public float rotateAroundPoint_Y = 0f;

    private void Start() {
        if (triggerAtStart) {
            Trigger(true);
        }
    }
    public void Exit() {
        active = false;
    }

    public void Trigger(bool cut) {
        previous = current;

        if (previous != null) {
            previous.Exit();
        }

        CameraBehavior.Instance.GetTransform.parent = target;

        current = this;

        active = true;

        if (cut) {
            CameraBehavior.Instance.GetTransform.position = target.position;
            if (followPlayer_Active) {

            } else {
                CameraBehavior.Instance.GetTransform.rotation = CameraBehavior.Instance.GetRotation();
            }
        } else {
            //CameraBehavior.Instance.GetTransform.DOMove(target.position, transitionSpeed);
        }

        //zoom
        CameraBehavior.Instance.zoom = zoom_Active;
        if (zoom_Active) {
            CameraBehavior.Instance.zoom_Distance = zoom_Distance;
        }

        // rotate around point
        CameraBehavior.Instance.rotateAroundPoint_Active = rotateAroundPoint_Active;

        CameraBehavior.Instance.lookAtPoint_Active = followPlayer_Active || lookAtPoint_Active;

        if (followPlayer_Active || lookAtPoint_Active) {
            if (followPlayer_Active) {
                CameraBehavior.Instance.lookAtPoint_Target = CameraBehavior.Instance.followPlayer_Target;
            } else {
                CameraBehavior.Instance.lookAtPoint_Target = lookAtPoint_Target;
            }
        }

        if (cut) {
            if (followPlayer_Active || lookAtPoint_Active) {
                CameraBehavior.Instance.GetTransform.rotation = CameraBehavior.Instance.GetRotation();
            } else {
                CameraBehavior.Instance.GetTransform.rotation = target.rotation;
            }
        }


    }

    public void TestCamera() {
        Transform cameraTransform = GameObject.FindObjectOfType<CameraBehavior>().transform;

        cameraTransform.position = target.position;
        cameraTransform.rotation = target.rotation;

        cameraTransform.parent = target;
        cameraTransform.localPosition = Vector3.zero;
        cameraTransform.rotation = target.rotation;
    }

    private void OnDrawGizmos() {
        if (target == null) {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 0.5f);
            return;
        }

        Gizmos.color = color;

        foreach (var item in GetComponentsInChildren<CameraGroup>()) {
            Gizmos.DrawLine(item.transform.position, target.position);
        }

        Gizmos.DrawRay(target.position, target.forward * 0.75f);

        if (zoom_Active) {
            Vector3 dir = target.TransformDirection(zoom_Direction);
            Gizmos.DrawRay(target.position, dir * zoom_Distance);
        }

        Gizmos.matrix = target.localToWorldMatrix;

        Gizmos.DrawCube(Vector3.zero, new Vector3(0.5f, 0.3f, 0.2f));

        foreach (var item in GetComponentsInChildren<CameraGroup>()) {
            Gizmos.matrix = item.transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
        }
    }
}
