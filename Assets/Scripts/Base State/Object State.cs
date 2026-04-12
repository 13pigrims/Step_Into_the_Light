using UnityEngine;
using System;

public class ObjectState : BaseState
{
    [SerializeField] public int objectID;

    protected override void Awake()
    {
        base.Awake();
        // 启动时初始化到网格
        SnapToGrid();
        // 开发状态下测试当前场景是否出现重复ID。发布时剔除
#if UNITY_EDITOR
        // 寻找全局的ObejctState
        var allObjects = FindObjectsByType<ObjectState>(FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            if (obj != this && obj.objectID == objectID)
            {
                Debug.LogError($"重复ID: {objectID}, 物体: {obj.name}");
            }
        }
#endif
    }

    public override void BackToPreviousState(GameStateSnapshot lastSnapshot)
    {
        // 根据ID找到对应的ObjectSnapshot
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
        Debug.Log(_characterState != null ? $"ObjectState {name} successfully linked to CharacterState {_characterState.name}" : $"ObjectState {name} failed to link to CharacterState");
    }

    public override bool IsInteractive()
    {
        bool isSameColor = CurrentColor.GetState() == _characterState.GetColor().GetState();
        return isSameColor;
    }


    public override bool canMoveOn(Vector3 movement)
    {
        // 【改动】动画播放中不允许再次移动
        if (IsMoving) return false;

        // 前方检测一格距离（使用 movementBlockMask 排除 Button 层）
        Vector3 dir = NormalizeToCardinal(movement);
        // 检查目标格子是否有影子 → 不能移动
        var shadowMgr = GridShadowManager.Instance;
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
        if (movement.sqrMagnitude > 0.01f)
        {
            GameRoot.GetInstance().AudioManager_Root.PlaySFX(GameRoot.GetInstance().MoveClip);
        }
        // 调用基类的栅格移动（带动画）
        base.Move(movement);
    }

}
