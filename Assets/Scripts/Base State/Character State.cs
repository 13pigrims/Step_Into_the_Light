using UnityEngine;
using System;

public class CharacterState : BaseState
{
    // 游戏结束事件，供外部订阅
    public static event Action OnGameOver;
    private bool _isGameOver;

    public override void BackToPreviousState(GameStateSnapshot lastSnapshot)
    {
        CurrentColor.SetState(lastSnapshot.characterColor);
        _currentTransform.position = lastSnapshot.characterPosition;
        // 撤销后停止动画状态
        IsMoving = false;
    }

    public override bool canMoveOn(Vector3 movement)
    {
        if (IsMoving) return false;

        Vector3 dir = NormalizeToCardinal(movement);
        Vector3 targetPos = _currentTransform.position + dir * gridCellSize;

        // 目标格子没有地面 → Game Over
        if (!HasGroundAt(targetPos))
        {
            // 目标格子没有地面 → 不允许移动（不触发 Game Over）
            if (!HasGroundAt(targetPos))
                return false;
        }

        // 前方检测：只挡墙壁和物体，角色可以走进影子
        if (Physics.Raycast(_currentTransform.position, dir, out RaycastHit hit, gridCellSize * 0.9f, movementBlockMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Object"))
                return false;
        }

        return true;
    }

    /// <summary>
    /// 每帧检测脚下是否站在影子上，独立于输入
    /// 角色走进影子后立刻触发死亡，不需要再按键
    /// </summary>
    private void Update()
    {
        if (IsMoving || _isGameOver) return;

        // 从角色位置略上方向下发射射线，距离要足够到达地面
        Vector3 feetPos = _currentTransform.position + Vector3.up * 0.1f;
        float rayDistance = feetPos.y + 0.5f; // 确保射线能到达 Y=0 以下
        if (Physics.Raycast(feetPos, Vector3.down, out RaycastHit hit, rayDistance, movementBlockMask))
        {
            if (hit.collider.CompareTag("Shadow"))
            {
                _isGameOver = true;
                Debug.Log("Game Over: Stepped on a shadow!");
                GameRoot.GetInstance().UIManager_Root.PushPanel(new GameOverPanel());
            }
        }
    }

    public override void Move(Vector3 movement)
    {
        // 播放移动音效
        if (movement.sqrMagnitude > 0.01f)
        {
            GameRoot.GetInstance().AudioManager_Root.PlaySFX(GameRoot.GetInstance().MoveClip);
        }
        // 调用基类的栅格移动（带动画）
        base.Move(movement);
    }

    public override void Initialize(ButtonManager buttonManager)
    {
        _buttonManager = buttonManager;
        buttonManager.OnPedestalPressed += HandleButtonPressed;
        buttonManager.OnPedestalReleased += HandleButtonReleased;
    }

    public override void InitializeInput(InputManager inputManager, CharacterState characterState)
    {
        _inputManager = inputManager;
    }

    public override bool IsInteractive()
    {
        return true;
    }

    protected override void Awake()
    {
        base.Awake();
        // 启动时对齐到网格
        SnapToGrid();
    }

    public override void HandleButtonPressed()
    {
        ExchangeColor();
    }

    public override void HandleButtonReleased()
    {
        ExchangeColor();
    }

    private void TriggerGameOver()
    {
        OnGameOver?.Invoke();
    }

    public void ResetGameOver()
    {
        _isGameOver = false;
    }

    protected override void OnDestroy()
    {
        _buttonManager.OnPedestalPressed -= HandleButtonPressed;
        _buttonManager.OnPedestalReleased -= HandleButtonReleased;
    }
}