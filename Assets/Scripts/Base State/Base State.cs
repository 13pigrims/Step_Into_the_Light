using UnityEngine;
using System;

public enum MoveAxis { Horizontal, Vertical, All }

public abstract class BaseState : MonoBehaviour
{
    [Header("颜色设置")]
    [SerializeField] private ColorType.State initialColor;
    public ColorType CurrentColor { get; private set; }

    [Header("移动设置")]
    [SerializeField] protected MoveAxis moveAxis;
    [SerializeField] protected float moveSpeed = 5f;

    [Header("碰撞设置")]
    [SerializeField] protected float checkDistance = 1f;
    protected Transform _currentTransform;

    protected InputManager _inputManager;
    protected CharacterState _characterState;
    protected ButtonManager _buttonManager;

    // 倾听状态变化回调事件
    public static event Action OnStateChanged;
    /// <summary>
    /// 外部访问当前颜色状态及当前Transform的函数
    /// </summary>
    /// <returns></returns>
    public ColorType GetColor() { return CurrentColor; }
    public Transform GetTransform() { return _currentTransform; }
    /// <summary>
    /// 用于初始化的函数
    /// </summary>
    protected virtual void Awake()
    {
        // 获取物体脚本上的Render并设置
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
            CurrentColor = new ColorType(renderer, initialColor);
        else
            Debug.LogError($"{gameObject.name} 没有找到Renderer!");
        // 获取当前物体的Transform组件
        _currentTransform = transform;
        Debug.Log($"BaseState Awake: Initialized for {gameObject.name} with initial color {initialColor}");
        Debug.Log($"BaseState Awake: _currentTransform set to {(_currentTransform != null ? _currentTransform.name : "null")}");
    }
    /// <summary>
    /// 初始化接收和订阅事件的函数
    /// </summary>
    /// <param name="buttonManager"></param>
    public virtual void Initialize(ButtonManager buttonManager) { }
    /// <summary>
    /// 初始化输入管理器和角色状态的函数
    /// </summary>
    /// <param name="inputManager"></param>
    /// <param name="characterState"></param>
    public virtual void InitializeInput(InputManager inputManager, CharacterState characterState) { }
    /// <summary>
    /// 处理不同状态下颜色切换的函数
    /// </summary>
    public virtual void HandleButtonPressed() { }
    public virtual void HandleButtonReleased() { }
    /// <summary>
    /// 实际进行颜色切换的函数
    /// </summary>
    public virtual void ExchangeColor()
    {
        CurrentColor.Exchange();
        // 颜色变化后通知HistoryManager              
        NotifyStateChanged();
    }
    /// <summary>
    /// 判断物体是否可以交互的函数
    /// </summary>
    /// <returns></returns>
    public virtual bool IsInteractive() { return false; }
    /// <summary>
    /// 判断是否可以继续前进的方法
    /// </summary>
    /// <param name="movement"></param>
    /// <returns></returns>
    public virtual bool canMoveOn(Vector3 movement) { return false; }
    /// <summary>
    /// 实际上用于前进的函数，进行物体移动
    /// </summary>
    /// <param name="movement"></param>
    public virtual void Move(Vector3 movement) { }
    /// <summary>
    /// 用于回调给HistoryManager的方法，通知状态变化以便记录历史
    /// </summary>
    protected void NotifyStateChanged() 
    {
        OnStateChanged?.Invoke();
    }
    /// <summary>
    /// 用于销毁子类额外订阅的事件的方法
    /// </summary>
    protected virtual void OnDestroy() { }
    /// <summary>
    /// 用于状态回调的函数，接收上一个状态快照以便回退到之前的状态
    /// </summary>
    /// <param name="lastSnapshot"></param>
    public virtual void BackToPreviousState(GameStateSnapshot lastSnapshot) { }
}
