using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventHandler 
{
    public static event Action TransformPanel;
    public static void CallTransformPanel()
    {
        TransformPanel?.Invoke();
    }
    public static event Action StartNewGameAnimation;
    public static void CallStartNewGameAnimation()
    {
        StartNewGameAnimation?.Invoke();
    }
}
