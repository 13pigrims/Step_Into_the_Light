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
    // 栅格化输入
    // 上一帧输入值
    private Vector2 _lastInput;
    // 如有按键重复，延迟输入后移动
    private float _holdTimer;
    private bool _isHolding;
    private const float HOLD_DELAY = 0.3f;    // 按住多久后开始连续移动
    private const float HOLD_INTERVAL = 0.15f; // 连续移动的间隔
    private float _repeatTimer;

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
            return Instance;
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
            _holdTimer = 0f;
            _isHolding = false; 
            _repeatTimer = 0f;
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
            if (state != null)
                Debug.Log($"IsInteractive: {state.IsInteractive()}");
            if (state != null && state.IsInteractive())
                return state;
        }
        return null; // 点到空白或不可交互物体，返回null
    }
    /// <summary>
    /// 返回离散的移动方向，按下后返回一次方向，按住一段时间后开始重复
    /// 当返回Vector3.zero时表示没有输入或输入被延迟，Level Root在收到Vector3.zero时记录状态并不移动物体和角色
    /// </summary>
    /// <returns></returns>
    public Vector3 OnMove()
    {
        Vector2 input = _moveAction.ReadValue<Vector2>();
        //没有输入时
        if (input == Vector2.zero)
        {
            _lastInput = Vector2.zero;
            _holdTimer = 0f;
            _isHolding = false;
            _repeatTimer = 0f;
            return Vector3.zero;
        }
        // 量化输入值
        Vector2 discrete = QuantizeToCardinal(input);
        // 当上帧无输入，当前帧存在输入,且未长按时
        if (_lastInput == Vector2.zero)
        {
            _lastInput = discrete;
            _holdTimer = 0f;
            _isHolding = false;
            return new Vector3(-discrete.x, 0, -discrete.y);
        }

        // 当输入持续且长按时
        _holdTimer += Time.deltaTime;
        if (_holdTimer >= HOLD_DELAY)
        {
            _repeatTimer += Time.deltaTime;
            if (!_isHolding || _repeatTimer >= HOLD_INTERVAL)
            {
                _isHolding = true;
                _repeatTimer = 0f;
                _lastInput = discrete;
                return new Vector3(-discrete.x, 0, -discrete.y);
            }
        }

        _lastInput = discrete;
        return Vector3.zero;
    }
    /// <summary>
    /// 将输入量化为四个方向之一，只保留绝对值最大的轴
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private Vector2 QuantizeToCardinal(Vector2 input)
    { 
        if (Mathf.Abs(input.x) >= Mathf.Abs(input.y))
            return new Vector2(Mathf.Sign(input.x), 0);
        else
            return new Vector2(0, Mathf.Sign(input.y));
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
