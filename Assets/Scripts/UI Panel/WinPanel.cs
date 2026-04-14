using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinPanel : BasePanel
{
    public static string name = "WinPanel";
    public static string path = "Panel/WinPanel";
    public static readonly UIType uIType = new UIType(name, path);

    public WinPanel() : base(uIType)
    {
    }

    public override void OnStart()
    {
        base.OnStart();
        // 下一关按钮
        UIMethod.GetInstance().GetOrAddSingleComponentInChild<Button>(activeObj, "NextLevelButton").onClick.AddListener(NextLevel);
        // 返回主菜单按钮
        UIMethod.GetInstance().GetOrAddSingleComponentInChild<Button>(activeObj, "BackToMenuButton").onClick.AddListener(BackToMenu);
    }

    /// <summary>
    /// 加载下一关场景
    /// 约定：场景按 Build Index 顺序排列，当前 +1 就是下一关
    /// </summary>
    private void NextLevel()
    {
        Debug.Log("Next Level Button Clicked!");
        GameRoot.GetInstance().UIManager_Root.PopPanel(true);
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;
        // 检查下一关是否存在
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.Log("已经是最后一关，返回主菜单");
            BackToMenu();
        }
    }

    private void BackToMenu()
    {
        Debug.Log("Back To Menu Button Clicked!");
        GameRoot.GetInstance().UIManager_Root.PopPanel(true);
        GameRoot.GetInstance().UIManager_Root.PushPanel(new StartPanel());
    }
}