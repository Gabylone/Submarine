using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class CameraTrigger : MonoBehaviour
{
    private CameraRoom cameraRoom;

    public bool cut = false;

    public List<Triggerable> triggerables = new List<Triggerable>();

    private void Start()
    {
        cameraRoom = GetComponentInParent<CameraRoom>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();

        if (player != null && !cameraRoom.active)
        {
            cameraRoom.Trigger(cut);

            if ( triggerables.Count > 0)
            {
                foreach (var item in triggerables)
                {
                    item.Trigger();
                }
            }
        }
    }

}
