
using UnityEngine;

public class NewCameraController : MonoBehaviour
{
    class CameraState
    {
        public float yaw;
        public float pitch;
        public float roll;
        public float x;
        public float y;
        public float z;
        Transform transform;

    public void SetFromTransform(Transform t)
        {
            pitch = t.eulerAngles.x;
            yaw = t.eulerAngles.y;
            roll = t.eulerAngles.z;
            x = t.position.x;
            y = t.position.y;
            z = t.position.z;
            transform = t;
        }

        public void Translate(Vector3 translation)
        {
            Vector3 rotatedTranslation = Quaternion.Euler(0, yaw, 0) * translation;

            int layerMask = 1 << 9;
            RaycastHit hit;
            // Does the ray intersect any camera bounds object
            if (Physics.Raycast(transform.position, rotatedTranslation, out hit, 1f, layerMask))
            {
                Debug.DrawRay(transform.position, rotatedTranslation * hit.distance, Color.yellow);
            }
            else
            {
                Debug.DrawRay(transform.position, rotatedTranslation * 1000, Color.red);
                x += rotatedTranslation.x;
                y += rotatedTranslation.y;
                z += rotatedTranslation.z;
            }

        }

        public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
        {
            yaw = Mathf.SmoothStep(yaw, target.yaw, rotationLerpPct);
            pitch = Mathf.SmoothStep(pitch, target.pitch, rotationLerpPct);
            roll = Mathf.SmoothStep(roll, target.roll, rotationLerpPct);

            x = Mathf.SmoothStep(x, target.x, positionLerpPct);
            y = Mathf.SmoothStep(y, target.y, positionLerpPct);
            z = Mathf.SmoothStep(z, target.z, positionLerpPct);
        }

        public void UpdateTransform(Transform t)
        {
            t.eulerAngles = new Vector3(pitch, yaw, roll);
            t.position = new Vector3(x, y, z);
        }
    }

    CameraState m_TargetCameraState = new CameraState();
    CameraState m_InterpolatingCameraState = new CameraState();


    [Header("Movement Settings")]
    [Tooltip("Exponential boost factor on translation, controllable by mouse wheel.")]
    public float boost = 3.5f;

    [Tooltip("Time it takes to interpolate camera position 99% of the way to the target."), Range(0.001f, 1f)]
    public float positionLerpTime = 0.2f;

    [Header("Rotation Settings")]
    [Tooltip("X = Change in mouse position.\nY = Multiplicative factor for camera rotation.")]
    public AnimationCurve mouseSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

    [Tooltip("Time it takes to interpolate camera rotation 99% of the way to the target."), Range(0.001f, 1f)]
    public float rotationLerpTime = 0.01f;

    [Tooltip("Whether or not to invert our Y axis for mouse input to rotation.")]
    public bool invertY = false;

    private Vector3 moveToSelected;
    [SerializeField]
    private bool focusOnPlayer;


    void OnEnable()
    {
        m_TargetCameraState.SetFromTransform(transform);
        m_InterpolatingCameraState.SetFromTransform(transform);
        //GameManager.OnSelected += OnSelect;
    }

    Vector3 GetInputIsometricTranslationDirection()
    {
        Vector3 direction = Vector3.zero;
        //if (Input.GetKey(KeyCode.W) || cameraInputManager.CanCameraPanForward())
        if (Input.GetKey(KeyCode.W))
        {
            direction += Vector3.forward + Vector3.left;
        }
        //if (Input.GetKey(KeyCode.S) || cameraInputManager.CanCameraPanBackward())
        if (Input.GetKey(KeyCode.S))
        {
            direction -= Vector3.forward + Vector3.left;
        }
        //if (Input.GetKey(KeyCode.A) || cameraInputManager.CanCameraPanLeft())
        if (Input.GetKey(KeyCode.A))
        {
            direction -= Vector3.forward + Vector3.right;
        }
        //if (Input.GetKey(KeyCode.D) || cameraInputManager.CanCameraPanRight())
        if (Input.GetKey(KeyCode.D))
        {
            direction += Vector3.forward + Vector3.right;
        }
        return direction;
    }

