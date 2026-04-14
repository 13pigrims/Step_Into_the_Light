using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ChapterShadowPanel : BasePanel
{   
    public static string name = "ChaptersPanel";
    public static string path = "Panel/ChapterShadowPanel";
    public static readonly UIType uIType = new UIType(name, path);

    public ChapterShadowPanel() : base(uIType)
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
        Scene2 scene2 = new Scene2();
        GameRoot.GetInstance().SceneControl_Root.LoadScene("scene2", scene2);
    }
    /// <summary>
    /// 触发进入第二关按钮点击事件的方法
    /// </summary>
    private void EnterLevelTwo()
    {
        Debug.Log("Enter Level One Button Clicked!");
        GameRoot.GetInstance().UIManager_Root.PopPanel(false);
        Scene3 scene3 = new Scene3();
        GameRoot.GetInstance().SceneControl_Root.LoadScene("scene3", scene3);
    }
    /// <summary>
    /// 触发进入第三关按钮点击事件的方法
    /// </summary>
    private void EnterLevelThree()
    {
        /// 在这里添加点击进入第三关按钮后的逻辑，例如，切换到第三关的游戏界面等
        Debug.Log("Enter Level One Button Clicked!");
        GameRoot.GetInstance().UIManager_Root.PopPanel(false);
        Scene4 scene4 = new Scene4();
        GameRoot.GetInstance().SceneControl_Root.LoadScene("scene4", scene4);
    }
    /// <summary>
    /// 触发进入第四关按钮点击事件的方法
    /// </summary>
    private void EnterLevelFour()
    {
        Debug.Log("Enter Level Four Button Clicked!");
        GameRoot.GetInstance().UIManager_Root.PopPanel(false);
        Scene5 scene5 = new Scene5();
        GameRoot.GetInstance().SceneControl_Root.LoadScene("scene5", scene5);
    }
    /// <summary>
    /// 触发进入第五关按钮点击事件的方法
    /// </summary>
    private void EnterLevelFive() 
    {
        Debug.Log("Enter Level Five Button Clicked!");
        GameRoot.GetInstance().UIManager_Root.PopPanel(false);
        Scene6 scene6 = new Scene6();
        GameRoot.GetInstance().SceneControl_Root.LoadScene("scene6", scene6);
    }

}
