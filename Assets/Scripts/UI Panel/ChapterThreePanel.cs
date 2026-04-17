using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ChapterThreePanel : BasePanel
{   
    public static string name = "ChapterThreePanel";
    public static string path = "Panel/ChapterThreePanel";
    public static readonly UIType uIType = new UIType(name, path);

    public ChapterThreePanel() : base(uIType)
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
        Scene12 scene12 = new Scene12();
        GameRoot.GetInstance().SceneControl_Root.LoadScene("scene12", scene12);
    }
    /// <summary>
    /// 触发进入第二关按钮点击事件的方法
    /// </summary>
    private void EnterLevelTwo()
    {
        Debug.Log("Enter Level Two Button Clicked!");
        GameRoot.GetInstance().UIManager_Root.PopPanel(false);
        Scene13 scene13 = new Scene13();
        GameRoot.GetInstance().SceneControl_Root.LoadScene("scene13", scene13);
    }
    /// <summary>
    /// 触发进入第三关按钮点击事件的方法
    /// </summary>
    private void EnterLevelThree()
    {
        /// 在这里添加点击进入第三关按钮后的逻辑，例如，切换到第三关的游戏界面等
        Debug.Log("Enter Level Three Button Clicked!");
        GameRoot.GetInstance().UIManager_Root.PopPanel(false);
        Scene14 scene14 = new Scene14();
        GameRoot.GetInstance().SceneControl_Root.LoadScene("scene14", scene14);
    }
    /// <summary>
    /// 触发进入第四关按钮点击事件的方法
    /// </summary>
    private void EnterLevelFour()
    {
        Debug.Log("Enter Level Four Button Clicked!");
        GameRoot.GetInstance().UIManager_Root.PopPanel(false);
        Scene15 scene15 = new Scene15();
        GameRoot.GetInstance().SceneControl_Root.LoadScene("scene15", scene15);
    }
    /// <summary>
    /// 触发进入第五关按钮点击事件的方法
    /// </summary>
    private void EnterLevelFive() 
    {
        Debug.Log("Enter Level Five Button Clicked!");
        GameRoot.GetInstance().UIManager_Root.PopPanel(false);
        Scene16 scene16 = new Scene16();
        GameRoot.GetInstance().SceneControl_Root.LoadScene("scene16", scene16);
    }

}
