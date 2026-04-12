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
        // 动画播放中不允许再次移动
        if (IsMoving) return false;
        // 新增网格管理器实例
        var shadowMgr = GridShadowManager.Instance;

        // 检查当前脚下是否站在影子上 → Game Over
        if (shadowMgr != null)
        {
            Vector2Int currentCell = shadowMgr.WorldToGrid(_currentTransform.position);
            if (shadowMgr.HasShadow(currentCell))
            {
                if (!_isGameOver)
                {
                    _isGameOver = true;
                    GameRoot.GetInstance().UIManager_Root.PushPanel(new GameOverPanel());
                }
                return false;
            }
        }

        // 检查目标格子是否有影子 → 不能走
        Vector3 dir = NormalizeToCardinal(movement);
        if (shadowMgr != null)
        {
            Vector3 targetWorldPos = _currentTransform.position + dir * gridCellSize;
            Vector2Int targetCell = shadowMgr.WorldToGrid(targetWorldPos);
            if (shadowMgr.HasShadow(targetCell))
                return false;
        }

        // 前方检测墙壁和物体（射线检测，排除 Button 层和 Shadow 层）
        if (Physics.Raycast(_currentTransform.position, dir, out RaycastHit hit, gridCellSize * 0.9f, movementBlockMask))
        {
            if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Object"))
                return false;
        }

        return true;
    }

    public override void Move(Vector3 movement)
    {
        // 播放移动音效，只有当角色实际移动时才播放
        if (movement.sqrMagnitude > 0.01f)
        {
            GameRoot.GetInstance().AudioManager_Root.PlaySFX(GameRoot.GetInstance().MoveClip);
        }
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
        return true; // 角色永远可交互
    }

    protected override void Awake()
    {
        base.Awake();
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
    /// <summary>
    /// 用于触发游戏结束面板
    /// </summary>
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
