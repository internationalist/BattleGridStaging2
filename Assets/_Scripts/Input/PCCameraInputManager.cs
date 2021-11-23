using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(fileName = "PCCameraInputManager", menuName = "Input/PCCameraInputManager", order = 1)]
public class PCCameraInputManager : AbstractCameraInputManager
{
    public override float CameraPitchAmount(out bool isUserRotatingCam)
    {
        isUserRotatingCam = false;
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            isUserRotatingCam = true;
            return Input.GetAxis("Mouse Y");
        }
        else
        {
            return 0;
        }
    }

    public override float CameraYawAmount(out bool isUserRotatingCam)
    {
        isUserRotatingCam = false;
        if(Input.GetKey(KeyCode.LeftAlt))
        {
            isUserRotatingCam = true;
            return Input.GetAxis("Mouse X");
        } else
        {
            return 0;
        }
    }

    public override float CameraZoomAmount()
    {
        float zoomAmt = Input.mouseScrollDelta.y * 0.2f;
        return zoomAmt;
    }

    public override bool CanCameraPanBackward()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            return Input.mousePosition.y <= cameraPanOffset && Input.mousePosition.y >= 0;
        } else
        {
            return false;
        }
    }

    public override bool CanCameraPanForward()
    {
        if(!EventSystem.current.IsPointerOverGameObject())
        {
            return Input.mousePosition.y >= (Screen.height - cameraPanOffset) && Input.mousePosition.y <= Screen.height;
        } else
        {
            return false;
        }

    }

    public override bool CanCameraPanLeft()
    {
        return Input.mousePosition.x <= cameraPanOffset && Input.mousePosition.x >= 0;
    }

    public override bool CanCameraPanRight()
    {
        return Input.mousePosition.x >= (Screen.width - cameraPanOffset) && Input.mousePosition.x <= Screen.width;
    }
}
