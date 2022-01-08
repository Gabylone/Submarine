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

    private float _speed;
    public float maxSpeed = 3f;
    public float acceleration = 0.5f;
    public float decceleration = 0.5f;

    private bool _canMove = true;

    public Transform Body;
    public float rotSpeed = 1f;

    private Transform _transform;

    public float deltaSpeed = 0f;
    public Vector3 previousPos;

    public LayerMask layerMask;
    public float wallDetection_Distance = 1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_canMove)
        {
            UpdateMovement();
        }

    }

    void UpdateMovement()
    {
        // get delta
        previousPos = GetTransform.position;
        deltaSpeed = Vector3.Distance(previousPos, GetTransform.position);

        Vector3 dir = GetInputDirection();

        GetAnimator.SetFloat("movement", _speed / maxSpeed);

        bool hitsWall = Physics.Raycast(Player.Instance.Body.position + Vector3.up, dir, wallDetection_Distance, layerMask);
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


        if (PressInput())
        {
            Body.forward = Vector3.Lerp(Body.forward, dir, rotSpeed * Time.deltaTime);
        }


        GetTransform.Translate(dir * _speed * Time.deltaTime);

    }

    Vector3 GetInputDirection()
    {
        Vector3 inputDir = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        Vector3 dir = Camera.main.transform.TransformDirection(inputDir);
        dir.y = 0f;

        return dir;
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

    public void EnableMovement()
    {
        _canMove = true;
    }

    public void DisableMovement()
    {
        _canMove = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(Player.Instance.Body.position + Vector3.up, GetInputDirection() * wallDetection_Distance);
    }

}
