using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Transform _transform;
    Animator _animator;
    public Transform bodyTransform;

    public float speed = 1f;
    float currentSpeed = 0f;
    public float acceleration = 1f;
    public float decceleration = 5f;
    public float turnSpeed = 1f;
    Vector3 currentDir;
    

    // Start is called before the first frame update
    void Start()
    {
        _transform = transform;
        _animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 inputDir = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        Vector3 dir = Camera.main.transform.TransformDirection(inputDir);
        dir.y = 0f;


        if (inputDir == Vector3.zero)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, decceleration * Time.deltaTime);
        }
        else
        {
            currentDir = dir;
            currentSpeed = Mathf.Lerp(currentSpeed, speed, acceleration * Time.deltaTime);
            bodyTransform.forward = Vector3.Lerp(bodyTransform.forward, dir, turnSpeed * Time.deltaTime);

        }

        //_transform.Translate(currentDir * currentSpeed * Time.deltaTime, Space.World);
        _transform.Translate(bodyTransform.forward * currentSpeed * Time.deltaTime, Space.World);

        _animator.SetFloat("movement", currentSpeed/speed);
    }
}
