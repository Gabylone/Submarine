using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class Lever : Interactable
{
    public float rotateSpeed = 50f;

    public Transform _targetTransform;

    [Range(0,1)]
    public float lerp = 0f;

    public Transform playerBody_Target;
    public Transform playerBody_A;
    public Transform playerBody_B;

    public Quaternion rot_A;
    public Quaternion rot_B;

    public override void Interact_Start()
    {
        base.Interact_Start();

        Player.Instance.DisableMovement();
    }

    public override void Interact_Update()
    {
        base.Interact_Update();

        UpdateInput();

        IKManager.Instance.SetTarget(IKManager.IKParam.Type.Body, playerBody_Target);
    }

    public override void Interact_LateUpdate()
    {
        base.Interact_LateUpdate();

        lerp = Mathf.InverseLerp(rot_A.x, rot_B.x, _targetTransform.localRotation.x);

        playerBody_Target.rotation = Quaternion.Lerp(playerBody_A.rotation, playerBody_B.rotation, lerp);
        playerBody_Target.position = Vector3.Lerp(playerBody_A.position, playerBody_B.position, lerp);
    }

    void UpdateInput()
    {
        if (Input.GetAxis("Horizontal") > -.1f
                &&
                Input.GetAxis("Horizontal") < .1f
                )
        {
            return;
        }


        if (Input.GetAxis("Horizontal") < 0)
        {
            _targetTransform.localRotation = Quaternion.Lerp(_targetTransform.localRotation, rot_A, rotateSpeed * Time.deltaTime);
        }
        else
        {
            _targetTransform.localRotation = Quaternion.Lerp(_targetTransform.localRotation, rot_B, rotateSpeed  * Time.deltaTime);
        }
    }


    public override void Interact_Exit()
    {
        base.Interact_Exit();

        Player.Instance.EnableMovement();

        IKManager.Instance.StopAll();

    }


}
