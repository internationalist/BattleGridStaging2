using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct UIMetaData
{

    public string commandName;
    public string commandImage;

    public UIMetaData(string commandName, string commandImage)
    {
        this.commandName = commandName;
        this.commandImage = commandImage;
    }
}
