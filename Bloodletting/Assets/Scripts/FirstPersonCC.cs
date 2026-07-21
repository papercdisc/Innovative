using UnityEngine;
using UnityEngine.InputSystem;

// to-do:
// - movement that responds to player input
// - camera rotation that responds to mouse input (y axis independent of x, as body should rotate with x axis, but not y axis)
// - dash, jump, slide(?) functionality
// - juice (leaning, zoom fov at high speed, etc.)

public class FirstPersonCC : MonoBehaviour
{
    [Header("Mouse Sensitivity")]
    public float sensX_m;
    public float sensY_m;
    [Header("Gamepad Sensitivity")]
    public float sensX_gp;
    public float sensY_gp;
    [Header("References")]
    public Transform orientation;
    public Transform camHolder;

    public PlayerInputSubscription_FPS getInput;

    float xRot;
    float yRot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        float sensX = (getInput.currentDevice is Mouse) ? sensX_m : sensX_gp; // check for which sensitivity to use based on the current input device
        float lookX = getInput.LookInput.x * Time.deltaTime * sensX;

        float sensY = (getInput.currentDevice is Mouse) ? sensY_m : sensY_gp;
        float lookY = getInput.LookInput.y * Time.deltaTime * sensY;

        yRot += lookX;
        xRot -= lookY;
        xRot = Mathf.Clamp(xRot, -90f, 90f); // clamp the x rotation to prevent the camera from flipping upside down

        // rotate the camera and orientation
        camHolder.rotation = Quaternion.Euler(xRot, yRot, 0); // rotate camera
        orientation.rotation = Quaternion.Euler(0, yRot, 0); // rotate orientation (body) only on the y axis
    }
}
