using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ChaptersPanel : BasePanel
{   
    public static string name = "ChaptersPanel";
    public static string path = "Panel/ChaptersPanel";
    public static readonly UIType uIType = new UIType(name, path);

    public ChaptersPanel() : base(uIType)
    {
    }

    public override void OnStart()
    {
        base.OnStart();
        // 在StartPanel的OnStart方法中添加特定于StartPanel的逻辑，例如，开始游戏/加载游戏/退出游戏按钮的点击事件监听等
        UIMethod.GetInstance().GetOrAddSingleComponentInChild<Button>(activeObj, "BackButton").onClick.AddListener(Back);
        UIMethod.GetInstance().GetOrAddSingleComponentInChild<Button>(activeObj, "ChapterOne").onClick.AddListener(EnterChapterOne);
        UIMethod.GetInstance().GetOrAddSingleComponentInChild<Button>(activeObj, "ChapterTwo").onClick.AddListener(EnterChapterTwo);
        UIMethod.GetInstance().GetOrAddSingleComponentInChild<Button>(activeObj, "ChapterThree").onClick.AddListener(EnterChapterThree);
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
    private void Back()
    {
        Debug.Log("Back Button Clicked!");
        // 在这里添加点击Back按钮后的逻辑，例如，返回到上一个界面等
        GameRoot.GetInstance().UIManager_Root.PopPanel(false);
        StartPanel startPanel = new StartPanel();
        GameRoot.GetInstance().UIManager_Root.PushPanel(startPanel);
    }
    /// <summary>
    /// 触发进入第一章按钮点击事件的方法
    /// </summary>
    private void EnterChapterOne()
    {
        Debug.Log("Enter Chapter One Button Clicked!");
        GameRoot.GetInstance().UIManager_Root.PopPanel(false);
        ChapterShadowPanel chapterShadowPanel = new ChapterShadowPanel();
        GameRoot.GetInstance().UIManager_Root.PushPanel(chapterShadowPanel);
        /// 在这里添加点击进入第一章按钮后的逻辑，例如，切换到第一章的游戏界面等

    }
    /// <summary>
    /// 触发第二章按钮点击事件的方法
    /// </summary>
    private void EnterChapterTwo()
    { 

    }
    /// <summary>
    /// 触发进入第三章按钮点击事件的方法
    /// </summary>
    private void EnterChapterThree()
    {
        /// 在这里添加点击进入第三章按钮后的逻辑，例如，切换到第三章的游戏界面等
    }

}
