using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager
{
    // 单例模式
    public static InputManager Instance;
    // 输入系统
    private InputAction _moveAction;
    private InputAction _clickAction;
    // 用于向Level Root回调State的事件
    public event Action<BaseState> OnObjectSelected;
    /// <summary>
    /// 获取InputManager实例的方法，若实例不存在则输出警告
    /// </summary>
    /// <returns></returns>
    public static InputManager GetInstance()
    {
        // 初始化访问到的Instance
        if (Instance == null)
        {
            Debug.LogError("InputManager实体不存在！");
            return null;
        }
        else
        {
            return Instance;
        }
    }
    /// <summary>
    /// 构造函数，绑定输入事件并启用它们，同时设置回调事件以便在点击时获取State并传递给Level Root
    /// </summary>
    public InputManager()
    {
        Instance = this;
        // 绑定移动方法
        _moveAction = new InputAction("Move", binding: "<Keyboard>/wasd");
        _moveAction.Enable();
        _clickAction = new InputAction("Click", binding: "<Mouse>/leftButton");
        _clickAction.Enable();
        // 包含回调事件的方法
        _clickAction.performed += context =>
        {
            BaseState state = OnClick();
            // 将得到的state传到用以回调到Level Root的事件中
            OnObjectSelected?.Invoke(state);
        };
    }
    /// <summary>
    /// 处理点击事件的方法，发射射线检测点击位置的物体，如果是可交互的State则返回它，否则返回null
    /// </summary>
    /// <returns></returns>
    public BaseState OnClick()
    {
        // 从摄像机发射射线到鼠标位置
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            BaseState state = hit.collider.GetComponent<BaseState>();
            if (state != null && state.IsInteractive())
                return state;
        }
        return null; // 点到空白或不可交互物体，返回null
    }
    /// <summary>
    /// 实际处理移动输入的方法，读取输入值并转换为Vector3格式以供Level Root使用
    /// </summary>
    /// <returns></returns>
    public Vector3 OnMove()
    {
        Vector2 input = _moveAction.ReadValue<Vector2>();
        return new Vector3(input.x, 0, input.y);
    }
    /// <summary>
    /// 释放InputAction，在LevelRoot的OnDestroy里调用
    /// </summary>
    public void Dispose()
    {
        _moveAction.Disable();
        _clickAction.Disable();
    }
}
