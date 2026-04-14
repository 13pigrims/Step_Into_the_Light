using Unity.VisualScripting;
using UnityEngine;

public class LevelRoot : MonoBehaviour
{
    public static LevelRoot Instance;
    // 管理器实例
    private ButtonManager buttonManager_root;
    private InputManager inputManager_root;
    public HistoryManager historyManager_root;
    // 影子管理器（挂在同一个 GameObject 上）
    private GridShadowManager _shadowManager;
    // 各类型状态
    private WorldState[] _worldStates;
    private CharacterState _characterState;
    private ObjectState[] _objectStates;
    private SyncWorldObjState[] _syncObjStates;
    private FinalState[] _finalStates;
    // 当前选中对象
    private BaseState _objSelectedState;
    public Camera MainCamera { get => Camera.main; }

    public static LevelRoot GetInstance()
    {
        if (Instance == null)
        {
            Debug.LogWarning("Level Root实例获取失败!");
            return null;
        }
        return Instance;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        // 获取影子管理器（与 LevelRoot 挂在同一个 GameObject 上）
        _shadowManager = GetComponent<GridShadowManager>();
        if (_shadowManager == null)
            Debug.LogWarning("LevelRoot 上未找到 GridShadowManager，影子系统将不可用。");

        Debug.Log($"MainCamera: {MainCamera}");
        buttonManager_root = new ButtonManager();
        inputManager_root = new InputManager(MainCamera);

        var allButtons = GetComponentsInChildren<BaseButton>();

        foreach (var button in allButtons)
        {
            button.Initialize(buttonManager_root);
        }
        Debug.Log($"初始化ButtonManager及所有BaseButton完成, ButtonManager: {buttonManager_root}, BaseButtons数量: {allButtons.Length}");

        // 获取所有状态组件
        _worldStates = FindObjectsByType<WorldState>(FindObjectsSortMode.None);
        _characterState = FindObjectsByType<CharacterState>(FindObjectsSortMode.None)[0];
        _objectStates = FindObjectsByType<ObjectState>(FindObjectsSortMode.None);
        _syncObjStates = FindObjectsByType<SyncWorldObjState>(FindObjectsSortMode.None);
        _finalStates = FindObjectsByType<FinalState>(FindObjectsSortMode.None);

        Debug.Log($"WorldStates数量: {_worldStates.Length}");
        Debug.Log($"CharacterState: {_characterState}");
        Debug.Log($"ObjectStates数量: {_objectStates.Length}");
        Debug.Log($"SyncWorldObjStates数量: {_syncObjStates.Length}");
        Debug.Log($"FinalStates数量: {_finalStates.Length}");

        // 注册 ButtonManager
        foreach (WorldState ws in _worldStates)
        {
            ws.Initialize(buttonManager_root);
        }
        _characterState.Initialize(buttonManager_root);
        foreach (ObjectState obj in _objectStates)
        {
            obj.Initialize(buttonManager_root);
        }
        foreach (SyncWorldObjState syncObj in _syncObjStates)
        {
            syncObj.Initialize(buttonManager_root);
        }
        foreach (FinalState fs in _finalStates)
        {
            fs.Initialize(buttonManager_root);
        }

        // 注册 InputManager 和 CharacterState
        _characterState.InitializeInput(inputManager_root, _characterState);
        foreach (ObjectState obj in _objectStates)
        {
            obj.InitializeInput(inputManager_root, _characterState);
        }
        foreach (SyncWorldObjState syncObj in _syncObjStates)
        {
            syncObj.InitializeInput(inputManager_root, _characterState);
        }

        inputManager_root.OnObjectSelected += HandleObjectSelected;
        CharacterState.OnGameOver += HandleGameOver;
        inputManager_root.OnPause += HandleGameOver;
    }

    private void Start()
    {
        historyManager_root = new HistoryManager(_worldStates, _characterState, _objectStates, _syncObjStates);
    }

    private void Update()
    {
        if (_objSelectedState != null)
        {
            if (_objSelectedState.IsMoving) return;

            Vector3 movement = inputManager_root.OnMove();

            if (movement != Vector3.zero)
            {
                if (_objSelectedState is CharacterState characterState)
                {
                    if (characterState.canMoveOn(movement))
                    {
                        characterState.Move(movement);
                        StartCoroutine(RecordAfterMove(characterState));
                    }
                }
                else if (_objSelectedState is ObjectState objState)
                {
                    if (objState.canMoveOn(movement))
                    {
                        objState.Move(movement);
                        StartCoroutine(RecordAfterMove(objState));
                    }
                }
                else if (_objSelectedState is SyncWorldObjState syncObj)
                {
                    if (syncObj.canMoveOn(movement))
                    {
                        syncObj.Move(movement);
                        StartCoroutine(RecordAfterMove(syncObj));
                    }
                }
            }
        }

        if (historyManager_root != null && historyManager_root.IsUndoPressed())
        {
            if (_objSelectedState != null && _objSelectedState.IsMoving) return;

            Debug.Log("撤销键被按下，执行撤销操作");
            historyManager_root.Undo();
        }
    }

    private System.Collections.IEnumerator RecordAfterMove(BaseState state)
    {
        yield return new WaitUntil(() => !state.IsMoving);
        if (historyManager_root != null)
            historyManager_root.RecordState();
    }

    private void HandleObjectSelected(BaseState state)
    {
        _objSelectedState = state;
    }

    private void HandleGameOver()
    {
        GameRoot.GetInstance().UIManager_Root.PushPanel(new GameOverPanel());
    }

    public void UndoLastStep()
    {
        historyManager_root.Undo();
        _characterState.ResetGameOver();
    }

    private void OnDestroy()
    {
        historyManager_root.Dispose();
        inputManager_root.Dispose();
        inputManager_root.OnObjectSelected -= HandleObjectSelected;
        CharacterState.OnGameOver -= HandleGameOver;
    }
}