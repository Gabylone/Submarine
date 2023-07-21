using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class Lever : Interactable
{
    public float rotateSpeed = 50f;

    public Transform _targetTransform;

    public float value;
    public float valueMultiplier = 1f;

    public float lerp = 0f;

    public Transform playerBody_Target;
    public Transform playerBody_A;
    public Transform playerBody_B;

    public Quaternion rot_A;
    public Quaternion rot_B;

    public float reset_duration = 2f;
    public float reset_timer = 0f;
    public bool reseting = false;

    Quaternion reset_InitRot;
    Quaternion reset_LerpRot;

    public float loop_MinPitch = 0.5f;
    public float loop_MaxPitch = 1f;

    public float effect_MinPitch = 0.5f;
    public float effect_MaxPitch = 1f;

    public AudioClip sound_StopClip;
    public AudioClip sound_StartClip;
    public AudioClip sound_HandleClip;

    public AudioSource source_Effect;
    public AudioSource source_Loop;

    public float sound_Buffer = 0.9f;
    public bool sound_StopSoundPlayed = false;
    public bool sound_StartSoundPlayed = false;

    public enum Axis
    {
        Horizontal,
        Vertical,
    }

    public Axis axis;

    public override void Start()
    {
        base.Start();

        reset_InitRot = _targetTransform.localRotation;

        Submarine.Instance.onResetMecanisms += HandleOnCrash;
    }

    public override void Update()
    {
        lerp = Mathf.InverseLerp(rot_A.x, rot_B.x, _targetTransform.localRotation.x);

        if (reseting)
        {
            Reset_Update();
        }
        value = lerp * 2 - 1;

        base.Update();

        

        

    }

    public void PlaySound (AudioClip audioClip )
    {
        Debug.Log(audioClip.name);

        source_Effect.clip = audioClip;
        source_Effect.pitch = Mathf.Lerp(effect_MinPitch, effect_MaxPitch, lerp);
        source_Effect.Play();
    }

    #region interact
    public override void Interact_Start()
    {
        base.Interact_Start();

        Player.Instance.DisableMovement();

        if (lerpBody)
        {
            IKManager.Instance.SetTarget(IKParam.Type.Body, playerBody_Target);
        }

        PlaySound(sound_HandleClip);
    }

    public override void Interact_Update()
    {
        base.Interact_Update();

        UpdateInput();
    }

    public override void Interact_LateUpdate()
    {
        base.Interact_LateUpdate();

        if (lerpBody)
        {
            playerBody_Target.rotation = Quaternion.Lerp(playerBody_A.rotation, playerBody_B.rotation, lerp);
            playerBody_Target.position = Vector3.Lerp(playerBody_A.position, playerBody_B.position, lerp);
        }
    }

    public override void Interact_Exit()
    {
        base.Interact_Exit();

        Player.Instance.EnableMovements();

        PlaySound(sound_HandleClip);


        IKManager.Instance.StopAll();

        Invoke("Interact_ExitDelay", 0.01f);
    }
    void Interact_ExitDelay()
    {
        source_Loop.Stop();

    }
    #endregion

    #region input
    public string GetAxis()
    {
        return axis == Axis.Horizontal ? "Horizontal" : "Vertical";
    }

    void UpdateInput()
    {
        if (Input.GetAxis(GetAxis()) > -.1f
                &&
                Input.GetAxis(GetAxis()) < .1f
                )
        {

            if (!sound_StopSoundPlayed)
            {
                PlaySound(sound_StopClip);
                sound_StopSoundPlayed = true;
            }

            if (source_Loop.isPlaying)
                source_Loop.Stop();


            sound_StartSoundPlayed = false;
            return;
        }


        if (lerp >= sound_Buffer || lerp <= 1 - sound_Buffer)
        {
            if (source_Loop.isPlaying)
                source_Loop.Stop();

            if (!sound_StopSoundPlayed)
            {
                PlaySound(sound_StopClip);
                sound_StopSoundPlayed = true;
            }
        }
        else
        {
            sound_StopSoundPlayed = false;
        }

        if (!sound_StartSoundPlayed)
        {
            PlaySound(sound_StartClip);
            sound_StartSoundPlayed=true;
        }

        source_Loop.pitch = Mathf.Lerp(loop_MinPitch, loop_MaxPitch, lerp);


        if (!source_Loop.isPlaying)
            source_Loop.Play();

        if (Input.GetAxis(GetAxis()) < 0)
        {
            _targetTransform.localRotation = Quaternion.Lerp(_targetTransform.localRotation, rot_A, rotateSpeed * Time.deltaTime);
        }
        else
        {
            _targetTransform.localRotation = Quaternion.Lerp(_targetTransform.localRotation, rot_B, rotateSpeed  * Time.deltaTime);
        }

        /*Vector3 handPos = IKManager.Instance._animator.GetBoneTransform(HumanBodyBones.RightHand).position;
        Vector3 dir = handPos - test_targetTransform.position;
        test_targetTransform.up = Vector3.Lerp(test_targetTransform.up, dir , testRotSpeed * Time.deltaTime);*/
    }
    #endregion

    #region reset
    void HandleOnCrash()
    {
        Reset_Start();
    }

    public void Reset_Start()
    {
        if (interacting)
        {
            Interact_Exit();
        }

        reset_timer = 0f;

        reseting = true;

        source_Loop.Play();


        reset_LerpRot = _targetTransform.localRotation;
    }

    void Reset_Update()
    {
        reset_timer += Time.deltaTime;

        _targetTransform.localRotation = Quaternion.Lerp(reset_LerpRot, reset_InitRot, reset_timer/reset_duration);

        if ( reset_timer >= reset_duration)
        { 
            Reset_Exit();
        }
    }

    void Reset_Exit()
    {
        reseting = false;
        source_Loop.Stop();

    }
    #endregion

    public float GetValue()
    {
        return value * valueMultiplier;
    }

   


}
