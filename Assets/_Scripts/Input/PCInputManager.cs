using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCInputManager : AbstractInputManager
{
    private static PCInputManager _instance;

    private PCInputManager() { }

    public static PCInputManager Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new PCInputManager();
            }
            return _instance;
        }
    }


    public override bool MouseClick()
    {
        return Input.GetMouseButtonDown(0);
    }

    public override bool RightClickOnDestination()
    {
        return Input.GetMouseButtonDown(1);
    }

    public override Vector3 hoverPosition()
    {
        return Input.mousePosition;
    }


}
