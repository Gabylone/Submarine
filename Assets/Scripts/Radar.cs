using UnityEngine;

using DG.Tweening;

public class Radar : Interactable {
    public static Radar Instance;

    public CameraRoom targetCameraRoom_Upstairs;
    public CameraRoom targetCameraRoom_Downstairs;

    [Header("Mecanism")]
    public Wheel turnWheel;
    public Lever verticalMoveLever;
    public Lever lateralMoveLever;
    public Pully reset_pully;

    [Header("Zoom")]
    public Lever zoomLever;
    public float zoom_Min;
    public float zoom_Max;

    [Header("Reset")]
    public float reset_duration;
    Vector3 initLocalPos;
    float reset_Timer;
    bool reseting;
    Vector3 reset_lerpPos;

    [Header("Speed")]
    public float turnSpeed = 50f;
    public float moveSpeed = 10f;

    [Header("Transforms")]
    public Transform base_transform;
    public Transform target_Transform;
    public Transform cave_Radar;
    public Transform submarine_World;
    public Transform submarine_Radar;
    public Transform radarOverall;
    public Transform initTransform;

    public Transform upstairs_Anchor;
    public Transform downstairs_Anchor;

    public Transform objective_Target;
    public Transform objective_Pointer;
    public float objective_Distance = 1f;

    public float mult;

    private bool upstairs = true;

    Vector3 initPos;

    private void Awake() {
        Instance = this;
    }

    public override void Start() {
        base.Start();

        initLocalPos = target_Transform.localPosition;

        reset_pully.onTrigger += HandleOnTriggerPully;

        initPos = radarOverall.localPosition;

        //ToUpstairs();
    }

    public override void Update() {
        base.Update();

        target_Transform.SetParent(initTransform);

        // minimap calculations
        float mod = cave_Radar.localScale.x;

        Vector3 v = submarine_World.localPosition * mod;
        submarine_Radar.localPosition = Vector3.zero;

        submarine_Radar.localRotation = submarine_World.localRotation;

        cave_Radar.localPosition = -v;

        target_Transform.localPosition = Vector3.zero;

        target_Transform.Translate(Vector3.right * lateralMoveLever.GetValue() * moveSpeed, Space.World);
        target_Transform.Translate(Vector3.up * verticalMoveLever.GetValue() * moveSpeed);

        target_Transform.SetParent(base_transform);

        // movement
        base_transform.localRotation = Quaternion.identity;
        base_transform.Rotate(Vector3.up * turnWheel.GetValue() * turnSpeed);

        float lerp = Mathf.InverseLerp(-1, 1, zoomLever.value);
        base_transform.localScale = Vector3.Lerp(Vector3.one * zoom_Min, Vector3.one * zoom_Max, lerp);

        target_Transform.SetParent(initTransform);

        UpdateObjective();

    }

    void UpdateObjective() {
        float distanceToObjective = Vector3.Distance(base_transform.position, objective_Target.position);

        if (distanceToObjective > objective_Distance) {
            Vector3 dir = (objective_Target.position - base_transform.position).normalized;
            objective_Pointer.position = base_transform.position + (dir * objective_Distance);
        } else {
            objective_Pointer.position = objective_Target.position;
        }


    }

    void HandleOnTriggerPully() {
        lateralMoveLever.Reset_Start();
        verticalMoveLever.Reset_Start();

        zoomLever.Reset_Start();

        target_Transform.DOLocalMove(initLocalPos, reset_duration);
    }

    public override void Interact_Start() {
        base.Interact_Start();

        //Player.Instance.Hide();

        if (upstairs) {
            targetCameraRoom_Upstairs.Trigger(false);
        } else {
            targetCameraRoom_Downstairs.Trigger(false);
        }

    }

    public void ToUpstairs() {
        upstairs = true;
        GetTransform.position = upstairs_Anchor.position;
    }

    public void ToDownstairs() {
        upstairs = false;
        GetTransform.position = downstairs_Anchor.position;
    }

    public override void Interact_Exit() {
        base.Interact_Exit();

        Player.Instance.Show();

        CameraRoom.previous.Trigger(false);
    }
}
