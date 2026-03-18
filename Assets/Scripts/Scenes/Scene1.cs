using UnityEngine;

public class Scene1 : SceneBase
{
    public readonly string scene_name = "Scene1";
    public override void EnterScene()
    {
        Debug.Log("进入Scene1");
    }
    public override void ExitScene()
    {
        Debug.Log("退出Scene1");
    }
}
