using UnityEngine;

public class UIMethod
{
    public static UIMethod Instance;
    /// <summary>
    /// UIMethod自我实例化
    /// </summary>
    /// <returns></returns>
    public static UIMethod GetInstance()
    {
        // 初始化访问到的Instance
        if (Instance == null)
        {
            Instance = new UIMethod();
        }
        return Instance;
    }
    /// <summary>
    /// 寻找场景中的Canvas对象（注：可能会遇到多个Canvas情况，需根据优先级进行判断，此处代码待优化）
    /// </summary>
    /// <returns></returns>
    public GameObject FindCanvas()
    {
        GameObject canvas_obj = GameObject.FindAnyObjectByType<Canvas>().gameObject;
        if (canvas_obj == null)
        {
           Debug.LogError("未在场景中找到Canvas物体");
        }
        return canvas_obj;
         
    }
    /// <summary>
    /// 寻找Panel物体下的子对象
    /// </summary>
    /// <param name="parent_Panel"></param>
    /// <param name="child_name"></param>
    /// <returns></returns>
    public GameObject FindObjectInChild(GameObject parent_Panel, string child_name)
    {
        Transform[] transforms = parent_Panel.GetComponentsInChildren<Transform>();
        foreach (Transform t in transforms)
        {
            if (t.gameObject.name == child_name)
            {
                return t.gameObject;
            }
        }
        Debug.LogWarning("未在Panel中找到子物体");
        return null;
    }
    /// <summary>
    /// 在目标对象上寻找组件（注：缺少添加组件的功能，待优化）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Get_Obj"></param>
    /// <returns></returns>

    public T AddOrGetComponent<T>(GameObject Get_Obj) where T : Component
    {
        if (Get_Obj.GetComponent<T>() != null)
        {
            return Get_Obj.GetComponent<T>();
        }
        Debug.LogWarning("未在目标对象上找到组件");
        return null;
    }
    /// <summary>
    /// 获得目标对象上的子物体上的组件（注：可能会遇到多个同名子物体情况，需根据优先级进行判断，此处代码待优化），同时缺少添加组件的功能，待优化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="panel"></param>
    /// <param name="ComponentName"></param>
    /// <returns></returns>
    public T GetOrAddSingleComponentInChild<T>(GameObject panel, string ComponentName) where T : Component
    {
        Transform[] transforms = panel.GetComponentsInChildren<Transform>();
        foreach (var t in transforms)
        {
            if (t.gameObject.name == ComponentName)
            {
                if (t.GetComponent<T>() != null)
                {
                    return t.gameObject.GetComponent<T>();
                }
            }
        }
        Debug.LogWarning("未在Panel中找到子物体");
        return null;
    }

}

