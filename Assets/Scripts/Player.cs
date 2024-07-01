using UnityEngine;

public class Player : MonoBehaviour {

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

    /// <summary>
    /// COMPONENTS
    /// </summary>
    private Animator _animator;
    private BoxCollider _boxCollider;
    private Transform _transform;
    private Transform turn_anchor;

    /// <summary>
    /// MOVEMENT
    /// </summary>
    [Header("Movement")]
    [Space]
    public float maxSpeed = 3f;
    public float acceleration = 0.5f;
    public float decceleration = 0.5f;
    private float _speed;
    private bool _canMove = true;

    public float deltaSpeed = 0f;
    public Vector3 previousPos;

    /// <summary>
    /// TURN
    /// </summary>
    [Header("Rotation")]
    [Space]
    public Transform Body;
    public float rotSpeed = 1f;
    private bool lockBodyRot = false;
    public float ray_distanceToPlayer = 0.2f;

    /// <summary>
    /// COLISION
    /// </summary>
    [Header("Collisions")]
    public LayerMask layerMask;
    public float wallDetection_Distance = 1f;

    #region constructors
    // Start is called before the first frame update
    void Start() {
        _boxCollider = GetComponent<BoxCollider>();

        if (turn_anchor == null) {
            turn_anchor = new GameObject().transform;
            turn_anchor.parent = GetTransform.parent;
            turn_anchor.name = "Turn Anchor (Player)";
        }
    }

    // Update is called once per frame
    void Update() {
        UpdateAnimation();

        if (_canMove) {
            UpdateMovement();
        } else {
            _speed = Mathf.Lerp(_speed, 0f, decceleration * Time.deltaTime);
        }

    }

    private void LateUpdate() {
        if (_canMove) {
            UpdateRotation();
        }
    }
    #endregion

    void UpdateAnimation() {
        // get delta
        deltaSpeed = Vector3.Distance(previousPos, GetTransform.position);
        previousPos = GetTransform.position;


        deltaSpeed = Mathf.Clamp01(deltaSpeed);

        //float _animationDelta = (_speed / maxSpeed) * deltaSpeed;
        float _animationDelta = (_speed / maxSpeed);
        GetAnimator.SetFloat("movement", _animationDelta);


        // update rotation
        turn_anchor.position = GetTransform.position;

        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    void UpdateMovement() {
        Vector3 inputDir = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        Vector3 toCamDir = Camera.main.transform.TransformDirection(inputDir).normalized;

        Vector3 pos = GetTransform.position + toCamDir * ray_distanceToPlayer;
        Vector3 dir = (pos - Camera.main.transform.position).normalized;
        Ray ray = new Ray(Camera.main.transform.position, dir);

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

        bool hitsGround = Physics.Raycast(GetTransform.position, -Body.up + Body.up * 0.2f, 0.1f, layerMask);
        if (!hitsGround) {
            GetTransform.Translate(-Body.up * 0.58f * Time.deltaTime);
        }

        // speed
        if (PressInput()) {
            _speed = Mathf.Lerp(_speed, maxSpeed, acceleration * Time.deltaTime);
        } else {
            _speed = Mathf.Lerp(_speed, 0f, decceleration * Time.deltaTime);
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

        if (lockBodyRot) {
            GetTransform.Translate(turn_anchor.forward * _speed * Time.deltaTime, Space.World);
        } else {
            GetTransform.Translate(Body.forward * _speed * Time.deltaTime, Space.World);
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
}