using UnityEngine;

public class LevelRoot : MonoBehaviour
{
    // 各类管理器实例
    private ButtonManager buttonManager_root;
    private InputManager inputManager_root;
    private HistoryManager historyManager_root;
    // 各类物体状态
    private WorldState _worldState;
    private CharacterState _characterState;
    private ObjectState[] _objectStates;
    // 当前选中物体
    private BaseState _objSelectedState;

    private void Awake()
    {
        // 先实例化ButtonManager
        buttonManager_root = new ButtonManager();
        inputManager_root = new InputManager();

        // 获取场景中所有BaseButton子类
        var allButtons = GetComponentsInChildren<BaseButton>();

        // 将当前Level中的ButtonManager赋给所有BaseButton子类
        foreach (var button in allButtons)
        {
            button.Initialize(buttonManager_root);
        }

        // 获取场景中的MonoBehaviour组件
        _worldState = GetComponentInChildren<WorldState>();
        _characterState = GetComponentInChildren<CharacterState>();
        _objectStates = GetComponentsInChildren<ObjectState>();

        // 实例化HistoryManager
        historyManager_root = new HistoryManager(_worldState, _characterState, _objectStates);

        // 把ButtonManager注入给需要它的组件
        _worldState.Initialize(buttonManager_root);
        _characterState.Initialize(buttonManager_root);
        foreach (var obj in _objectStates)
        {
            obj.Initialize(buttonManager_root);
        }
        // 把InputManager注入给所需组件
        _characterState.InitializeInput(inputManager_root, _characterState);
        foreach (var obj in _objectStates)
        {
            obj.InitializeInput(inputManager_root, _characterState);
        }
        // 调用InputManager中的OnObjectSelected事件
        inputManager_root.OnObjectSelected += HandleObjectSelected;
    }

    private void Update()
    {
        // 当被选择物体不为空时，检测移动
        if (_objSelectedState != null)
        {
            Vector3 movement = inputManager_root.OnMove();

            if (movement != Vector3.zero)
            {
                if (_objSelectedState is CharacterState characterState)
                {
                    // 角色：canMoveOn里判断影子，触发Game Over
                    if (characterState.canMoveOn(movement))
                    {
                        characterState.Move(movement);
                        historyManager_root.RecordState();
                    }
                }
                else if (_objSelectedState is ObjectState objState)
                {
                    // 物体：只判断障碍物，碰到影子不移动但不Game Over
                    if (objState.canMoveOn(movement))
                    {
                        objState.Move(movement);
                        historyManager_root.RecordState();
                    }
                }
            }
        }
        // 检测是否按下撤回键
        if (historyManager_root.IsUndoPressed())
            historyManager_root.Undo();
    }

    private void HandleObjectSelected(BaseState state)
    {
        _objSelectedState = state;
    }

    private void OnDestroy()
    {
        // 取消所有事件订阅
        historyManager_root.Dispose();
        inputManager_root.Dispose();
        inputManager_root.OnObjectSelected -= HandleObjectSelected;
    }
}
