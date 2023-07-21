using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private static Player _instance;
    public static Player Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<Player>();
            }

            return _instance;
        }
    }

    private Animator _animator;
    public BoxCollider _boxCollider;

    public Transform ref_UpTest;

    private float _speed;
    public float maxSpeed = 3f;
    public float acceleration = 0.5f;
    public float decceleration = 0.5f;

    private bool _canMove = true;

    public Transform Body;
    public float rotSpeed = 1f;

    private Transform _transform;

    public Transform turn_anchor;

    [Range(0,180f)]
    public float angle_test = 0f;
    [Range(0,1f)]
    public float dot_test = 0f;

    public float deltaSpeed = 0f;
    public Vector3 previousPos;

    [Header("Collisions")]
    public LayerMask layerMask;
    public float wallDetection_Distance = 1f;

    [Header("Tween")]
    public Transform tween_anchor;
    public float tween_duration = 0.5f;
    float tween_timer;
    bool tween_active;

    public Transform rot_Target;

    public bool lockBodyRot = false;

    [Header("Rotation Raycast")]
    public float ray_distanceToPlayer = 0.2f;
    public float ray_distanceToCam = 10f;
    public LayerMask ray_LayerMask;

    // Start is called before the first frame update
    void Start()
    {
        _boxCollider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimation();

        if (_canMove)
        {
            UpdateMovement();
        }
        else
        {
            _speed = Mathf.Lerp(_speed, 0f, decceleration * Time.deltaTime);
        }

    }

    private void LateUpdate()
    {
        if (_canMove)
        {
            UpdateRotation();
        }
    }

    void UpdateAnimation()
    {
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

    void UpdateMovement()
    {
        Vector3 inputDir = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        Vector3 toCamDir = Camera.main.transform.TransformDirection(inputDir).normalized;

        Vector3 pos = GetTransform.position + toCamDir * ray_distanceToPlayer;
        Vector3 dir = (pos - Camera.main.transform.position).normalized;
        Ray ray = new Ray(Camera.main.transform.position, dir);

        Vector3 test_lookat = GetTransform.position + toCamDir;

        if (PressInput())
        {
            turn_anchor.LookAt(test_lookat, ref_UpTest.up);

            Vector3 eulerAngles = turn_anchor.localEulerAngles;
            eulerAngles.x = 0f;
            turn_anchor.localEulerAngles = eulerAngles;
        }

        // collision
        bool hitsWall = Physics.Raycast(Body.position + Body.up * 0.5f, turn_anchor.forward, wallDetection_Distance, layerMask); ;
        if (hitsWall)
        {
            _speed = Mathf.Lerp(_speed, 0f, decceleration * Time.deltaTime);
            return;
        }

        // speed
        if (PressInput())
        {
            _speed = Mathf.Lerp(_speed, maxSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            _speed = Mathf.Lerp(_speed, 0f, decceleration * Time.deltaTime);
        }

    }

    public Vector3 GetVector()
    {
        Vector3 inputDir = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        Vector3 normalizedInputDir = Camera.main.transform.TransformDirection(inputDir).normalized;

        Vector3 pos = GetTransform.position + normalizedInputDir * ray_distanceToPlayer;
        Vector3 dir = (pos - Camera.main.transform.position).normalized;
        Ray ray = new Ray(Camera.main.transform.position, dir);

        Vector3 test_lookat = GetTransform.position + normalizedInputDir;

        if (PressInput())
        {
            turn_anchor.LookAt(test_lookat, ref_UpTest.up);

            Vector3 eulerAngles = turn_anchor.localEulerAngles;
            eulerAngles.x = 0f;
            turn_anchor.localEulerAngles = eulerAngles;
        }

        return turn_anchor.forward;
    }

    private void UpdateRotation()
    {
        if (PressInput())
        {
            Vector3 eulerAngles = turn_anchor.localEulerAngles;
            eulerAngles.x = 0f;
            turn_anchor.localEulerAngles = eulerAngles;

            if (!lockBodyRot)
            {
                float tmp_RotSpeed = rotSpeed * Time.deltaTime;

                dot_test = Vector3.Dot(turn_anchor.forward, -rot_Target.up);

                dot_test = Mathf.InverseLerp(-1f, 1f, dot_test);
                dot_test = Mathf.Clamp(dot_test, 0.1f, 1f);

                Body.rotation = Quaternion.Lerp(Body.rotation, turn_anchor.rotation, tmp_RotSpeed);
            }
        }
        
        if (lockBodyRot)
        {
            GetTransform.Translate(turn_anchor.forward * _speed * Time.deltaTime, Space.World);
        }
        else
        {
            GetTransform.Translate(Body.forward * _speed * Time.deltaTime, Space.World);
        }
    }

    public bool PressInput()
    {
        return Input.GetAxis("Horizontal") != 0
            || Input.GetAxis("Vertical") != 0;
    }

    public Transform GetTransform
    {
        get
        {
            if (_transform == null)
            {
                _transform = transform;
            }

            return _transform;
        }
    }

    public Animator GetAnimator
    {
        get
        {
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }

            return _animator;
        }
    }

    public void EnableMovements()
    {
        _canMove = true;
    }

    public void DisableMovement()
    {
        _canMove = false;
    }

    public void Show()
    {
        Body.gameObject.SetActive(true);
    }

    public void Hide()
    {
        Body.gameObject.SetActive(false);
    }


}












