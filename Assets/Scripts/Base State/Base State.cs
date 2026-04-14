using UnityEngine;
using System;
using System.Collections;

public enum MoveAxis { Horizontal, Vertical, All }

public abstract class BaseState : MonoBehaviour
{
    [Header("颜色设置")]
    [SerializeField] private ColorType.State initialColor;
    [SerializeField] private Material monochromeMaterial;
    public ColorType CurrentColor { get; private set; }

    [Header("移动设置")]
    [SerializeField] protected MoveAxis moveAxis;
    public MoveAxis GetMoveAxis() { return moveAxis; }

    // 【改动】移除 moveSpeed，改用网格参数
    [Header("栅格移动")]
    [Tooltip("每格在世界空间中的大小")]
    [SerializeField] protected float gridCellSize = 1f;

    [Tooltip("移动动画时长（秒）")]
    [SerializeField] protected float moveDuration = 0.12f;

    [Header("碰撞检测")]
    [SerializeField] protected float checkDistance = 1f;

    [Tooltip("移动射线检测的Layer遮罩，应排除Button层，保留Shadow层")]
    [SerializeField] protected LayerMask movementBlockMask = ~0;

    [Tooltip("地面检测的Layer遮罩，用于防止移动到没有地板的位置")]
    [SerializeField] protected LayerMask groundCheckMask = ~0;

    protected Transform _currentTransform;

    protected InputManager _inputManager;
    protected CharacterState _characterState;
    protected ButtonManager _buttonManager;

    // ========== 栅格化新增 ==========
    /// <summary>是否正在播放移动动画</summary>
    public bool IsMoving { get; protected set; }

    /// <summary>移动动画完成时触发</summary>
    public event Action OnMoveAnimComplete;
    // ==========================================

    // 状态变化回调事件
    public static event Action OnStateChanged;

    public ColorType GetColor() { return CurrentColor; }
    public Transform GetTransform() { return _currentTransform; }

    protected virtual void Awake()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
            CurrentColor = new ColorType(renderer, initialColor, monochromeMaterial);
        else
            Debug.LogError($"{gameObject.name} 没有找到Renderer!");
        _currentTransform = transform;
    }

    public virtual void Initialize(ButtonManager buttonManager) { }
    public virtual void InitializeInput(InputManager inputManager, CharacterState characterState) { }
    public virtual void HandleButtonPressed() { }
    public virtual void HandleButtonReleased() { }

    public virtual void ExchangeColor()
    {
        CurrentColor.Exchange();
        NotifyStateChanged();
    }

    public virtual bool IsInteractive() { return false; }
    public virtual bool canMoveOn(Vector3 movement) { return false; }

    /// <summary>
    /// 检测目标格子下方是否有地面
    /// 所有子类的 canMoveOn 中调用，没有地面则不允许移动
    /// </summary>
    protected bool HasGroundAt(Vector3 targetPos)
    {
        // 从目标位置上方向下射线，检测是否有地面
        bool hasGround = Physics.Raycast(
            targetPos + Vector3.up * 5f,
            Vector3.down,
            10f,
            groundCheckMask
        );
        return hasGround;
    }

    /// <summary>
    /// 【改动】移动一格：movement 应为单位方向向量（如 (1,0,0)）
    /// 实际位移 = movement * gridCellSize，用协程做插值动画
    /// </summary>
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
    /// 移动动画协程：从当前位置平滑滑到目标位置
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
    /// 将方向向量归一化为四方向之一，每个分量只有 -1、0、1
    /// </summary>
    protected Vector3 NormalizeToCardinal(Vector3 dir)
    {
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.z))
            return new Vector3(Mathf.Sign(dir.x), 0, 0);
        else
            return new Vector3(0, 0, Mathf.Sign(dir.z));
    }

    /// <summary>
    /// 将当前位置对齐到最近的格子中心（不是网格线上，而是格子里面）
    /// </summary>
    public void SnapToGrid()
    {
        Vector3 pos = _currentTransform.position;
        Vector3 origin = Vector3.zero;

        if (GridShadowManager.Instance != null)
            origin = GridShadowManager.Instance.gridOrigin;

        // Floor 找到所在格子编号，+0.5 跳到格子中心
        pos.x = (Mathf.Floor((pos.x - origin.x) / gridCellSize) + 0.5f) * gridCellSize + origin.x;
        pos.z = (Mathf.Floor((pos.z - origin.z) / gridCellSize) + 0.5f) * gridCellSize + origin.z;
        _currentTransform.position = pos;
    }

    protected void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }

    protected virtual void OnDestroy() { }
    public virtual void BackToPreviousState(GameStateSnapshot lastSnapshot) { }
}