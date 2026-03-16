using UnityEngine;

public class BasePanel
{
    public GameObject activeObj; // UI Manager中设置
    public UIType uiType;
    // 构造函数，传入UIType参数
    public BasePanel(UIType uitype)
    {
        uiType = uitype;
    }
    /// <summary>
    /// 子类通用生命周期方法
    public virtual void OnStart()
    {
        
    }
    public virtual void OnEnable()
    {
       
    }
    public virtual void OnDisable()
    {

    }
    public virtual void OnDestroy()
    {

    }
}
