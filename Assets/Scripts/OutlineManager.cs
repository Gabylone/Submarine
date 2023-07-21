using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineManager : MonoBehaviour
{
    public EdgeDetect edgeDetect;

    public static OutlineManager Instance;

    public float outlineSpeed = 1f;
    public Color outlineColor = Color.red;

    private void Awake()
    {
        Instance = this;
    }

    public void SetOutline(GameObject _go)
    {
        foreach (var item in _go.GetComponentsInChildren<Transform>())
        {
            item.gameObject.layer = 3;
        }

        //edgeDetect.outlineColor = Color.clear;
    }
}
