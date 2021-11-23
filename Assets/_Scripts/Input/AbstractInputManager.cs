using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractInputManager
{
    public abstract Vector3 hoverPosition();
    public abstract bool ClickOnDestination();
    public abstract bool RightClickOnDestination();
}
