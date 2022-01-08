using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class IKManager : MonoBehaviour
{
    static IKManager _instance;
    public static IKManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<IKManager>();
            }

            return _instance;
        }
    }

    public Animator _animator;

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
        public float transition_speed = 2f;
        public float transition_acceleration = 1f;
        public float maxTransitionSpeed = 50f;
        public Transform _transform;
        public Transform target;
        [HideInInspector]
        public Transform transition;
        public HumanBodyBones humanBodyBone;
        public AvatarIKGoal avatarIKGoal;

        public static Transform _transitionParent;

        public void Init()
        {
            if ( _transitionParent == null)
            {
                _transitionParent = new GameObject().transform;
                _transitionParent.name = "[IK Transitions]";
            }

            GameObject obj = new GameObject();
            obj.transform.parent = _transitionParent;
            obj.name = humanBodyBone + " (transition)";
            transition = obj.transform;
            transition.position = Player.Instance.Body.position;
            transition.rotation = Player.Instance.Body.rotation;
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

        public void UpdateTarget()
        {
            if (target!= null)
            {
                transition.position = Vector3.Lerp(transition.position, target.position, transition_speed * Time.deltaTime); ;
                transition.rotation = Quaternion.Lerp(transition.rotation, target.rotation, transition_speed * Time.deltaTime);

                transition_speed = Mathf.Lerp(transition_speed, maxTransitionSpeed, transition_acceleration * Time.deltaTime );
            }
            

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
                        _transform.position = Vector3.Lerp(_transform.position, transition.position, transition_speed * Time.deltaTime);
                        _transform.rotation = Quaternion.Lerp(_transform.rotation, transition.rotation, transition_speed * Time.deltaTime);
                    }
                    else
                    {
                        _transform.localPosition = Vector3.Lerp(_transform.localPosition, Vector3.zero, transition_speed * Time.deltaTime);
                        _transform.localRotation = Quaternion.Lerp(_transform.localRotation, Quaternion.identity, transition_speed * Time.deltaTime);
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
    public IKParam[] ikParams;
    public IKParam GetIKParam (IKParam.Type type)
    {
        return ikParams[(int)type];
    }

    public float weight_Speed = 1f;

    private void Start()
    {
        foreach (var item in ikParams)
        {
            item.Init();
        }
    }

    private void Update()
    {
        GetIKParam(IKParam.Type.Body).UpdateTarget();
        GetIKParam(IKParam.Type.Body).UpdateWeight();
    }

    public void SetTarget(IKParam.Type type, Transform target)
    {
        GetIKParam(type).target = target;
        GetIKParam(type).active = true;
        GetIKParam(type).transition_speed = 1f;
    }

    public void StopAll()
    {
        for (int i = 0; i < 6; i++)
        {
            Stop((IKParam.Type)i);
        }
    }

    public void StopFeet()
    {
        Stop(IKParam.Type.LeftFoot);
        Stop(IKParam.Type.RightFoot);
    }

    public void StopHands()
    {
        Stop(IKParam.Type.LeftHand);
        Stop(IKParam.Type.RightHand);
    }

    public void Stop(IKParam.Type type)
    {
        GetIKParam(type).active = false;
        GetIKParam(type).transition_speed = 1f;
    }

    // tests
    private void OnAnimatorIK(int layerIndex)
    {
        _animator.SetLookAtWeight(GetIKParam(IKParam.Type.Head).weight);

        for (int index = 0; index < ikParams.Length-1; index++)
        {
            IKParam.Type type = (IKParam.Type)index;
            IKParam ikParam = GetIKParam(type);

            ikParam.UpdateTarget();
            ikParam.UpdateWeight();
        }
    }

    private void OnDrawGizmos()
    {
        foreach (var item in ikParams)
        {
            item.DrawGizmos();
        }
    }
}
