using UnityEngine;

public class StartPanel : BasePanel
{   
    public static string name = "StartPanel";
    public static string path = "Panel/StartPanel";
    public static readonly UIType uIType = new UIType(name, path);

    public StartPanel() : base(uIType)
    {
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public override void OnEnable()
    {
        base.OnEnable();
    }

    public override void OnStart()
    {
        base.OnStart();
    }
}
