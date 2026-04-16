using UnityEngine;

public class Scene7 : SceneBase
{
    public readonly string scene_name = "Scene7";
    public override void EnterScene()
    {
        Debug.Log("进入Scene7");
    }
    public override void ExitScene()
    {
        Debug.Log("退出Scene7");
    }

}
