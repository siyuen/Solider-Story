using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerClickArgs : EventArgs
{
    private string UIName;

    public PlayerClickArgs(string name)
    {
        UIName = name;
    }

    public string GetUIName()
    {
        return UIName;
    }

}

