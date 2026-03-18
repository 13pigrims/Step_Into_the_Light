using UnityEngine;

public class Scene2 : SceneBase
{
    public readonly string scene_name = "Scene2";
    public override void EnterScene()
    {
        Debug.Log("进入Scene2");
    }
    public override void ExitScene()
    {
        Debug.Log("退出Scene2");
    }

}
