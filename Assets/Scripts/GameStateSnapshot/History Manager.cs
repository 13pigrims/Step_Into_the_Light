using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HistoryManager
{
    public static HistoryManager Instance;
    private Stack<GameStateSnapshot> _historyStack;
    private WorldState _worldState;
    private CharacterState _characterState;
    private ObjectState[] _objectStates;

    /// <summary>
    /// 获取HistoryManager实例的静态方法，确保外部只能通过这个方法访问到Instance
    /// </summary>
    /// <returns></returns>
    public static HistoryManager GetInstance()
    {
        // 初始化访问到的Instance
        if (Instance == null)
        {
            Debug.Log("HistoryManager实体不存在！");
            return null;
        }
        else
        {
            return Instance;
        }
    }
    /// <summary>
    /// 构造函数，接受当前的世界状态、角色状态和物体状态数组，并将它们存储在私有字段中。同时，创建一个新的GameStateSnapshot对象，记录初始状态，并将其压入历史栈中。
    /// 最后，订阅BaseState中的事件，以便在状态发生变化时记录新的快照。
    /// </summary>
    /// <param name="worldState"></param>
    /// <param name="characterState"></param>
    /// <param name="objectStates"></param>
    public HistoryManager(WorldState worldState, CharacterState characterState, ObjectState[] objectStates)
    {
        Instance = this;
        _historyStack = new Stack<GameStateSnapshot>();
        _worldState = worldState;
        _characterState = characterState;
        _objectStates = objectStates;
        //Debug
        Debug.Log($"HistoryManager构造开始");
        // 往栈内压入初始状态
        GameStateSnapshot initialSnapshot = new GameStateSnapshot(_worldState, _characterState, _objectStates);
        Debug.Log($"Initial GameStateSnapshot created;");
        _historyStack.Push(initialSnapshot);
        // 订阅BaseState中的事件，一有状态变化，生成新的快照
        // BaseState.OnStateChanged += RecordState;
    }
    /// <summary>
    /// 当状态发生变化时，创建一个新的GameStateSnapshot对象，并将当前的世界状态、角色状态和物体状态传递给它。然后，将这个快照压入历史栈中，以便后续的撤销操作可以访问到这个状态。
    /// </summary>
    public void RecordState()
    {
        GameStateSnapshot snapshot = new GameStateSnapshot(_worldState, _characterState, _objectStates);
        _historyStack.Push(snapshot);
    }
    /// <summary>
    /// 判断有没有按下撤销键。如果按下了撤销键，返回true，否则返回false。这个方法可以在游戏的Update方法中调用，以便实时检测玩家是否想要撤销上一步操作。
    /// </summary>
    /// <returns></returns>
    public bool IsUndoPressed()
    {
        return Keyboard.current.zKey.wasPressedThisFrame;
    }
    /// <summary>
    /// 执行撤销操作。首先，检查历史栈中是否有足够的状态可以撤销（至少需要两个状态，一个是当前状态，一个是之前的状态）。如果满足条件，从历史栈中弹出当前状态，并将上一个状态应用到游戏世界中，以实现撤销效果。
    /// </summary>
    public void Undo()
    {
        // 确保栈内必须有初始状态，不得删除
        if (_historyStack.Count <= 1) return;

        _historyStack.Pop();
        ApplyStateToWorld(_historyStack.Peek());
    }
    /// <summary>
    /// 还原游戏世界到之前的状态。这个方法接受一个GameStateSnapshot对象作为参数，并将其中保存的状态应用到当前的游戏世界中。这包括恢复世界颜色、角色颜色和位置，以及所有物体的状态。通过调用每个状态对象的BackToPreviousState方法，将游戏世界还原到之前的状态。
    /// </summary>
    /// <param name="lastSnapshot"></param>
    public void ApplyStateToWorld(GameStateSnapshot lastSnapshot)
    {
        _worldState.BackToPreviousState(lastSnapshot);
        _characterState.BackToPreviousState(lastSnapshot);
        foreach (var obj in _objectStates)
        {
            obj.BackToPreviousState(lastSnapshot);
        }
    }
    /// <summary>
    /// 取消事件订阅，确保在HistoryManager被销毁时，不会继续监听BaseState的状态变化事件，从而避免潜在的内存泄漏或错误调用。
    /// </summary>
    public void Dispose()
    {
        BaseState.OnStateChanged -= RecordState;
    }
}
