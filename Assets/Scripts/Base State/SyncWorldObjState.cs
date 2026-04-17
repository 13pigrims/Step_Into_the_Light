using UnityEngine;
using System;

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

    // ========== 交互性 ==========

    public override void InitializeInput(InputManager inputManager, CharacterState characterState)
    {
        _inputManager = inputManager;
        _characterState = characterState;
    }

    public override bool IsInteractive()
    {
        bool isSameColor = CurrentColor.GetState() == _characterState.GetColor().GetState();
        return isSameColor;
    }

    // ========== 移动 ==========

    public override bool canMoveOn(Vector3 movement)
    {
        if (IsMoving) return false;

        Vector3 dir = NormalizeToCardinal(movement);
        Vector3 targetPos = _currentTransform.position + dir * gridCellSize;

        if (!HasGroundAt(targetPos))
            return false;

        // 检查目标格子是否有角色 → 不能推进角色身上
        if (_characterState != null)
        {
            float dist = Vector3.Distance(
                new Vector3(targetPos.x, 0, targetPos.z),
                new Vector3(_characterState.GetTransform().position.x, 0, _characterState.GetTransform().position.z)
            );
            if (dist < gridCellSize * 0.5f)
                return false;
        }

        if (Physics.Raycast(_currentTransform.position, dir, out RaycastHit hit, gridCellSize * 0.9f, movementBlockMask, QueryTriggerInteraction.Ignore))
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