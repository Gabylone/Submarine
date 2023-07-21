using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class Submarine : MonoBehaviour
{
    public static Submarine Instance;
    public enum Value
    {
        Speed,
        Turn,
        Dive,
        ForwardTilt,
        SidewaysTilt,

    }

    private Transform _transform;

    public float moveSpeed = 1f;
    public float turnSpeed = 5f;
    public float diveSpeed = 1f;

    public float tiltSpeed = 0f;
    public float sideTiltSpeed = 0f;

    public float maxTurnSpeed = 200f;
    public float maxMoveSpeed = 50f;
    public float maxDiveSpeed = 50f;
    public float maxTiltSpeed = 10f;

    public float turnAngle = 0f;
    public float currentTiltAngle = 0f;
    public float currentTiltSpeed = 0f;
    public float sideTiltAngle = 0f;


    public Wheel speedWheel;
    public Lever speedLever1;
    public Lever speedLever2;

    public Wheel turnWheel;
    //public Lever turnLever;

    //public Wheel diveWheel;
    public Lever diveLever;

    public Wheel tiltWheel;
    public Lever tiltLever;

    /*public Lever sideTiltLever;
    public Wheel sideTiltWheel;*/

    /*public Pully speedBreaks;
    public Pully turnBreaks;*/

    public ButtonTrigger stoppAll_Button;

    public delegate void OnCrash();
    public OnCrash onCrash;
    public float crash_duration = 1f;
    public bool crashing = false;

    public delegate void OnApproachCollision();
    public OnApproachCollision onApproachCollision;

    public delegate void OnExitCollision();
    public OnExitCollision onExitCollision;

    public delegate void OnReset();
    public OnReset onResetMecanisms;

    [System.Serializable]
    public class Sound
    {
        public Value value;
        public float minPitch = 0.8f;
        public float maxPitch = 1f;
        public float minVolume = 0.1f;
        public float maxVolume = 1f;
        public AnimationCurve curve;
        public AudioSource[] sources;
    }

    public AudioSource[] sources_Proximity;
    public AudioSource source_Crash;

    public float crash_Speed = -5f;

    public Sound[] sounds;

    public int proximityIndex = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        stoppAll_Button.onTrigger += HandleOnTriggerStopAllButton;
    }

    private void Update()
    {
        UpdateMovement();

    }

    private void HandleOnTriggerSpeedBreaks()
    {
        
    }

    private void HandleOnTriggerTurnBreaks()
    {
        
    }

    private void HandleOnTriggerStopAllButton()
    {
        Crash();
    }

    public float GetLerp(Value value)
    {
        return GetLerp(value, false);
    }
    public float GetLerp(Value value, bool sound)
    {
        float lerp = 0f;

        switch (value)
        {
            case Value.Speed:
                // 0 - 1
                if ( sound)
                {
                    float l = moveSpeed / maxMoveSpeed;
                    lerp = moveSpeed > 0 ? l : -l;
                }
                // -1 - 1
                else
                {
                    lerp = moveSpeed / maxMoveSpeed;
                }
                break;
            case Value.Turn:

                if (sound)
                {
                    float l = turnSpeed / maxTurnSpeed;
                    lerp = turnSpeed > 0 ? l : -l;
                }
                else
                {
                    lerp = turnSpeed / maxTurnSpeed;
                }
                break;
            case Value.Dive:
                if (sound)
                {
                    float l = diveSpeed / maxDiveSpeed;
                    lerp = diveSpeed > 0 ? l : -l;
                }
                else
                {
                    lerp = diveSpeed / maxDiveSpeed;
                }
                break;
            case Value.ForwardTilt:
                if (sound)
                {
                    float l = currentTiltSpeed / maxTiltSpeed;
                    lerp = currentTiltSpeed > 0 ? l : -l;
                }
                else
                {
                    lerp = currentTiltSpeed / maxTiltSpeed;
                }
                break;
            case Value.SidewaysTilt:
                //lerp = sideTiltSpeed / maxTiltSpeed;
                break;
            default:
                break;
        }

        return lerp;
    }

    public Vector3 eulerAngles_Test;

    private void UpdateMovement()
    {
       if (!crashing)
        {
            // move
            moveSpeed = (speedWheel.GetValue() + speedLever1.GetValue() + speedLever2.GetValue()) * maxMoveSpeed;
            /*Vector3 dir = GetTransform.forward;
            dir.y = 0f;
            dir = dir.normalized;*/
            GetTransform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

            // dive
            diveSpeed = diveLever.GetValue() * maxDiveSpeed;
            GetTransform.Translate(Vector3.up * diveSpeed * Time.deltaTime);

            currentTiltSpeed = (tiltLever.GetValue() + tiltWheel.GetValue()) * maxTiltSpeed;

            /*sideTiltSpeed = (sideTiltLever.GetValue() + sideTiltWheel.GetValue()) * maxTiltSpeed;
            sideTiltAngle += sideTiltSpeed * Time.deltaTime;*/

            turnSpeed = turnWheel.GetValue() * maxTurnSpeed;
            //turnAngle += turnSpeed * Time.deltaTime;

            //GetTransform.rotation = Quaternion.identity;
            //GetTransform.Rotate(Vector3.forward * sideTiltAngle);
            //GetTransform.Rotate(Vector3.right * currentTiltAngle);
            GetTransform.Rotate(Vector3.right * currentTiltSpeed * Time.deltaTime);
            GetTransform.Rotate(Vector3.up * turnSpeed * Time.deltaTime);

        }
        else
        {
            CrashUpdate();
        }

        UpdateSounds();
    }

    void CrashUpdate()
    {
        GetTransform.Translate(Vector3.forward * crash_Speed * Time.deltaTime);
    }

    void UpdateSounds()
    {
        foreach (var item in sounds)
        {
            float t = item.curve.Evaluate(GetLerp(item.value, true));

            foreach (var source in item.sources)
            {

                source.volume = Mathf.Lerp(item.minVolume, item.maxVolume, t);
                source.pitch = Mathf.Lerp(item.minPitch, item.maxPitch, t);
            }
            
        }
    }

    public void Crash()
    {
        Debug.Log("crash");

        if (onCrash != null)
        {
            onCrash();
        }

        ResetMecanisms();

        crashing = true;

        source_Crash.Play();

        Invoke("EndCrash", crash_duration);
    }

    public void ResetMecanisms()
    {
        if ( onResetMecanisms != null)
        {
            onResetMecanisms();
        }
    }

    public void EndCrash()
    {
        crashing = false;
        source_Crash.Stop();
    }

    public void ApproachCollision(int id)
    {
        proximityIndex = id;

        if ( id == 0)
        {
            if (onApproachCollision != null)
            {
                onApproachCollision();
            }
        }


        sources_Proximity[id].Play();


    }

    public void ExitCollision(int id)
    {
        proximityIndex = id;

        sources_Proximity[id].Stop();

        if (id == 0)
        {
            foreach (var item in sources_Proximity)
            {
                item.Stop();
            }

            if (onExitCollision != null)
            {
                onExitCollision();
            }
        }

        

    }

    public Transform GetTransform
    {
        get
        {
            if (_transform == null)
            {
                _transform = transform;
            }

            return _transform;
        }
    }
}
