using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(PlayerController), true)]
public class PlayerControllerEditor : Editor
{
    PlayerController thisController;
    Command currentCommand;


    public void OnEnable()
    {
        if (serializedObject.targetObject is PlayerController)
        {
            thisController = target as PlayerController;
        }
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        currentCommand = thisController.getCurrentCommand();
        if(currentCommand != null)
        {
            EditorGUILayout.EnumFlagsField("Current command", currentCommand.commandType);
        }
    }
}
