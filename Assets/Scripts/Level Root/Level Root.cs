using Unity.VisualScripting;
using UnityEngine;

public class LevelRoot : MonoBehaviour
{
    public static LevelRoot Instance;
    // 各类管理器实例
    private ButtonManager buttonManager_root;
    private InputManager inputManager_root;
    public HistoryManager historyManager_root;
    // 新增影子管理器（挂在当前GameObject上）
    private GridShadowManager _shadowManager;
    // 各类物体状态
    private WorldState _worldState;
    private CharacterState _characterState;
    private ObjectState[] _objectStates;
    // 当前选中物体
    private BaseState _objSelectedState;
    public Camera MainCamera { get => Camera.main; }

    public static LevelRoot GetInstance()
    {
        if (Instance == null)
        {
            Debug.LogWarning("Level Root实例化失败!");
            return null;
        }
        return Instance;
    }

    private void Awake()
    {
        // 确保LevelRoot是单例
        if (Instance == null)
        {
            Instance = this;
        }

        // 获取影子管理器（与 LevelRoot 挂在同一个 GameObject 上）
        _shadowManager = GetComponent<GridShadowManager>();
        if (_shadowManager == null)
            Debug.LogWarning("LevelRoot 上未找到 GridShadowManager，影子系统将不可用。");

        // 测试能否获取主相机
        Debug.Log($"MainCamera: {MainCamera}");
        // 先实例化ButtonManager
        buttonManager_root = new ButtonManager();
        inputManager_root = new InputManager(MainCamera);

        // 获取场景中所有BaseButton子类
        var allButtons = GetComponentsInChildren<BaseButton>();

        // 将当前Level中的ButtonManager赋给所有BaseButton子类
        foreach (var button in allButtons)
        {
            button.Initialize(buttonManager_root);
        }
        Debug.Log($"初始化ButtonManager给所有BaseButton子类, ButtonManager: {buttonManager_root}, BaseButtons数量: {allButtons.Length}");

        // 获取场景中的MonoBehaviour组件
        _worldState = FindObjectsByType<WorldState>(FindObjectsSortMode.None)[0];
        _characterState = FindObjectsByType<CharacterState>(FindObjectsSortMode.None)[0];
        _objectStates = FindObjectsByType<ObjectState>(FindObjectsSortMode.None);

        Debug.Log($"WorldState: {_worldState}");
        Debug.Log($"CharacterState: {_characterState}");
        Debug.Log($"ObjectStates数量: {_objectStates.Length}");
        Debug.Log($"ObjectStates: {_objectStates[0]}");

        // 把ButtonManager注入给需要它的组件
        _worldState.Initialize(buttonManager_root);
        Debug.Log($"注入ButtonManager给CharacterState: {_characterState}, ButtonManager: {buttonManager_root}");
        _characterState.Initialize(buttonManager_root);
        foreach (ObjectState obj in _objectStates)
        {
            obj.Initialize(buttonManager_root);
        }
        Debug.Log($"注入ButtonManager给ObjectStates, ButtonManager: {buttonManager_root}");
        // 把InputManager注入给所需组件
        _characterState.InitializeInput(inputManager_root, _characterState);
        // Debug.Log($"注入CharacterState给CharacterState: {_characterState}, CharacterState: {_characterState}");
        foreach (ObjectState obj in _objectStates)
        {
           
            obj.InitializeInput(inputManager_root, _characterState);
        }
        // Debug.Log($"注入CharacterState给ObjectStates, CharacterState: {_characterState}");
        // 调用InputManager中的OnObjectSelected事件
        inputManager_root.OnObjectSelected += HandleObjectSelected;
        // 调用CharacterState中的OnGameOver事件
        CharacterState.OnGameOver += HandleGameOver;
        // 订阅InputManager的OnPause事件
        inputManager_root.OnPause += HandleGameOver;
        // Debug.Log($"订阅InputManager的OnObjectSelected事件, InputManager: {inputManager_root}");
        // 改为在移动动画结束时调用HistoryManager的RecordStep方法记录历史状态，先等HistoryManager构造完成再订阅事件
    }

    private void Start()
    {
        // 所有Awake都执行完之后再构造HistoryManager
        historyManager_root = new HistoryManager(_worldState, _characterState, _objectStates);
    }

    private void Update()
    {
        // 当被选择物体不为空时，检测移动
        if (_objSelectedState != null)
        {
            // 如果当前物体处在移动动画中，不进行更新
            if (_objSelectedState.IsMoving) return;

            Vector3 movement = inputManager_root.OnMove();
            // Debug.Log($"movement: {movement}");

            if (movement != Vector3.zero)
            {
                if (_objSelectedState is CharacterState characterState)
                {
                    // 角色：canMoveOn里判断影子，触发Game Over
                    if (characterState.canMoveOn(movement))
                    {
                        characterState.Move(movement);
                        // 动画结束后记录
                        StartCoroutine(RecordAfterMove(characterState));
                    }
                }
                else if (_objSelectedState is ObjectState objState)
                {
                    // 物体：只判断障碍物，碰到影子不移动但不Game Over
                    if (objState.canMoveOn(movement))
                    {
                        objState.Move(movement);
                        StartCoroutine(RecordAfterMove(objState));
                    }
                }
            }
        }
        // 检测是否按下撤回键
        if (historyManager_root != null && historyManager_root.IsUndoPressed())
        {
            // 动画播放时不允许撤回
            if (_objSelectedState != null && _objSelectedState.IsMoving) return;

            Debug.Log("撤回键被按下，执行撤回操作");
            historyManager_root.Undo();
        }
           
    }
    /// <summary>
    /// 等待移动动画完成后记录状态的协程
    /// </summary>
    private System.Collections.IEnumerator RecordAfterMove(BaseState state)
    {
        // 等待动画播放完毕
        yield return new WaitUntil(() => !state.IsMoving);
        // 动画结束，position 已到位，记录快照
        if (historyManager_root != null)
            historyManager_root.RecordState();
    }

    private void HandleObjectSelected(BaseState state)
    {
        _objSelectedState = state;
    }

    private void HandleGameOver()
    {
        // 通知GameRoot弹出Game Over Panel
        GameRoot.GetInstance().UIManager_Root.PushPanel(new GameOverPanel());
    }

    public void UndoLastStep()
    {
        Debug.Log($"GameRoot.Instance: {GameRoot.Instance}");
        Debug.Log($"LevelRoot.Instance: {LevelRoot.Instance}");
        historyManager_root.Undo();
        _characterState.ResetGameOver();
    }

    private void OnDestroy()
    {
        // 取消所有事件订阅
        historyManager_root.Dispose();
        inputManager_root.Dispose();
        inputManager_root.OnObjectSelected -= HandleObjectSelected;
        CharacterState.OnGameOver -= HandleGameOver;
    }
}
