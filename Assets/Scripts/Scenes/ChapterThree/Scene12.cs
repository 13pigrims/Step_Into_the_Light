using UnityEngine;

public class Scene12 : SceneBase
{
    public readonly string scene_name = "Scene12";
    public override void EnterScene()
    {
        Debug.Log("进入Scene12");
    }
    public override void ExitScene()
    {
        Debug.Log("退出Scene12");
    }

}
