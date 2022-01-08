using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTrigger : Interactable
{
    public Transform _targetTransform;

    public float disToTrigger = .5f;

    public bool pressed = false;

    public Renderer button_rend;

    public Door door;

    public override void Start()
    {
        base.Start();

        door.onSwitch += HandleOnDoorSwitch;
    }

    public override void Interact_Start()
    {
        base.Interact_Start();

        pressed = false;
    }

    public override void Interact_Update()
    {
        base.Interact_Update();

        if (!pressed)
        {
            float disToLeftHand = Vector3.Distance(IKManager.Instance._animator.GetBoneTransform(HumanBodyBones.RightHand).position, _targetTransform.position);

            if (disToLeftHand < disToTrigger)
            {
                Trigger();
            }

        }

    }

    void Trigger()
    {
        pressed = true;

        door.Switch();

        Interact_Exit();

        Debug.Log("trigger");

        Tween.Bounce(_targetTransform);
    }

    public void HandleOnDoorSwitch()
    {
        if (door.opened)
        {
            button_rend.material.color = Color.red;
        }
        else
        {
            button_rend.material.color = Color.gray;
        }
    }

    public override void Interact_Exit()
    {
        base.Interact_Exit();

        IKManager.Instance.StopAll();
    }

    public override void Update()
    {
        base.Update();
    }

    private void OnDrawGizmos()
    {

        if (pressed)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.blue;
        }

        Gizmos.DrawWireSphere(_targetTransform.position, disToTrigger);

    }

}
