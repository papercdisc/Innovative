using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// First person camera with mouse and gamepad support (as well as sensitivity settings for both).
/// ~~~
/// Current Features:
/// - Automatically detects current input device and switches sensitivity accordingly.
/// - L+R camera lean
/// ~~~
/// Features to add:
/// - toggle ability to look depending on state of the game (e.g. when in a menu, disable looking)
/// - Juice: increased FOV at high speeds, etc.
/// </summary>

public class FirstPersonCam : MonoBehaviour
{
    public static FirstPersonCam Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    [Header("Mouse Sensitivity")]
    public float sensX_m;
    public float sensY_m;
    [Header("Gamepad Sensitivity")]
    public float sensX_gp;
    public float sensY_gp;

    [Header("References")]
    public Transform orientation;
    public Transform camHolder;
    public GameObject playerObject;
    PlayerInputSubscription_FPS getInput;

    float xRot;
    float yRot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // populate references
        getInput = PlayerInputSubscription_FPS.Instance;

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

        orientation.rotation = Quaternion.Euler(0, yRot, 0); // rotate orientation (body) only on the y axis

        this.transform.rotation = Quaternion.Euler(xRot, yRot, 0); // rotate camera
        this.transform.position = camHolder.position; // keep the camera at the same position as the camera holder
        
        if (PlayerMovement3D.Instance != null) // check if the player movement script is present before trying to update the rotation
        {
            PlayerMovement3D.Instance.SetYaw(yRot); // update the rotation of the player based on camera rotation
        }

        //if (playerObject != null) // rotate player thing (NOT ACTUAL PLAYER JUST THE VISUAL REPRESENTATION) to match the orientation of the camera
        //{
        //    Quaternion objRot = playerObject.transform.rotation;
        //    Quaternion targetRot = orientation.rotation;

        //    playerObject.transform.rotation = Quaternion.Lerp(objRot, targetRot , Time.deltaTime * 10f); // rotate the player object to match the orientation
        //}
    }
}
