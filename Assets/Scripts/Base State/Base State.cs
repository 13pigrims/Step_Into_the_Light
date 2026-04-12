using UnityEngine;
using System;
using System.Collections;

public enum MoveAxis { Horizontal, Vertical, All }

public abstract class BaseState : MonoBehaviour
{
    [Header("颜色设置")]
    [SerializeField] private ColorType.State initialColor;
    [SerializeField] private Material monochromeMaterial; // 新增，Inspector里拖入灰度材质
    public ColorType CurrentColor { get; private set; }

    [Header("移动设置")]
    [SerializeField] protected MoveAxis moveAxis;
    // 不再使用移动速度 [SerializeField] protected float moveSpeed = 5f;
    [Header("栅格移动")]
    [Tooltip("每格在世界空间中的大小")]
    [SerializeField] protected float gridCellSize = 1f;

    [Tooltip("移动动画时长（秒）")]
    [SerializeField] protected float moveDuration = 0.12f;

    [Header("碰撞设置")]
    [SerializeField] protected float checkDistance = 1f;

    [Tooltip("移动射线检测的Layer遮罩，应排除Button层（如Ignore Raycast层）")]
    [SerializeField] protected LayerMask movementBlockMask = ~0; // 默认检测所有层，需在Inspector中排除Button层

    protected Transform _currentTransform;

    protected InputManager _inputManager;
    protected CharacterState _characterState;
    protected ButtonManager _buttonManager;

    // 栅格化新增
    /// <summary>是否正在播放移动动画</summary>
    public bool IsMoving { get; protected set; }
    /// <summary>移动动画完成时触发</summary>
    public event Action OnMoveAnimComplete;

    // 倾听状态变化回调事件
    public static event Action OnStateChanged;
    /// <summary>外部访问当前颜色状态及当前Transform的函数 </summary>
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
            CurrentColor = new ColorType(renderer, initialColor, monochromeMaterial);
        else
            Debug.LogError($"{gameObject.name} 没有找到Renderer!");
        // 获取当前物体的Transform组件
        _currentTransform = transform;
    }
    // 初始化接收和订阅事件的函数
    public virtual void Initialize(ButtonManager buttonManager) { }
    // 初始化输入管理器和角色状态的函数
    public virtual void InitializeInput(InputManager inputManager, CharacterState characterState) { }
    // 处理不同状态下颜色切换的函数
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
    // 判断物体是否可以交互的函数
    public virtual bool IsInteractive() { return false; }
    // 判断是否可以继续前进的方法
    public virtual bool canMoveOn(Vector3 movement) { return false; }
    /// <summary>
    /// 实际上用于前进的函数，进行物体移动
    /// </summary>
    /// <param name="movement"></param>
    public virtual void Move(Vector3 movement)
    {
        if (IsMoving) return;

        // 根据轴限制过滤方向
        Vector3 filteredMovement = moveAxis switch
        {
            MoveAxis.Horizontal => new Vector3(movement.x, 0, 0),
            MoveAxis.Vertical => new Vector3(0, 0, movement.z),
            MoveAxis.All => movement,
            _ => Vector3.zero
        };

        if (filteredMovement == Vector3.zero) return;

        // 归一化为单位方向，确保只移动一格
        filteredMovement = NormalizeToCardinal(filteredMovement);

        Vector3 targetPos = _currentTransform.position + filteredMovement * gridCellSize;
        StartCoroutine(AnimateMove(targetPos));
    }

    /// <summary>
    /// 移动动画协程：从当前位置平滑插值滑到目标位置
    /// </summary>
    protected IEnumerator AnimateMove(Vector3 targetPos)
    {
        IsMoving = true;

        Vector3 startPos = _currentTransform.position;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            // SmoothStep 缓动
            t = t * t * (3f - 2f * t);
            _currentTransform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        _currentTransform.position = targetPos;
        _currentTransform = transform;
        IsMoving = false;
        OnMoveAnimComplete?.Invoke();
    }

    /// <summary>
    /// 将方向向量归一化为四方向的单位向量，每个分量只有 -1、0、1
    /// </summary>
    protected Vector3 NormalizeToCardinal(Vector3 dir)
    {
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.z))
            return new Vector3(Mathf.Sign(dir.x), 0, 0);
        else
            return new Vector3(0, 0, Mathf.Sign(dir.z));
    }

    /// <summary>
    /// 将当前位置对齐到最近的网格点（可在初始化或撤销时调用）
    /// </summary>
    public void SnapToGrid()
    {
        Vector3 pos = _currentTransform.position;
        Vector3 origin = Vector3.zero;

        // 运行时使用 GridShadowManager 的原点
        if (GridShadowManager.Instance != null)
            origin = GridShadowManager.Instance.gridOrigin;

        pos.x = Mathf.Round((pos.x - origin.x) / gridCellSize) * gridCellSize + origin.x;
        pos.z = Mathf.Round((pos.z - origin.z) / gridCellSize) * gridCellSize + origin.z;
        _currentTransform.position = pos;
    }

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
