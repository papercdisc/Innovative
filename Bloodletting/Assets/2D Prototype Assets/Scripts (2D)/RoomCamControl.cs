using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;
using Unity.Cinemachine;

public class RoomCamControl : MonoBehaviour
{
    // literally just like,....swap target when event tells it to. set zoom, too, but default is 5.77f.
    [SerializeField] List<CamTrigger> camTriggers;
    [SerializeField] CinemachineCamera cam;

    private void Start()
    {
        foreach (CamTrigger trigger in camTriggers)
        {
            trigger.onTriggerEnter.AddListener(SwitchTarget);
        }
    }

    void SwitchTarget(Transform target, float zoom)
    {
            cam.Follow = target;
            cam.Lens.OrthographicSize = zoom;
    }
}
