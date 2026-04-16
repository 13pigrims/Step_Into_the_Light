using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ChapterTwoPanel : BasePanel
{   
    public static string name = "ChapterTwoPanel";
    public static string path = "Panel/ChapterTwoPanel";
    public static readonly UIType uIType = new UIType(name, path);

    public ChapterTwoPanel() : base(uIType)
    {
    }

    public override void OnStart()
    {
        base.OnStart();
        // 在StartPanel的OnStart方法中添加特定于StartPanel的逻辑，例如，开始游戏/加载游戏/退出游戏按钮的点击事件监听等
        UIMethod.GetInstance().GetOrAddSingleComponentInChild<Button>(activeObj, "BackButton").onClick.AddListener(Back);
        UIMethod.GetInstance().GetOrAddSingleComponentInChild<Button>(activeObj, "LevelOne").onClick.AddListener(EnterLevelOne);
        UIMethod.GetInstance().GetOrAddSingleComponentInChild<Button>(activeObj, "LevelTwo").onClick.AddListener(EnterLevelTwo);
        UIMethod.GetInstance().GetOrAddSingleComponentInChild<Button>(activeObj, "LevelThree").onClick.AddListener(EnterLevelThree);
        UIMethod.GetInstance().GetOrAddSingleComponentInChild<Button>(activeObj, "LevelFour").onClick.AddListener(EnterLevelFour);
        UIMethod.GetInstance().GetOrAddSingleComponentInChild<Button>(activeObj, "LevelFive").onClick.AddListener(EnterLevelFive);
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
        ChaptersPanel chaptersPanel = new ChaptersPanel();
        GameRoot.GetInstance().UIManager_Root.PushPanel(chaptersPanel);
    }
    /// <summary>
    /// 触发进入第一章按钮点击事件的方法
    /// </summary>
    private void EnterLevelOne()
    {
        Debug.Log("Enter Level One Button Clicked!");
        GameRoot.GetInstance().UIManager_Root.PopPanel(false);
        Scene7 scene7 = new Scene7();
        GameRoot.GetInstance().SceneControl_Root.LoadScene("scene7", scene7);
    }
    /// <summary>
    /// 触发进入第二关按钮点击事件的方法
    /// </summary>
    private void EnterLevelTwo()
    {
        Debug.Log("Enter Level One Button Clicked!");
        GameRoot.GetInstance().UIManager_Root.PopPanel(false);
        Scene8 scene8 = new Scene8();
        GameRoot.GetInstance().SceneControl_Root.LoadScene("scene8", scene8);
    }
    /// <summary>
    /// 触发进入第三关按钮点击事件的方法
    /// </summary>
    private void EnterLevelThree()
    {
        /// 在这里添加点击进入第三关按钮后的逻辑，例如，切换到第三关的游戏界面等
        Debug.Log("Enter Level One Button Clicked!");
        GameRoot.GetInstance().UIManager_Root.PopPanel(false);
        Scene9 scene9 = new Scene9();
        GameRoot.GetInstance().SceneControl_Root.LoadScene("scene9", scene9);
    }
    /// <summary>
    /// 触发进入第四关按钮点击事件的方法
    /// </summary>
    private void EnterLevelFour()
    {
        Debug.Log("Enter Level Four Button Clicked!");
        GameRoot.GetInstance().UIManager_Root.PopPanel(false);
        Scene10 scene10 = new Scene10();
        GameRoot.GetInstance().SceneControl_Root.LoadScene("scene10", scene10);
    }
    /// <summary>
    /// 触发进入第五关按钮点击事件的方法
    /// </summary>
    private void EnterLevelFive() 
    {
        Debug.Log("Enter Level Five Button Clicked!");
        GameRoot.GetInstance().UIManager_Root.PopPanel(false);
        Scene11 scene11 = new Scene11();
        GameRoot.GetInstance().SceneControl_Root.LoadScene("scene11", scene11);
    }

}
