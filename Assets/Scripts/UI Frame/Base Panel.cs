using System.Collections;
using System.Collections.Generic;
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
        Debug.Log("Obj is loaded!");
        // 判断当前UI物体是否有CanvasGroup组件，如果没有则添加一个
        if (activeObj.GetComponent<CanvasGroup>() == null)
        {
            activeObj.AddComponent<CanvasGroup>();
        }
    }
    public virtual void OnEnable()
    {
        UIMethod.GetInstance().AddOrGetComponent<CanvasGroup>(activeObj).interactable = true;
    }
    public virtual void OnDisable()
    {
        // 创建一个UIMethod实例，调用寻找对象CanvasGroup组件的interactable方法，控制当前面板是否可以交互
        UIMethod.GetInstance().AddOrGetComponent<CanvasGroup>(activeObj).interactable = false;
    }
    public virtual void OnDestroy()
    {
        UIMethod.GetInstance().AddOrGetComponent<CanvasGroup>(activeObj).interactable = false;
    }
}
