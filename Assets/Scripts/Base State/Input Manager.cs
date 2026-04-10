using UnityEngine;
using UnityEngine.InputSystem;
using System;
using UnityEngine.EventSystems;

public class InputManager
{
    // 单例模式
    public static InputManager Instance;
    // 输入系统
    private InputAction _moveAction;
    private InputAction _clickAction;
    public InputAction _pauseAction;
    public event Action OnMoveEnd;
    public event Action OnPause;
    // 用于向Level Root回调State的事件
    public event Action<BaseState> OnObjectSelected;
    public Camera _camera;
    public EventSystem EventSystem { get => UnityEngine.EventSystems.EventSystem.current; } 
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
    public InputManager(Camera camera)
    {
        _camera = camera;
        Instance = this;
        // 绑定移动方法
        _moveAction = new InputAction("Move", type: InputActionType.Value);
        _moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        _moveAction.Enable();
        _clickAction = new InputAction("Click", binding: "<Mouse>/leftButton");
        _clickAction.Enable();
        _pauseAction = new InputAction("Pause", binding: "<Keyboard>/escape");
        _pauseAction.Enable();
        // 包含回调事件的方法
        _clickAction.performed += context =>
        {
            BaseState state = OnClick();
            // 获得可交互物体，播放click音效
            if (state != null) 
            {
                var audio = GameRoot.GetInstance().AudioManager_Root;
                var clip = GameRoot.GetInstance().ClickClip;
                if (clip != null) audio.PlaySFX(clip);
            }
            // 将得到的state传到用以回调到Level Root的事件中
            OnObjectSelected?.Invoke(state);
        };
        // 按键松开时触发的事件，调用OnMoveEnd事件以通知Level Root记录状态
        _moveAction.canceled += context =>
        {
            // Move音效在BaseState的Move方法里播放，这里只负责通知Level Root记录状态
            OnMoveEnd?.Invoke();
        };
        // 暂停事件
        _pauseAction.performed += context =>
        {
            OnPause?.Invoke();
        };
    }
    /// <summary>
    /// 处理点击事件的方法，发射射线检测点击位置的物体，如果是可交互的State则返回它，否则返回null
    /// </summary>
    /// <returns></returns>
    public BaseState OnClick()
    {
        if (_camera == null)
        {
            Debug.LogError("Camera is null!");
            return null;
        }
        // 检查相机是否调用成功
        Debug.Log("Camera found: " + _camera.name);
        // 如果点击的是UI层，不处理
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return null;
        // 从摄像机发射射线到鼠标位置
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log($"射线命中: {hit.collider.gameObject.name}");
            // 先在当前物体找，找不到就往父物体找
            BaseState state = hit.collider.GetComponent<BaseState>();
            if (state == null)
                state = hit.collider.GetComponentInParent<BaseState>();

            Debug.Log($"BaseState: {state}");
            if (state != null)
                Debug.Log($"IsInteractive: {state.IsInteractive()}");
            if (state != null && state.IsInteractive())
                return state;
        }
        // Debug.LogError("No interactive object hit.");
        return null; // 点到空白或不可交互物体，返回null
    }
    /// <summary>
    /// 实际处理移动输入的方法，读取输入值并转换为Vector3格式以供Level Root使用
    /// </summary>
    /// <returns></returns>
    public Vector3 OnMove()
    {
        Vector2 input = _moveAction.ReadValue<Vector2>();
        return new Vector3(-input.x, 0, -input.y);
    }
    /// <summary>
    /// 释放InputAction，在LevelRoot的OnDestroy里调用
    /// </summary>
    public void Dispose()
    {
        _moveAction.Disable();
        _clickAction.Disable();
        _pauseAction.Disable();
    }
}
