using UnityEngine;
using System;

/// <summary>
/// 同步世界状态的可交互物体：
/// - 颜色受 ObeliskButton 控制（按下时切换颜色）
/// - 交互性与 ObjectState 一致（与角色同色时可交互）
/// - 可被玩家选中并移动
/// </summary>
public class SyncWorldObjState : BaseState
{
    [SerializeField] public int syncObjID;

    protected override void Awake()
    {
        base.Awake();
        SnapToGrid();

#if UNITY_EDITOR
        var allSyncObjs = FindObjectsByType<SyncWorldObjState>(FindObjectsSortMode.None);
        foreach (var obj in allSyncObjs)
        {
            if (obj != this && obj.syncObjID == syncObjID)
            {
                Debug.LogError($"SyncWorldObjState 重复ID: {syncObjID}, 对象: {obj.name}");
            }
        }
#endif
    }

    // ========== ObeliskButton 变色 ==========

    public override void Initialize(ButtonManager buttonManager)
    {
        _buttonManager = buttonManager;
        buttonManager.OnObeliskPressed += HandleButtonPressed;
        buttonManager.OnObeliskReleased += HandleButtonReleased;
    }

    public override void HandleButtonPressed()
    {
        ExchangeColor();
    }

    public override void HandleButtonReleased()
    {
        ExchangeColor();
    }

    // ========== 交互性（与 ObjectState 一致） ==========

    public override void InitializeInput(InputManager inputManager, CharacterState characterState)
    {
        _inputManager = inputManager;
        _characterState = characterState;
    }

    public override bool IsInteractive()
    {
        // 与角色同色时可交互
        bool isSameColor = CurrentColor.GetState() == _characterState.GetColor().GetState();
        return isSameColor;
    }

    // ========== 移动（与 ObjectState 一致） ==========

    public override bool canMoveOn(Vector3 movement)
    {
        if (IsMoving) return false;

        Vector3 dir = NormalizeToCardinal(movement);
        Vector3 targetPos = _currentTransform.position + dir * gridCellSize;

        // 目标格子没有地面 → Game Over
        // 目标格子没有地面 → 不允许移动（不触发 Game Over）
        if (!HasGroundAt(targetPos))
            return false;

        if (Physics.Raycast(_currentTransform.position, dir, out RaycastHit hit, gridCellSize * 0.9f, movementBlockMask))
        {
            if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Object") || hit.collider.CompareTag("Button"))
                return false;
        }
        return true;
    }

    public override void Move(Vector3 movement)
    {
        if (movement.sqrMagnitude > 0.01f)
        {
            GameRoot.GetInstance().AudioManager_Root.PlaySFX(GameRoot.GetInstance().MoveClip);
        }
        base.Move(movement);
    }

    // ========== 撤销 ==========

    public override void BackToPreviousState(GameStateSnapshot lastSnapshot)
    {
        foreach (var snap in lastSnapshot.syncObjSnapshots)
        {
            if (snap.syncObjID == syncObjID)
            {
                CurrentColor.SetState(snap.color);
                _currentTransform.position = snap.position;
                IsMoving = false;
                break;
            }
        }
    }

    protected override void OnDestroy()
    {
        if (_buttonManager != null)
        {
            _buttonManager.OnObeliskPressed -= HandleButtonPressed;
            _buttonManager.OnObeliskReleased -= HandleButtonReleased;
        }
    }
}