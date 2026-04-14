using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class StartPanel : BasePanel
{   
    public static string name = "StartPanel";
    public static string path = "Panel/StartPanel";
    public static readonly UIType uIType = new UIType(name, path);

    public StartPanel() : base(uIType)
    {
    }

    public override void OnStart()
    {
        base.OnStart();
        // 在StartPanel的OnStart方法中添加特定于StartPanel的逻辑，例如，开始游戏/加载游戏/退出游戏按钮的点击事件监听等
        UIMethod.GetInstance().GetOrAddSingleComponentInChild<Button>(activeObj, "StartButton").onClick.AddListener(Start);
        UIMethod.GetInstance().GetOrAddSingleComponentInChild<Button>(activeObj, "ExitButton").onClick.AddListener(Exit);
        UIMethod.GetInstance().GetOrAddSingleComponentInChild<Button>(activeObj, "LoadButton").onClick.AddListener(Load);
    }
    public override void OnEnable()
    {
        base.OnEnable();
    }
    public override void OnDisable()
    {
        base.OnDisable();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    /// <summary>
    /// 触发Start按钮点击事件的方法
    /// </summary>
    private void Start()
    {
        Debug.Log("Start Button Clicked!");
        // 在这里添加点击Start按钮后的逻辑，例如，切换到游戏主界面等
        GameRoot.GetInstance().UIManager_Root.PopPanel(false);
        ChaptersPanel chaptersPanel = new ChaptersPanel();
        GameRoot.GetInstance().UIManager_Root.PushPanel(chaptersPanel);
    }
    /// <summary>
    /// 触发Exit按钮点击事件的方法
    /// </summary>
    private void Exit()
    {
        Debug.Log("Exit Button Clicked!");
        // 在这里添加点击Exit按钮后的逻辑，例如，弹出栈内所有元素并退出游戏等
        GameRoot.GetInstance().UIManager_Root.PopPanel(true);
        Debug.Log("游戏正在退出..."); // 用于调试

        #if UNITY_EDITOR
        // 如果在 Unity 编辑器中运行
        EditorApplication.isPlaying = false;
        #else
        // 如果在打包后的应用程序中运行
        Application.Quit();
        #endif
    }
    /// <summary>
    /// 触发Load按钮点击事件的方法
    /// </summary>
    private void Load()
    { 
        Debug.Log("Load Button Clicked!");
        // 在这里添加点击Load按钮后的逻辑，例如，加载场景等
        // 这里用于保存玩家进度和加载游戏
    }
}
