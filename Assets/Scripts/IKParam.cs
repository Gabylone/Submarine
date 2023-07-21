using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IKParam
{
    public enum Type
    {
        LeftFoot,
        RightFoot,
        LeftHand,
        RightHand,
        Head,
        Body
    }

    public Type type;
    public bool active = false;
    public float weight = 0f;
    public float weight_speed = 1f;
    public Transform _transform;
    public Transform target;
    
    public HumanBodyBones humanBodyBone;
    public AvatarIKGoal avatarIKGoal;

    
    // transition //
    public Transform transition;

    Vector3 transition_InitPos;
    Quaternion transition_InitRot;
    public float transition_lerp = 0f;
    public float transitionTimer = 0f;
    public float transitionSpeed = 2f;

    public float transition_duration = 0.3f;
    public float transition_acceleration = 1f;

    public Transform relativePoint;

    public static Transform _transitionParent;

    public void Init()
    {
        if (_transitionParent == null)
        {
            _transitionParent = new GameObject().transform;
            _transitionParent.parent = Player.Instance.GetTransform;
            _transitionParent.name = "[IK Transitions]";
        }

        GameObject obj = new GameObject();
        obj.transform.parent = _transitionParent;
        obj.name = humanBodyBone + " (transition)";
        transition = obj.transform;

        GameObject relativePoint_obj = new GameObject();
        relativePoint_obj.name = "Relative Point (" + humanBodyBone + ")";
        relativePoint_obj.transform.parent = _transitionParent;
        relativePoint = relativePoint_obj.transform;
    }

    public void Start(Transform _target)
    {
        target = _target;
        active = true;

        transitionTimer = 0f;

        if (type == IKParam.Type.Body)
        {
            transition.position = _transform.position;
            transition.rotation= _transform.rotation;
        }
        else
        {
            transition.position  = Player.Instance.GetAnimator.GetBoneTransform(humanBodyBone).position;
            transition.rotation = Player.Instance.GetAnimator.GetBoneTransform(humanBodyBone).rotation;
        }

        relativePoint.position = transition.position;
        relativePoint.rotation = transition.rotation;

    }

    public void Update()
    {
        UpdateTransition();
        UpdateWeight();
        UpdateTarget();
    }

    public void UpdateWeight()
    {
        if (active)
        {
            weight = Mathf.Lerp(weight, 1f, weight_speed * Time.deltaTime);
        }
        else
        {
            weight = Mathf.Lerp(weight, 0f, weight_speed * Time.deltaTime * 3f);
        }

        switch (type)
        {
            case IKParam.Type.LeftFoot:
            case IKParam.Type.RightFoot:
            case IKParam.Type.LeftHand:
            case IKParam.Type.RightHand:
                IKManager.Instance._animator.SetIKPositionWeight(avatarIKGoal, weight);
                break;
            case IKParam.Type.Head:
                IKManager.Instance._animator.SetLookAtWeight(weight);
                break;
            case IKParam.Type.Body:
                // no need, see update target
                break;
            default:
                break;
        }
    }

    public void UpdateTransition()
    {
        if (!active)
        {
            return;
        }

        transition_lerp = transitionTimer / transition_duration;

        if (target != null)
        {
            transition.position = Vector3.Lerp(relativePoint.position, target.position, transition_lerp); ;
            transition.rotation = Quaternion.Lerp(relativePoint.rotation, target.rotation, transition_lerp);
        }

        transitionTimer += Time.deltaTime;
    }

    public void UpdateTarget()
    {
        switch (type)
        {
            case IKParam.Type.LeftFoot:
            case IKParam.Type.RightFoot:
            case IKParam.Type.LeftHand:
            case IKParam.Type.RightHand:

                // pas sur que cette ligne soit utile
                if (target == null)
                    return;

                IKManager.Instance._animator.SetIKPosition(avatarIKGoal, transition.position);
                break;
            case IKParam.Type.Head:

                // pas sur que cette ligne soit utile
                if (target == null)
                    return;

                IKManager.Instance._animator.SetLookAtPosition(transition.position);
                break;
            case IKParam.Type.Body:
                if (active)
                {
                    _transform.position = Vector3.Lerp(_transform.position, transition.position, transition_lerp);
                    _transform.rotation = Quaternion.Lerp(_transform.rotation, transition.rotation, transition_lerp);
                }
                else
                {
                    _transform.localPosition = Vector3.Lerp(_transform.localPosition, Vector3.zero, transitionSpeed * Time.deltaTime);
                    _transform.localRotation= Quaternion.Lerp(_transform.localRotation, Quaternion.identity, transitionSpeed * Time.deltaTime);
                }
                break;
            default:
                break;
        }
    }

    public void DrawGizmos()
    {
        if (transition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transition.position, 0.05f);
        }

        if (target != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(target.position, 0.05f);
            Gizmos.DrawLine(Player.Instance.GetAnimator.GetBoneTransform(humanBodyBone).position, target.position);
        }
    }
}