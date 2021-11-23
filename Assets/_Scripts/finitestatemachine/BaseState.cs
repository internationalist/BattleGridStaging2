using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseState
{
    public abstract void EnterState(BaseFSMController controller);

    public abstract void Update(BaseFSMController controller);

    public virtual void ExitState(BaseFSMController controller)
    {

    }

}
