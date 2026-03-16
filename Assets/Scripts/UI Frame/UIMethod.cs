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
            return Instance;
        }
        else
        {
            return Instance;
        }
    }
    /// <summary>
    /// 寻找场景中的Canvas物体（注：可能会遇到多个Canvas情况，需根据优先级进行判断，此处代码待优化）
    /// </summary>
    /// <returns></returns>
    public GameObject FindCanvas()
    {
        GameObject canvas_obj = GameObject.FindAnyObjectByType<Canvas>().gameObject;
        if (canvas_obj != null)
        {
            Debug.LogError("未在场景中找到Canvas物体");
           return canvas_obj;
        }
        return canvas_obj;
         
    }
    /// <summary>
    /// 寻找Panel物体下的子物体
    /// </summary>
    /// <param name="parent_Panel"></param>
    /// <param name="child_name"></param>
    /// <returns></returns>
    public GameObject FindObjectInChild(GameObject parent_Panel, string child_name)
    {
        Transform[] transforms = parent_Panel.GetComponentsInChildren<Transform>();
        foreach (var t in transforms)
        {
            if (t.name == child_name)
            {
                return t.gameObject;
            }
        }
        Debug.LogError("未在Panel中找到子物体");
        return null;
    }

    public GameObject AddOrGetComponent()
    {
        return null;
    }

    public GameObject GetOrAddSingleComponentInChild()
    {
        return null;
    }

}