    Vector3 GetInputTranslationDirection()
    {
        Vector3 direction = Vector3.zero;
        //if (Input.GetKey(KeyCode.W) || cameraInputManager.CanCameraPanForward())
        if (Input.GetKey(KeyCode.W))
        {
            direction += Vector3.forward;
        }
        //if (Input.GetKey(KeyCode.S) || cameraInputManager.CanCameraPanBackward())
        if (Input.GetKey(KeyCode.S))
        {
            direction += Vector3.back;
        }
        //if (Input.GetKey(KeyCode.A) || cameraInputManager.CanCameraPanLeft())
        if (Input.GetKey(KeyCode.A))
        {
            direction += Vector3.left;
        }
        //if (Input.GetKey(KeyCode.D) || cameraInputManager.CanCameraPanRight())
        if (Input.GetKey(KeyCode.D))
        {
            direction += Vector3.right;
        }
        //if (Input.GetKey(KeyCode.Q) || cameraInputManager.CameraZoomAmount() < 0)
        if (Input.GetKey(KeyCode.Q))
        {
            direction += Vector3.down;
        }
        //if (Input.GetKey(KeyCode.E) || cameraInputManager.CameraZoomAmount() > 0)
        if (Input.GetKey(KeyCode.E))
        {
            direction += Vector3.up;
        }
        return direction;
    }


    void Update()
    {
        // Exit Sample  

        if (IsEscapePressed())
        {
            Application.Quit();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

        // Unlock and show cursor when right mouse button released
        if (IsRightMouseButtonUp())
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // Rotation
        if (IsCameraRotationAllowed())
        {
            var mouseMovement = GetInputLookRotation() * Time.deltaTime * 5;
            if (invertY)
                mouseMovement.y = -mouseMovement.y;

            var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);

            m_TargetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
            m_TargetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;
        }

        // Translation
        Vector3 translation = GetTranslationFromInput();
        m_TargetCameraState.Translate(translation);


        // Framerate-independent interpolation
        // Calculate the lerp amount, such that we get 99% of the way to our target in the specified time
        var positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);
        var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);
        m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);
        if(!focusOnPlayer)
        {
            m_InterpolatingCameraState.UpdateTransform(transform);
        }
    }

    public void OnSelect(PlayerController player)
    {
        moveToSelected = player.transform.position;
        moveToSelected.y = transform.position.y;

        float amt = (moveToSelected - transform.position).magnitude;
        moveToSelected = (moveToSelected - transform.position).normalized;
        focusOnPlayer = true;
        transform.position = moveToSelected * amt*.45f + transform.position;
        transform.LookAt(player.transform);
        m_TargetCameraState.SetFromTransform(transform);
        m_InterpolatingCameraState.SetFromTransform(transform);
        focusOnPlayer = false;
    }

    private void OnDestroy()
    {
        GameManager.OnSelected -= OnSelect;
    }

    private Vector3 GetTranslationFromInput()
    {
        var translation = GetInputIsometricTranslationDirection() * Time.deltaTime;

        // Speed up movement when shift key held
        if (IsBoostPressed())
        {
            translation *= 10.0f;
        }

        // Modify movement by a boost factor (defined in Inspector and modified in play mode through the mouse scroll wheel)
        //boost += GetBoostFactor();
        translation *= Mathf.Pow(2.0f, boost);
        return translation;
    }

    float GetBoostFactor()
    {
        return Input.mouseScrollDelta.y * 0.2f;
    }

    Vector2 GetInputLookRotation()
    {
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * 10;
    }

    bool IsBoostPressed()
    {
        return Input.GetKey(KeyCode.LeftShift);
    }

    bool IsEscapePressed()
    {
        return Input.GetKey(KeyCode.Escape);
    }

    bool IsCameraRotationAllowed()
    {
        return Input.GetMouseButton(1);
    }

    bool IsRightMouseButtonDown()
    {
        return Input.GetMouseButtonDown(1);
    }

    bool IsRightMouseButtonUp()
    {
        return Input.GetMouseButtonUp(1);
    }
}
