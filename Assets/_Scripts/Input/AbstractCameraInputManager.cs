using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class AbstractCameraInputManager : ScriptableObject, ICameraInputManager
{
    [Header("Inscribed")]
    [Tooltip("The speed of camera move with mouse")]
    public float cameraMoveSpeed;
    [Tooltip("The offset from the edge of the screen that triggers the camera span")]
    public float cameraPanOffset;
    [Tooltip("The camera rotation speed")]
    public float cameraAngularSpeed;

    public abstract float CameraPitchAmount(out bool isUserRotatingCam);
    public abstract float CameraYawAmount(out bool isUserRotatingCam);
    public abstract float CameraZoomAmount();
    public abstract bool CanCameraPanBackward();
    public abstract bool CanCameraPanForward();
    public abstract bool CanCameraPanLeft();
    public abstract bool CanCameraPanRight();
}
