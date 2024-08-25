using UnityEditor;
using UnityEngine;

public class IKManager : MonoBehaviour {

    static IKManager _instance;
    public static IKManager Instance {
        get {
            if (_instance == null) {
                _instance = GameObject.FindObjectOfType<IKManager>();
            }

            return _instance;
        }
    }

    public Transform headLook;
    public Animator _animator;
    public IKParam[] ikMainParams;
    public float weight_Speed = 1f;

    public float feetIK_Distance = 0.4f;
    public Vector3 feetIK_DetectionDecal = Vector3.zero;
    public Vector3 feedIK_CollisionDecal = Vector3.zero;

    /// <summary>
    ///  STEPS & FEET IK
    /// </summary>

    private void Start() {
        foreach (var item in ikMainParams) {
            item.Init();
        }
    }

    public IKParam GetIKParam(IKParam.Type type) {
        return ikMainParams[(int)type];
    }

    private void Update() {
        GetIKParam(IKParam.Type.Body).Update();
    }

    public void SetTarget(IKParam.Type type, Transform target) {
        GetIKParam(type).Start(target);
    }

    public void StopAll() {
        for (int i = 0; i < 6; i++) {
            Stop((IKParam.Type)i);
        }
    } 

    public void StopFeet() {
        Stop(IKParam.Type.LeftFoot);
        Stop(IKParam.Type.RightFoot);
    }

    public void StopHands() {
        Stop(IKParam.Type.LeftHand);
        Stop(IKParam.Type.RightHand);
    }

    public void Stop(IKParam.Type type) {
        GetIKParam(type).active = false;
    }

    // tests
    private void OnAnimatorIK(int layerIndex) {
        _animator.SetLookAtWeight(GetIKParam(IKParam.Type.Head).weight);

        for (int index = 0; index < ikMainParams.Length - 1; index++) {
            IKParam.Type type = (IKParam.Type)index;
            IKParam ikParam = GetIKParam(type);
            ikParam.Update();
        }

        // floor ( after all )
        for (int i = 0; i < 2; i++) {
            RaycastHit hit;
            var bone = i == 0 ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot;
            var ikGoal = i == 0 ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot;
            var foot = _animator.GetBoneTransform(bone);
            var origin = foot.position + Player.Instance.Body.TransformDirection(feetIK_DetectionDecal);
            if (Physics.Raycast(origin, -Vector3.up, out hit, feetIK_Distance, LayerMask.GetMask("Stair Step"))) {
                _animator.SetIKPosition(ikGoal, hit.point + Player.Instance.Body.TransformDirection(feedIK_CollisionDecal));
                _animator.SetIKPositionWeight(ikGoal, 1f);
            }
        }
    }

    private void OnDrawGizmos() {

        /*foreach (var item in ikMainParams) {
            item.DrawGizmos();
        }*/
    }
}
