using UnityEngine;

public class Scene5 : SceneBase
{
    public readonly string scene_name = "Scene5";
    public override void EnterScene()
    {
        Debug.Log("进入Scene5");
    }
    public override void ExitScene()
    {
        Debug.Log("退出Scene5");
    }

}
