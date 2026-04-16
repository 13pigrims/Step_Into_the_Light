using UnityEngine;

public class Scene4 : SceneBase
{
    public readonly string scene_name = "Scene4";
    public override void EnterScene()
    {
        Debug.Log("进入Scene4");
    }
    public override void ExitScene()
    {
        Debug.Log("退出Scene4");
    }

}