/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private static Player _instance;
    public static Player Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<Player>();
            }

            return _instance;
        }
    }

    private Animator _animator;
    public BoxCollider _boxCollider;

    private float _speed;
    public float maxSpeed = 3f;
    public float acceleration = 0.5f;
    public float decceleration = 0.5f;

    private bool _canMove = true;

    public Transform Body;
    public float rotSpeed = 1f;

    private Transform _transform;

    public Transform turn_anchor;

    public float deltaSpeed = 0f;
    public Vector3 previousPos;

    [Header("Collisions")]
    public LayerMask layerMask;
    public float wallDetection_Distance = 1f;

    [Header("Tween")]
    public Transform tween_anchor;
    public float tween_duration = 0.5f;
    float tween_timer;
    bool tween_active;

    public bool lockBodyRot = false;

    [Header("Rotation Raycast")]
    public float ray_distanceToPlayer = 0.2f;
    public float ray_distanceToCam = 10f;
    public LayerMask ray_LayerMask;

    // Start is called before the first frame update
    void Start()
    {
        _boxCollider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimation();

        if (_canMove)
        {
            UpdateMovement();
        }
        else
        {
            _speed = Mathf.Lerp(_speed, 0f, decceleration * Time.deltaTime);
        }

    }

    private void LateUpdate()
    {
        if (_canMove)
        {
            UpdateRotation();
        }
    }


    void UpdateAnimation()
    {
        // get delta
        previousPos = GetTransform.position;
        deltaSpeed = Vector3.Distance(previousPos, GetTransform.position);

        GetAnimator.SetFloat("movement", _speed / maxSpeed);

        // update rotation
        turn_anchor.position = GetTransform.position;
    }

    void UpdateMovement()
    {
        Vector3 inputDir = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        Vector3 normalizedInputDir = Camera.main.transform.TransformDirection(inputDir).normalized;

        Vector3 pos = GetTransform.position + normalizedInputDir * ray_distanceToPlayer;
        Vector3 dir = (pos - Camera.main.transform.position).normalized;
        Ray ray = new Ray(Camera.main.transform.position, dir);

        Vector3 test_lookat = GetTransform.position + normalizedInputDir;

        if (PressInput())
        {
            Vector3 up = Vector3.up;
            if (Submarine.Instance != null) up = Submarine.Instance.GetTransform.up;
            turn_anchor.LookAt(test_lookat, up);

            Vector3 eulerAngles = turn_anchor.localEulerAngles;
            eulerAngles.x = 0f;
            turn_anchor.localEulerAngles = eulerAngles;
        }

        // collision
        bool hitsWall = Physics.Raycast(Body.position + Body.up * 0.5f, turn_anchor.forward, wallDetection_Distance, layerMask); ;
        if (hitsWall)
        {
            _speed = Mathf.Lerp(_speed, 0f, decceleration * Time.deltaTime);
            return;
        }

        // speed
        if (PressInput())
        {
            _speed = Mathf.Lerp(_speed, maxSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            _speed = Mathf.Lerp(_speed, 0f, decceleration * Time.deltaTime);
        }

    }

    private void UpdateRotation()
    {
        if (PressInput())
        {
            Vector3 eulerAngles = turn_anchor.localEulerAngles;
            eulerAngles.x = 0f;
            turn_anchor.localEulerAngles = eulerAngles;

            if (!lockBodyRot)
            {
                Body.rotation = Quaternion.Lerp(Body.rotation, turn_anchor.rotation, rotSpeed * Time.deltaTime);
            }
        }

        GetTransform.Translate(turn_anchor.forward * _speed * Time.deltaTime, Space.World);
    }

    public bool PressInput()
    {
        return Input.GetAxis("Horizontal") != 0
            || Input.GetAxis("Vertical") != 0;
    }

    public Transform GetTransform
    {
        get
        {
            if (_transform == null)
            {
                _transform = transform;
            }

            return _transform;
        }
    }

    public Animator GetAnimator
    {
        get
        {
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }

            return _animator;
        }
    }

    public void EnableMovements()
    {
        _canMove = true;
    }

    public void DisableMovement()
    {
        _canMove = false;
    }

    public void Show()
    {
        Body.gameObject.SetActive(true);
    }

    public void Hide()
    {
        Body.gameObject.SetActive(false);
    }



}
*/