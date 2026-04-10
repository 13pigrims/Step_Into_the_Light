using UnityEngine;
using UnityEngine.EventSystems;

public class GameRoot : MonoBehaviour
{
    public static GameRoot Instance;
    public EventSystem EventSystem { get => UnityEngine.EventSystems.EventSystem.current; }
    public static Camera MainCamera { get => Camera.main; }

    private UIManager UIManager;
    public UIManager UIManager_Root { get => UIManager; }
    private SceneControl SceneControl;
    public SceneControl SceneControl_Root { get => SceneControl; }
    private AudioManager _audioManager;
    public AudioManager AudioManager_Root => _audioManager;

    [Header("Audio")]
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip moveClip;
    public AudioClip ClickClip => clickClip;
    public AudioClip MoveClip => moveClip;

    public static GameRoot GetInstance()
    {
        if (Instance == null)
        {
            Debug.LogWarning("Game Root实例化失败!");
            return Instance;
        }
        return Instance;
    }

    private void Awake()
    {
        // 确保GameRoot在场景中是唯一的实例，如果已经存在则销毁当前对象，否则将当前对象设置为实例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(MainCamera);
        // 初始化BGM和SFX的AudioSource组件，并将它们设置为GameRoot的子对象，以便在场景切换时保持持久性
        _audioManager = new AudioManager(gameObject);
        if (bgmClip != null)
            _audioManager.PlayBGM(bgmClip);
        // 构造各类管理器
        UIManager = new UIManager();
        SceneControl = new SceneControl();
        _audioManager = new AudioManager(gameObject);
    }

    private void Start()
    {
        // 使GameRoot在场景切换时不会被销毁，保持其持久性,并且在游戏开始时找到当前场景中的Canvas和加载初始场景和UI
        DontDestroyOnLoad(this);
        DontDestroyOnLoad(EventSystem);
        UIManager_Root.CanvasObj = UIMethod.GetInstance().FindCanvas();
        // 初始化场景控制器，加载初始场景
        Scene1 scene1 = new Scene1();
        SceneControl_Root.dict_scene.Add(scene1.scene_name, scene1);
        #region StartScene加载
        UIManager_Root.PushPanel(new StartPanel());
        #endregion
    }

}
