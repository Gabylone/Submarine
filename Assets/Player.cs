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

    float _speed;
    public float maxSpeed = 3f;
    public float acceleration = 0.5f;
    public float decceleration = 0.5f;

    private bool _canMove = true;

    public Transform Body;
    public float rotSpeed = 1f;

    private Transform _transform;

    public float deltaSpeed = 0f;
    public Vector3 previousPos;

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
        previousPos = GetTransform.position;

        if (PressInput())
        {
            _speed = Mathf.Lerp(_speed, maxSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            _speed = Mathf.Lerp(_speed, 0f, decceleration * Time.deltaTime);
        }

        deltaSpeed = Vector3.Distance(previousPos, GetTransform.position);

        Vector3 inputDir = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        Vector3 dir = Camera.main.transform.TransformDirection(inputDir);
        dir.y = 0f;


        if (PressInput())
        {
        Body.forward = Vector3.Lerp(Body.forward, dir, rotSpeed * Time.deltaTime);
            //Body.forward = dir;
        }

        //GetTransform.Translate(Body.forward * _speed * Time.deltaTime);
        GetTransform.Translate(dir * _speed * Time.deltaTime);

        GetAnimator.SetFloat("movement", _speed / maxSpeed);
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


}
