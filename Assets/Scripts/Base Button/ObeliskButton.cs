using UnityEngine;
using System;

public class ObeliskButton : BaseButton
{
    protected override void NotifyStateChanged(bool isPressed)
    {
        base.NotifyStateChanged(isPressed);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
