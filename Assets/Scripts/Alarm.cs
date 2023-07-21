using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alarm : MonoBehaviour
{
    MeshRenderer rend;

    public bool on = false;

    public float crash_duration = 1f;

    public Light light;
    public GameObject light_Obj;
    public Transform light_Transform;

    public float proximity_duration = 3f;

    public float[] turnSpeeds;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<MeshRenderer>();

        Off();

        Submarine.Instance.onCrash += HandleOnCrash;
        Submarine.Instance.onApproachCollision += HandleOnApproachCollision;
        Submarine.Instance.onExitCollision += HandleOnExitCollision;
    }

    private void Update()
    {
        if (light_Obj.activeSelf)
        {
            light_Transform.Rotate(Vector3.right, turnSpeeds[Submarine.Instance.proximityIndex] * Time.deltaTime);
        }
    }

    void On()
    {
        rend.material.color = Color.red;
        rend.material.SetColor("_EmissionColor", Color.red);

        light_Obj.SetActive(true);

        CancelInvoke("HandleOnApproachCollision");
        Invoke("HandleOnApproachCollision", crash_duration);
    }

    void HandleOnApproachCollision()
    {
        rend.material.color = Color.yellow;
        rend.material.SetColor("_EmissionColor", Color.yellow);

        light_Obj.SetActive(true);
    }

    void HandleOnCrash()
    {
        On();
    }

    public void HandleOnExitCollision()
    {
        Off();
    }

    void Off()
    {
        on = false;

        rend.material.color = Color.yellow;
        rend.material.SetColor("_EmissionColor", Color.black);
        light_Obj.SetActive(false);

        //GetComponentInChildren<Light>().color = Color.yellow;
    }
}
