using UnityEngine;

public class Scene6 : SceneBase
{
    public readonly string scene_name = "Scene6";
    public override void EnterScene()
    {
        Debug.Log("进入Scene6");
    }
    public override void ExitScene()
    {
        Debug.Log("退出Scene6");
    }

}
