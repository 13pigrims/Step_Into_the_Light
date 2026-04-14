using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverPanel : BasePanel
{
    public static string name = "GameOverPanel";
    public static string path = "Panel/GameOverPanel";
    public static readonly UIType uIType = new UIType(name, path);

    public GameOverPanel() : base(uIType)
    {
    }

    public override void OnStart()
    {
        base.OnStart();
        // 在GameOverPanel的OnStart方法中添加特定于GameOverPanel的逻辑，例如，撤回/重开/回到主菜单的点击事件监听等
        UIMethod.GetInstance().GetOrAddSingleComponentInChild<Button>(activeObj, "UndoButton").onClick.AddListener(UndoLastStep);
        UIMethod.GetInstance().GetOrAddSingleComponentInChild<Button>(activeObj, "RestartButton").onClick.AddListener(Restart);
        UIMethod.GetInstance().GetOrAddSingleComponentInChild<Button>(activeObj, "BackToMenuButton").onClick.AddListener(BackToMenu);
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
    /// 触发Undo按钮点击事件的方法
    /// </summary>
    private void UndoLastStep()
    {
        Debug.Log("Undo Button Clicked!");
        Debug.Log($"GameRoot.Instance: {GameRoot.Instance}");

        if (GameRoot.Instance == null)
        {
            Debug.LogError("GameRoot是null，无法继续");
            return;
        }

        GameRoot.GetInstance().UIManager_Root.PopPanel(true);
        LevelRoot.GetInstance().UndoLastStep();
    }
    /// <summary>
    /// 触发Restart按钮点击事件的方法
    /// </summary>
    private void Restart()
    {
        Debug.Log("Restart Button Clicked!");
        GameRoot.GetInstance().UIManager_Root.PopPanel(true);
        // 直接重新加载当前场景，不管是第几关
        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }
    /// <summary>
    /// 触发Back To Menu按钮点击事件的方法
    /// </summary>
    private void BackToMenu()
    {
        Debug.Log("Back To Menu Button Clicked!");
        // 在这里添加点击Back To Menu按钮后的逻辑，例如，弹出栈内所有元素并返回主菜单等
        GameRoot.GetInstance().UIManager_Root.PopPanel(true);
        GameRoot.GetInstance().UIManager_Root.PushPanel(new StartPanel());
    }

}
