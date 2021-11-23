

 public interface ICameraInputManager
{
    bool CanCameraPanRight();
    bool CanCameraPanLeft();
    bool CanCameraPanForward();
    bool CanCameraPanBackward();
    float CameraYawAmount(out bool isUserRotatingCam);
    float CameraPitchAmount(out bool isUserRotatingCam);
    float CameraZoomAmount();
}

