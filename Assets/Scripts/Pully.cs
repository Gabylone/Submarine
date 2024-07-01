using UnityEngine;

public class Pully : Interactable {
    public Transform _targetTransform;

    public float pull_speed = 2f;

    public Transform cable_transform;

    public float distanceToTrigger = 10f;
    public float distanceToCatch = 0.2f;
    public float distanceToLeave = 2f;

    public Transform ik_target;

    public float speedToHand = 4f;

    public float speedToBase = 4f;

    bool caught = false;

    public delegate void OnTrigger();
    public OnTrigger onTrigger;

    public override void Interact_Start() {
        base.Interact_Start();

        IKManager.Instance.SetTarget(IKParam.Type.LeftHand, ik_target);
        IKManager.Instance.SetTarget(IKParam.Type.RightHand, ik_target);

        caught = false;

        Player.Instance.LockBodyRot(true);
    }

    public override void Update() {
        base.Update();

        float dis = Vector3.Distance(_targetTransform.position, cable_transform.position);
        cable_transform.localScale = new Vector3(cable_transform.localScale.x, dis, cable_transform.localScale.z);
        Vector3 dir = _targetTransform.position - cable_transform.position;
        cable_transform.up = -dir.normalized;

        if (!interacting) {
            _targetTransform.position = Vector3.Lerp(_targetTransform.position, cable_transform.position, speedToBase * Time.deltaTime);
        }
    }

    public override void Interact_Update() {
        base.Interact_Update();

        Vector3 handPos = IKManager.Instance._animator.GetBoneTransform(HumanBodyBones.RightHand).position;
        float distanceToHand = Vector3.Distance(_targetTransform.position, handPos);

        Vector3 dir = GetTransform.position - Player.Instance.GetTransform.position;
        //dir.y = 0f;
        //dir = Submarine.Instance.GetTransform.TransformDirection(dir);
        Player.Instance.Body.forward = dir;
        //Player.Instance.Body.LookAt(cable_transform, Submarine.Instance.GetTransform.up);

        if (caught) {
            _targetTransform.position = Vector3.Lerp(_targetTransform.position, handPos, speedToHand * Time.deltaTime);

            float distanceToBase = Vector3.Distance(_targetTransform.position, cable_transform.position);

            if (distanceToBase >= distanceToTrigger) {
                Trigger();
            }
        } else {
            if (distanceToHand <= distanceToCatch) {
                caught = true;

                ik_target.position = IKManager.Instance._animator.GetBoneTransform(HumanBodyBones.RightHand).position;
                IKManager.Instance.SetTarget(IKParam.Type.RightHand, ik_target);
                ik_target.parent = Player.Instance.Body;
            }

            if (distanceToHand >= distanceToLeave) {
                Interact_Exit();
            }
        }
    }

    void Trigger() {
        Interact_Exit();

        if (onTrigger != null) {
            onTrigger();
        }
    }


    public override void Interact_Exit() {
        base.Interact_Exit();

        Player.Instance.LockBodyRot(false);

        IKManager.Instance.StopAll();

    }


}
