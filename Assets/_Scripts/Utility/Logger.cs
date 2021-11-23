
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public static class Logger
{
    [Conditional("DEBUG")]
    public static void Trace(string className, string msg, params object[] args)
    {
        string log = className + "::";
        log += msg;
        UnityEngine.Debug.LogFormat(log, args);
    }

    public static void Trace(string className, string msg)
    {
        string log = className + "::";
        log += msg;
        UnityEngine.Debug.Log(log);
    }
}
