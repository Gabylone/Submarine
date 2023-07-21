using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class IKManager : MonoBehaviour
{
    public Transform headLook;

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
        GetIKParam(IKParam.Type.Body).Update();
    }

    public void SetTarget(IKParam.Type type, Transform target)
    {
        GetIKParam(type).Start(target);
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
    }

    // tests
    private void OnAnimatorIK(int layerIndex)
    {
        _animator.SetLookAtWeight(GetIKParam(IKParam.Type.Head).weight);

        for (int index = 0; index < ikParams.Length-1; index++)
        {
            IKParam.Type type = (IKParam.Type)index;
            IKParam ikParam = GetIKParam(type);

            ikParam.Update();
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
