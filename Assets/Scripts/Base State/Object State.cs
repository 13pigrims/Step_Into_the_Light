using UnityEngine;
using System;

public class ObjectState : BaseState
{
    [SerializeField] public int objectID;

    protected override void Awake()
    {
        base.Awake();
        // 启动时对齐到网格
        SnapToGrid();

#if UNITY_EDITOR
        var allObjects = FindObjectsByType<ObjectState>(FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            if (obj != this && obj.objectID == objectID)
            {
                Debug.LogError($"重复ID: {objectID}, 对象: {obj.name}");
            }
        }
#endif
    }

    public override void BackToPreviousState(GameStateSnapshot lastSnapshot)
    {
        foreach (var objSnapshot in lastSnapshot.objectSnapshots)
        {
            if (objSnapshot.objectID == objectID)
            {
                CurrentColor.SetState(objSnapshot.color);
                _currentTransform.position = objSnapshot.position;
                // 撤销后停止动画状态
                IsMoving = false;
                break;
            }
        }
    }

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

    public override bool canMoveOn(Vector3 movement)
    {
        if (IsMoving) return false;

        Vector3 dir = NormalizeToCardinal(movement);
        Vector3 targetPos = _currentTransform.position + dir * gridCellSize;

        // 目标格子没有地面 → Game Over
        // 目标格子没有地面 → 不允许移动（不触发 Game Over）
        if (!HasGroundAt(targetPos))
            return false;

        // 前方检测：墙壁、物体、按钮
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
        // 调用基类的栅格移动（带动画）
        base.Move(movement);
    }
}