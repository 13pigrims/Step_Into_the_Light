using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControl
{
    public static SceneControl Instance;
    public Dictionary<string, SceneBase> dict_scene;
    /// <summary>
    /// Scene Control单例获取方法
    /// </summary>
    /// <returns></returns>
    public static SceneControl GetInstance()
    {
        // 初始化访问到的Instance
        if (Instance == null)
        {
            Debug.Log("UIManager实体不存在！");
            return Instance;
        }
        else
        {
            return Instance;
        }
    }
    /// <summary>
    /// 实例化SceneControl对象，初始化场景字典
    /// </summary>
    public SceneControl()
    {
        Instance = this;
        dict_scene = new Dictionary<string, SceneBase>();
    }
    /// <summary>
    /// 切换到指定的场景，必要时注册该场景，并处理场景的退出和进入逻辑。
    /// </summary>
    /// <param name="scene_name">The name of the scene to load.</param>
    /// <param name="sceneBase">The scene object associated with the scene to load.</param>
    public void LoadScene(string scene_name, SceneBase sceneBase)
    {
        if (!dict_scene.ContainsKey(scene_name))
        {
            dict_scene.Add(scene_name, sceneBase);
        }
        // 退出当前场景，进入新场景
        if (dict_scene.ContainsKey(SceneManager.GetActiveScene().name))
        {
            dict_scene[SceneManager.GetActiveScene().name].ExitScene();
        }
        else
        {
            Debug.Log("当前场景未注册！");
        }
        #region 弹出栈中当前所有元素
        GameRoot.GetInstance().UIManager_Root.PopPanel(true);
        #endregion
        SceneManager.LoadScene(scene_name);
        sceneBase.EnterScene();
    }
}
