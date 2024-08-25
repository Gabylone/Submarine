using AdvancedPeopleSystem;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Player : MonoBehaviour {
    List<Vector3> debugoses = new List<Vector3>();
    /// <summary>
    /// SINGLETON
    /// </summary>
    private static Player _instance;
    public static Player Instance {
        get {
            if (_instance == null) {
                _instance = GameObject.FindObjectOfType<Player>();
            }

            return _instance;
        }
    }

    public Transform camera_Transform;
    public Transform camera_LookAt;

    /// <summary>
    /// CHARACTER CUSTOMIZATION
    /// </summary>
    public CharacterCustomization characterCustomization;

    /// <summary>
    /// COMPONENTS
    /// </summary>
    private Animator _animator;
    private Transform _transform;
    private Transform turn_anchor;

    /// <summary>
    /// MOVEMENT
    /// </summary>
    [Header("Movement")]
    [Space]
    public float currentMaxSpeed = 3f;
    public float flatMaxSpeed = 3f;
    public float climbMaxSpeed = 0f;
    public float acceleration = 0.5f;

    public float decceleration = 0.5f;
    private float _speed;
    private bool _canMove = true;

    public float deltaSpeed = 0f;
    public Vector3 previousPos;

    [Header("Ground Detection")]
    public Vector3 grounded_DetectionDecal;
    public float grounded_Distance = 1f;
    [Range(-1, 1)]
    public float grounded_Dot = 0f;
    public float grounded_MinAngle = 0.5f;
    [Range(0, 1)]
    public float testlerp = 0f;
    public float grounded_MaxDot = 0.8f;
    public float grounded_MinClimbSpeed = 1f;
    public LayerMask grounded_LayerMask;
    public bool isGrounded;

    [Header("Gravity")]
    public bool enableGravity = true;
    public float gravity_SpeedToGround = 1f;
    public float gravity_CurrentSpeed = 0f;
    public float gravity_Acceleration;
    public Vector2 gravity_SpeedRange;

    /// <summary>
    /// TURN
    /// </summary>
    [Header("Rotation")]
    [Space]
    public Transform Body;
    public float rotSpeed = 1f;
    private bool lockBodyRot = false;
    public float ray_distanceToPlayer = 0.2f;
    public bool accordingToCamera = false;

    public Transform temp_Parent;

    [System.Serializable]
    public class RagdollPart {
        public string name;
        public Transform target;
        public Vector3 center;
        public Vector3 size;
        public float mass;
    }

    public List<RagdollPart> parts = new List<RagdollPart>();

    /// <summary>
    /// COLISION
    /// </summary>
    [Header("Collisions")]
    public LayerMask layerMask;
    public float wallDetection_Distance = 1f;

    #region constructors
    // Start is called before the first frame update
    void Start() {
        SetRagdoll(false);

        characterCustomization.Randomize();


        if (turn_anchor == null) {
            turn_anchor = new GameObject().transform;
            turn_anchor.parent = GetTransform.parent;
            turn_anchor.name = "Turn Anchor (Player)";
        }
    }

    // Update is called once per frame
    void Update() {
        UpdateAnimation();

        UpdateMovement();

        if (Input.GetKeyDown(KeyCode.L)) {
            ragdoll = !ragdoll;
            SetRagdoll(ragdoll);
        }

    }

    bool ragdoll = false;
    private void LateUpdate() {
        if (_canMove) {
            UpdateRotation();

            
        }
    }

    void SetRagdoll(bool b) {
        _canMove = !b;
        GetAnimator.enabled = !b;
        var rgs = temp_Parent.GetComponentsInChildren<Rigidbody>();
        foreach (var item in rgs) {
            item.isKinematic = !b;
            item.GetComponent<Collider>().enabled = b;
        }
    }
    #endregion

    void UpdateAnimation() {
        // get delta
        deltaSpeed = Vector3.Distance(previousPos, GetTransform.position);
        previousPos = GetTransform.position;


        deltaSpeed = Mathf.Clamp01(deltaSpeed);

        //float _animationDelta = (_speed / maxSpeed) * deltaSpeed;
        float _animationDelta = (_speed / currentMaxSpeed);
        GetAnimator.SetFloat("movement", _animationDelta);

        // update rotation
        turn_anchor.position = GetTransform.position;
    }

    void UpdateMovement() {

        if (!_canMove) {
            _speed = Mathf.Lerp(_speed, 0f, decceleration * Time.deltaTime);
            return;
        }

        Vector3 inputDir = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        Vector3 toCamDir = camera_Transform.TransformDirection(inputDir).normalized;

        Vector3 pos = GetTransform.position + toCamDir * ray_distanceToPlayer;
        Vector3 dir = (pos - camera_Transform.position).normalized;
        Ray ray = new Ray(camera_Transform.position, dir);

        Vector3 test_lookat = GetTransform.position + toCamDir;

        if (PressInput()) {
            turn_anchor.LookAt(test_lookat, RotationRef.Instance.GetUpDirection());

            Vector3 eulerAngles = turn_anchor.localEulerAngles;
            eulerAngles.x = 0f;
            turn_anchor.localEulerAngles = eulerAngles;
        }

        // collision
        bool hitsWall = Physics.Raycast(Body.position + Body.up * 0.5f, Body.forward, wallDetection_Distance, layerMask); ;
        if (hitsWall) {
            _speed = Mathf.Lerp(_speed, 0f, decceleration * Time.deltaTime);
            return;
        }

        // speed
        if (PressInput()) {
            _speed = Mathf.Lerp(_speed, currentMaxSpeed, acceleration * Time.deltaTime);
        } else {
            _speed = Mathf.Lerp(_speed, 0f, decceleration * Time.deltaTime);
        }

        UpdateGravity();

        if (lockBodyRot) {
            GetTransform.Translate(turn_anchor.forward * _speed * Time.deltaTime, Space.World);
        } else {
            GetTransform.Translate(Body.forward * _speed * Time.deltaTime, Space.World);
        }
    }

    public void EnableGravity() {
        enableGravity = true;
    }

    public void DisableGravity() {
        _animator.SetBool("falling", false);
        enableGravity = false;
    }

    void UpdateGravity() {
        if (!enableGravity)
            return;
        var origin = GetTransform.position + Body.TransformDirection(grounded_DetectionDecal);
        RaycastHit hit;
        isGrounded = Physics.Raycast(origin, -Body.up, out hit, grounded_Distance, grounded_LayerMask);
        GetAnimator.SetBool("falling", !isGrounded);
        if (isGrounded) {
            grounded_Dot = Vector3.Dot(hit.normal, RotationRef.Instance.GetUpDirection());
            var lerp  = Mathf.InverseLerp(grounded_MinAngle, 1f, grounded_Dot);
            currentMaxSpeed = Mathf.Lerp(climbMaxSpeed, flatMaxSpeed, lerp);

            Debug.DrawLine(origin, hit.point, Color.green);
            debugoses.Add(hit.point);
            if (debugoses.Count > 100)
                debugoses.RemoveAt(0);
            GetTransform.position = hit.point;
            //GetTransform.position = Vector3.Lerp(GetTransform.position, hit.point, gravity_SpeedToGround * Time.deltaTime);
            gravity_CurrentSpeed = 0f;
        } else {
            Debug.DrawRay(origin, -Body.up * grounded_Distance, Color.red);
            gravity_CurrentSpeed = Mathf.Lerp(gravity_CurrentSpeed, gravity_SpeedRange.y, gravity_Acceleration * Time.timeScale);
            GetTransform.Translate(-RotationRef.Instance.GetUpDirection() * gravity_CurrentSpeed * Time.deltaTime);
        }
    }

    private void UpdateRotation() {
        if (PressInput()) {
            Vector3 eulerAngles = turn_anchor.localEulerAngles;
            eulerAngles.x = 0f;
            turn_anchor.localEulerAngles = eulerAngles;

            if (!lockBodyRot) {
                float tmp_RotSpeed = rotSpeed * Time.deltaTime;
                Body.rotation = Quaternion.Lerp(Body.rotation, turn_anchor.rotation, tmp_RotSpeed);
            }
        }
    }

    public bool PressInput() {
        return Input.GetAxis("Horizontal") != 0
            || Input.GetAxis("Vertical") != 0;
    }

    public Transform GetTransform {
        get {
            if (_transform == null) {
                _transform = transform;
            }

            return _transform;
        }
    }

    public Animator GetAnimator {
        get {
            if (_animator == null) {
                _animator = GetComponentInChildren<Animator>();
            }

            return _animator;
        }
    }

    public void EnableMovements() {
        _canMove = true;
    }

    public void DisableMovement() {
        _canMove = false;
    }

    public void Show() {
        Body.gameObject.SetActive(true);
    }

    public void Hide() {
        Body.gameObject.SetActive(false);
    }

    public void LockBodyRot(bool b) {
        lockBodyRot = b;
    }

    private void OnDrawGizmos() {
        foreach (var item in debugoses) {
            Gizmos.DrawSphere(item, 0.1f);
        }
    }
}