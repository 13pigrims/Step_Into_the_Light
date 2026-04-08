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
        }
        else
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(MainCamera);
        UIManager = new UIManager();
        SceneControl = new SceneControl();
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
