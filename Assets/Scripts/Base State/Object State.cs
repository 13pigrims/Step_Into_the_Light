using UnityEngine;
using System;

public class ObjectState : BaseState
{
    [SerializeField] public int objectID;

    protected override void Awake()
    {
        base.Awake();
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
                break;
            }
        }
    }

    public override bool canMoveOn(Vector3 movement)
    {
        if (Physics.Raycast(_currentTransform.position, movement.normalized, out RaycastHit hit, checkDistance))
        {
            if (hit.collider.CompareTag("Shadow"))
                return false; // 碰到影子，不移动但不Game Over
            if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Object"))
                return false;
        }
        return true;
    }

    public override void InitializeInput(InputManager inputManager, CharacterState characterState)
    {
        _inputManager = inputManager;
        _characterState = characterState;
    }

    public override bool IsInteractive()
    {
        return CurrentColor.GetState() == _characterState.GetColor().GetState();
    }

    public override void Move(Vector3 movement)
    {
        Vector3 filteredMovement = moveAxis switch
        {
            MoveAxis.Horizontal => new Vector3(movement.x, 0, 0),
            MoveAxis.Vertical => new Vector3(0, 0, movement.z),
            MoveAxis.All => movement,
            _ => Vector3.zero
        };

        _currentTransform.position += filteredMovement * moveSpeed * Time.deltaTime;
        // 更新currentTransform
        _currentTransform = transform;
        // 当前位移发生变化时，回调给HistoryManager
        NotifyStateChanged();
    }

}
