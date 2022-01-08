using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pully : Interactable
{
    public Transform _targetTransform;

    public float pull_speed = 2f;

    public Transform cable_transform;

    public float distanceToTrigger = 10f;
    public float distanceToCatch = 0.2f;

    public Transform ik_target;

    Vector3 initPos;

    public float speedToHand = 4f;

    public float speedToBase = 4f;

    bool caught = false;

    public override void Start()
    {
        base.Start();

        initPos = _targetTransform.position;

    }

    public override void Interact_Start()
    {
        base.Interact_Start();


        caught = false;

        IKManager.Instance.SetTarget(IKManager.IKParam.Type.RightHand, _targetTransform);
    }

    public override void Update()
    {
        base.Update();

        float dis = Vector3.Distance(_targetTransform.position, cable_transform.position);
        cable_transform.localScale = new Vector3(cable_transform.localScale.x, dis, cable_transform.localScale.z);
        Vector3 dir = _targetTransform.position - cable_transform.position;
        cable_transform.up = -dir.normalized;

        if (!interacting)
        {
            _targetTransform.position = Vector3.Lerp( _targetTransform.position , initPos , speedToBase * Time.deltaTime );
        }
    }

    public override void Interact_Update()
    {
        base.Interact_Update();

        Vector3 handPos = IKManager.Instance._animator.GetBoneTransform(HumanBodyBones.RightHand).position;
        float distanceToHand = Vector3.Distance(_targetTransform.position, handPos);

        if (caught)
        {
            _targetTransform.position = Vector3.Lerp( _targetTransform.position , handPos , speedToHand * Time.deltaTime );

            float distanceToBase = Vector3.Distance(_targetTransform.position, initPos);

            if (distanceToBase >= distanceToTrigger)
            {
                Trigger();
            }
        }
        else
        {

            if (distanceToHand <= distanceToCatch)
            {
                caught = true;

                ik_target.position = IKManager.Instance._animator.GetBoneTransform(HumanBodyBones.RightHand).position;
                IKManager.Instance.SetTarget(IKManager.IKParam.Type.RightHand, ik_target);
                ik_target.parent = Player.Instance.Body;

            }
        }
    }

    void Trigger()
    {
        Interact_Exit();
    }


    public override void Interact_Exit()
    {
        base.Interact_Exit();

        IKManager.Instance.StopAll();

    }


}
