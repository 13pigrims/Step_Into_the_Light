using UnityEngine;

public class Scene10 : SceneBase
{
    public readonly string scene_name = "Scene10";
    public override void EnterScene()
    {
        Debug.Log("进入Scene10");
    }
    public override void ExitScene()
    {
        Debug.Log("退出Scene10");
    }

}
