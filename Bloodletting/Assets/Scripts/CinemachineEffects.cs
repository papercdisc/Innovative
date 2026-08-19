using Unity.Cinemachine;
using UnityEngine;

public class CinemachineEffects : MonoBehaviour
{
    CinemachineCamera cam;
    PlayerInputSubscription_FPS getInput;

    [Header("Camera Settings")]
    public float baseFOV;
    public float fastFOV;
    public float dutchAngleMax;

    void Start()
    {
        cam = GetComponent<CinemachineCamera>();
        getInput = PlayerInputSubscription_FPS.Instance;
    }

    private void Update()
    {
        DutchAngleControl();
    }

    void DutchAngleControl()
    {
        if (getInput.MoveInput.x != 0)
        {
            float xInput = getInput.MoveInput.x;

            if(xInput > 0)
            {
                cam.Lens.Dutch = Mathf.Lerp(cam.Lens.Dutch, -dutchAngleMax, Time.deltaTime * 5f);
            }
            else if (xInput < 0)
            {
                cam.Lens.Dutch = Mathf.Lerp(cam.Lens.Dutch, dutchAngleMax, Time.deltaTime * 5f);
            }
        }
        else
        {
            if(cam.Lens.Dutch != 0) // if the camera is not at 0 dutch angle, lerp it back to 0
            {
                cam.Lens.Dutch = Mathf.Lerp(cam.Lens.Dutch, 0, Time.deltaTime * 1f);

                if (Mathf.Abs(cam.Lens.Dutch) < 0.1f) // set to 0 if ducth is low enough
                {
                    cam.Lens.Dutch = 0;
                }
            }
        }
    }

    void VerticalRotControl()
    {
        float targetFOV = baseFOV; // just base for now. 

        if (getInput.MoveInput.y != 0)
        {
            float yInput = getInput.MoveInput.y;
            if (yInput > 0)
            {
                cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, targetFOV + 5, Time.deltaTime * 2);
            }
            else if (yInput < 0)
            {
                cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, targetFOV - 5, Time.deltaTime * 2);
            }
        }
        else
        {
            if(cam.Lens.FieldOfView != targetFOV)
            {
                cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, targetFOV, Time.deltaTime * 1);

                if(Mathf.Abs(cam.Lens.FieldOfView) < 0.1f)
                {
                    cam.Lens.FieldOfView = targetFOV;
                }
            }
        }
    } // looks like ass unfortunately
}
