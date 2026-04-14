using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HistoryManager
{
    public static HistoryManager Instance;
    private Stack<GameStateSnapshot> _historyStack;
    private WorldState[] _worldStates;
    private CharacterState _characterState;
    private ObjectState[] _objectStates;
    private SyncWorldObjState[] _syncObjStates;

    public static HistoryManager GetInstance()
    {
        if (Instance == null)
        {
            Debug.Log("HistoryManager实例不存在！");
            return null;
        }
        return Instance;
    }

    public HistoryManager(WorldState[] worldStates, CharacterState characterState, ObjectState[] objectStates, SyncWorldObjState[] syncObjStates)
    {
        Instance = this;
        _historyStack = new Stack<GameStateSnapshot>();
        _worldStates = worldStates;
        _characterState = characterState;
        _objectStates = objectStates;
        _syncObjStates = syncObjStates;

        // 压入初始状态
        GameStateSnapshot initialSnapshot = new GameStateSnapshot(_worldStates, _characterState, _objectStates, _syncObjStates);
        _historyStack.Push(initialSnapshot);
    }

    public void RecordState()
    {
        GameStateSnapshot snapshot = new GameStateSnapshot(_worldStates, _characterState, _objectStates, _syncObjStates);
        _historyStack.Push(snapshot);
    }

    public bool IsUndoPressed()
    {
        return Keyboard.current.zKey.wasPressedThisFrame;
    }

    public void Undo()
    {
        if (_historyStack.Count <= 1) return;

        _historyStack.Pop();
        ApplyStateToWorld(_historyStack.Peek());
    }

    public void ApplyStateToWorld(GameStateSnapshot lastSnapshot)
    {
        // 还原所有 WorldState
        foreach (var ws in _worldStates)
        {
            ws.BackToPreviousState(lastSnapshot);
        }

        _characterState.BackToPreviousState(lastSnapshot);

        foreach (var obj in _objectStates)
        {
            obj.BackToPreviousState(lastSnapshot);
        }

        // 还原所有 SyncWorldObjState
        foreach (var syncObj in _syncObjStates)
        {
            syncObj.BackToPreviousState(lastSnapshot);
        }
    }

    public void Dispose()
    {
        BaseState.OnStateChanged -= RecordState;
    }
}