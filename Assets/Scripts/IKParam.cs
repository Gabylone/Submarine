using UnityEngine;

[System.Serializable]
public class IKParam {
    public enum Type {
        LeftFoot,
        RightFoot,
        LeftHand,
        RightHand,
        Head,
        Body
    }

    public Type type;
    public bool active = false;
    [HideInInspector]
    public float weight = 0f;
    public float weight_speed = 1f;
    public Transform target;

    public HumanBodyBones humanBodyBone;
    public AvatarIKGoal avatarIKGoal;

    Vector3 transition_InitPos;
    Quaternion transition_InitRot;
    private float transition_lerp = 0f;
    private float transitionTimer = 0f;
    public float transitionSpeed = 2f;
    public float transition_duration = 0.3f;

    public Transform _spine;
    private Transform transition;
    private Transform relativePoint;
    public static Transform _transitionParent;

    public void Init() {
        if (_transitionParent == null) {
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

    public void Start(Transform _target) {
        target = _target;
        active = true;

        transitionTimer = 0f;

        if (type == IKParam.Type.Body) {
            transition.position = _spine.position;
            transition.rotation = _spine.rotation;
        } else {
            transition.position = Player.Instance.GetAnimator.GetBoneTransform(humanBodyBone).position;
            transition.rotation = Player.Instance.GetAnimator.GetBoneTransform(humanBodyBone).rotation;
        }

        relativePoint.position = transition.position;
        relativePoint.rotation = transition.rotation;

    }

    public void Update() {
        UpdateTransition();
        UpdateWeight();
        UpdateTarget();
    }

    public void UpdateWeight() {
        if (active) {
            weight = Mathf.Lerp(weight, 1f, weight_speed * Time.deltaTime);
        } else {
            weight = Mathf.Lerp(weight, 0f, weight_speed * Time.deltaTime * 3f);
        }

        switch (type) {
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

    public void UpdateTransition() {
        if (!active) {
            return;
        }

        transition_lerp = transitionTimer / transition_duration;

        if (target != null) {
            transition.position = Vector3.Lerp(relativePoint.position, target.position, transition_lerp); ;
            transition.rotation = Quaternion.Lerp(relativePoint.rotation, target.rotation, transition_lerp);
        }

        transitionTimer += Time.deltaTime;
    }

    public void UpdateTarget() {
        if (target == null) {
            return;
        }

        if (type == Type.Head) {
            IKManager.Instance._animator.SetLookAtPosition(transition.position);
        } else if (type == Type.Body) {
            if (active) {
                _spine.position = Vector3.Lerp(_spine.position, transition.position, transition_lerp);
                _spine.rotation = Quaternion.Lerp(_spine.rotation, transition.rotation, transition_lerp);
            } else {
                _spine.localPosition = Vector3.Lerp(_spine.localPosition, Vector3.zero, transitionSpeed * Time.deltaTime);
                /*_spine.localRotation = Quaternion.Lerp(_spine.localRotation, Quaternion.identity, transitionSpeed * Time.deltaTime);*/
            }
        } else {
            IKManager.Instance._animator.SetIKPosition(avatarIKGoal, transition.position);

        }
    }

    public void DrawGizmos() {
        if (transition != null) {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transition.position, 0.05f);
        }

        if (target != null) {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(target.position, 0.05f);
            Gizmos.DrawLine(Player.Instance.GetAnimator.GetBoneTransform(humanBodyBone).position, target.position);
        }
    }
}