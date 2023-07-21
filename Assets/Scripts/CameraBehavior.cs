using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    public static CameraBehavior Instance;

    private Transform _transform;
    public Transform tmpRot;
    public Vector3 decal;

    [Header("Params")]
    public float rotSpeed = 4f;
    public float moveSpeed = 4f;
    public Transform body_Transform;

    [Header("Rotate Around Point")]
    public bool rotateAroundPoint_Active = false;
    public Vector3 rotateAroundPoint_DecalToCenter;
    public float rotateAroundPoint_Distance = 5f;


    [Header("Look At Point")]
    public bool lookAtPoint_Active = false;
    public Transform lookAtPoint_Target;
    public Transform followPlayer_Target;


    [Header("Screen Shake")]
    public int screenShake_Count;
    public int screenShake_MaxCount = 20;
    public float screenShake_MaxAmount = 1f;
    public float screenShake_Rate = 0.05f;


    [Header("Zoom")]
    public bool zoom = false;
    public float zoom_Distance = 5f;
    public float zoomSpeed = 1f;
    public float zoomBuffer = 0.5f;
    public float zoomCurrent = 0f;
    public float distanceToPlayer = 0f;

    public Transform rotation_Ref;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if ( Submarine.Instance != null)
        {
            Submarine.Instance.onResetMecanisms += HandleOnResetMechanisms;
        }
    }

    private void HandleOnResetMechanisms()
    {
        ScreenShake_Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (zoom)
        {
            distanceToPlayer = Vector3.Distance(GetTransform.position, Player.Instance.GetTransform.position);

            Vector3 targetToCam = (GetTransform.position - CameraRoom.current.target.position).normalized;

            if (distanceToPlayer > zoom_Distance + zoomBuffer)
            {
                zoomCurrent += zoomSpeed * Time.deltaTime;
            }
            else if (distanceToPlayer < zoom_Distance - zoomBuffer)
            {
                zoomCurrent -= zoomSpeed * Time.deltaTime;
            }

            zoomCurrent = Mathf.Clamp(zoomCurrent, 0f, zoomCurrent);

        }
        else
        {
            zoomCurrent = Mathf.Lerp(zoomCurrent, 0f, zoomSpeed * Time.deltaTime);
        }
        UpdateRotation();

        // local position, so start at zero for sub movements
        //Vector3 dir = CameraRoom.current.target.TransformDirection(CameraRoom.current.zoom_Direction);
        Vector3 camToPlayer = (followPlayer_Target.position - CameraBehavior.Instance.GetTransform.position).normalized;
        //Vector3 dir = CameraRoom.current.target.TransformDirection(camToPlayer);
        Vector3 dir = camToPlayer;
        Vector3 targetPos = CameraRoom.current.target.position + dir * zoomCurrent;

        if (rotateAroundPoint_Active)
        {
            Vector3 centerToPlayer = Player.Instance.GetTransform.position - CameraRoom.current.rotateAroundPoint_Center.position;
            //centerToPlayer.y = 0f;
            targetPos = CameraRoom.current.rotateAroundPoint_Center.position - centerToPlayer.normalized * rotateAroundPoint_Distance;
            //targetPos.y = 0f;
            targetPos += rotation_Ref.TransformDirection(rotateAroundPoint_DecalToCenter);
        }
        GetTransform.position = Vector3.Lerp(GetTransform.position, targetPos, moveSpeed * Time.deltaTime);

    }

    void UpdateFollowPoint()
    {

    }

    void UpdateRotation()
    {
        if (lookAtPoint_Active)
        {
            GetTransform.rotation = Quaternion.Lerp(GetTransform.rotation, GetRotation(), rotSpeed * Time.deltaTime);
        }
        else
        {
            GetTransform.localRotation = Quaternion.Lerp(GetTransform.localRotation, Quaternion.identity, rotSpeed * Time.deltaTime);
        }
    }

    void UpdateRotateAroundPoint()
    {
        
    }

    void UpdateZoom()
    {
        
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

    public Quaternion GetRotation()
    {
        tmpRot.LookAt(lookAtPoint_Target.position, rotation_Ref.up);
        return tmpRot.rotation;
    }

    #region screen shake
    public void ScreenShake_Start()
    {
        screenShake_Count = 0;

        ScreenShake();
    }

    public void ScreenShake()
    {
        ++screenShake_Count;
        body_Transform.localPosition = Random.insideUnitSphere * screenShake_MaxAmount;

        if (screenShake_Count >= screenShake_MaxCount)
        {
            ScreenShake_Exit();
        }
        else
        {
            Invoke("ScreenShake", screenShake_Rate);
        }
    }

    public void ScreenShake_Exit()
    {
        body_Transform.localPosition = Vector3.zero;
    }
    #endregion
}
