using Unity.Cinemachine;
using UnityEngine;

public class CinemachineEffects : MonoBehaviour
{
    CinemachineCamera cam;
    PlayerInputSubscription_FPS getInput;

    [Header("Camera Settings")]
    public float baseFOV;
    public float zoomFOV;
    float targetFOV;
    public float zoomSpeed;

    public float dutchAngleMax;
    public bool enableDutch;

    void Start()
    {
        cam = GetComponent<CinemachineCamera>();
        getInput = PlayerInputSubscription_FPS.Instance;

        targetFOV = baseFOV;
        cam.Lens.FieldOfView = baseFOV;
    }

    private void Update()
    {
        if(enableDutch)
            DutchAngleControl();

        // t is the fraction of steps *covered*, not remaining, so: covered (t) = 1f - remaining (Mathf.Exp())
        float t = 1f - Mathf.Exp(-zoomSpeed * Time.unscaledDeltaTime); // basically converts the time steps into something framerate independent
                                                                       // gap between FOVs shrinks by the same proportion for the same amount of real time (dt)
                                                                       // -zoomSpeed to make it shrink towards 0
        cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, targetFOV, t);
    }

    public void FOVZoom(bool activeZoom)
    {
        targetFOV = activeZoom ? zoomFOV : baseFOV; 
    }

    void DutchAngleControl()
    {
        if (getInput.MoveInput.x != 0)
        {
            float xInput = getInput.MoveInput.x;

            if(xInput > 0)
            {
                cam.Lens.Dutch = Mathf.Lerp(cam.Lens.Dutch, -dutchAngleMax, Time.deltaTime * 2f);
            }
            else if (xInput < 0)
            {
                cam.Lens.Dutch = Mathf.Lerp(cam.Lens.Dutch, dutchAngleMax, Time.deltaTime * 2f);
            }
        }
        else
        {
            if(cam.Lens.Dutch != 0) // if the camera is not at 0 dutch angle, lerp it back to 0
            {
                cam.Lens.Dutch = Mathf.Lerp(cam.Lens.Dutch, 0, Time.deltaTime * 7f);

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
