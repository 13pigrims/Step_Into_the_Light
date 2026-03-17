using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager
{
    public static UIManager Instance;
    public Dictionary<string, GameObject> dict_uiObject;
    public Stack<BasePanel> stack_ui;
    public GameObject CanvasObj;
    /// <summary>
    /// UIManager单例获取方法
    /// </summary>
    /// <returns></returns>
    public static UIManager GetInstance()
    {
        // 初始化访问到的Instance
        if (Instance == null)
        {
            Debug.Log("UIManager实体不存在！");
            return Instance;
        }
        else
        {
            return Instance;
        }
    }
    /// <summary>
    /// 实例化UIManager对象，初始化UI对象字典和UI栈
    /// </summary>
    public UIManager()
    {
        Instance = this;
        stack_ui = new Stack<BasePanel>();
        dict_uiObject = new Dictionary<string, GameObject>();
    }
    /// <summary>
    /// Panel入栈方法
    /// </summary>
    /// <param name="basePanel">The panel to be pushed onto the UI stack.</param>
    public void PushPanel(BasePanel basePanel)
    {
        // 清空当前栈中的所有元素
        if (stack_ui.Count > 0)
        {
            BasePanel top_panel = stack_ui.Peek();
            top_panel.OnDisable();
        }
        // 获取当前Panel对应的UI物体
        GameObject ui_obj = GetSingleObject(basePanel.uiType);
        dict_uiObject.Add(basePanel.uiType.Name, ui_obj);
        basePanel.activeObj = ui_obj;
        // 栈内没有元素则直接入栈，否则比较当前栈顶元素与即将入栈的元素是否相同，不相同则禁用下层Panel后入栈
        if (stack_ui.Count == 0)
        {
            stack_ui.Push(basePanel);
        }
        else
        {
            // 当前栈顶元素与即将入栈的元素不同，则入栈，否则不入栈
            if (stack_ui.Peek().uiType.Name != basePanel.uiType.Name)
            {
                stack_ui.Push(basePanel);
            }
        }
    }
    /// <summary>
    /// 获取当前Panel对应的UI物体，如果不存在则实例化一个新的对象并返回
    /// </summary>
    /// <param name="uIType"></param>
    /// <returns></returns>
    public GameObject GetSingleObject(UIType uIType) 
    {
        GameObject ui_obj;
        if (dict_uiObject.TryGetValue(uIType.Name, out ui_obj))
        {
            return ui_obj;
        }
        if (CanvasObj == null)
        {
            // 获取当前场景中的Canvas对象，赋值给CanvasObj变量
            CanvasObj = UIMethod.GetInstance().FindCanvas();
        }
        // 字典中没有对应的UI物体，找到本地UI物体，实例化一个新的对象并返回
        ui_obj = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(uIType.Path), CanvasObj.transform);
        return ui_obj;
    }
    /// <summary>
    /// Panel出栈方法，isLoad参数为true时表示弹出并销毁当前栈内所有元素，为false时表示弹出并销毁当前栈顶元素
    /// </summary>
    /// <param name="isLoad"></param>
    public void PopPanel(bool isLoad)
    {
        // 1. 弹出并销毁当前栈内所有元素
        if (isLoad ==  true)
        { 
           if (stack_ui.Count > 0)
            {
                // 禁用当前栈内所有元素及字典中对应的UI物体，弹出当前栈内所有元素
                stack_ui.Peek().OnDisable();
                stack_ui.Peek().OnDestroy();
                GameObject.Destroy(dict_uiObject[stack_ui.Peek().uiType.Name]);
                dict_uiObject.Remove(stack_ui.Peek().uiType.Name);
                stack_ui.Pop();
                PopPanel(true);
            }
        }
        // 2. 弹出并销毁当前栈顶元素
        if(isLoad == false)
        {
            if (stack_ui.Count > 0)
            {
                // 删除当前栈顶元素及字典中对应的UI物体，弹出当前栈顶元素
                stack_ui.Peek().OnDisable();
                stack_ui.Peek().OnDestroy();
                GameObject.Destroy(dict_uiObject[stack_ui.Peek().uiType.Name]);
                dict_uiObject.Remove(stack_ui.Peek().uiType.Name);
                stack_ui.Pop();
                if (stack_ui.Count > 0)
                {
                    stack_ui.Peek().OnEnable();
                }
            }
        }
    
    }
}
