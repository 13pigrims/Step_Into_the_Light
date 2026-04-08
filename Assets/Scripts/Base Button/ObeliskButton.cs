using UnityEngine;
using System;

public class ObeliskButton : BaseButton
{
    public override bool IsChangeStatePressed()
    {
        // 检测自身是否被影子Collider覆盖
        Collider[] colliders = Physics.OverlapBox(
           transform.position,
           transform.localScale / 2,
           transform.rotation);

        foreach (var col in colliders)
        {
            if (col.CompareTag("Shadow"))
            {  Debug.Log($"ObeliskButton {name} is covered by ShadowCollider {col.name}, state change triggered.");
                return true;
            }
        }
        return false;
    }

    protected override void NotifyStateChanged(bool isPressed)
    {
        base.NotifyStateChanged(isPressed);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void Update()
    {
        base.Update();
    }
}
