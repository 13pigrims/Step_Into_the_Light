using UnityEngine;

public class Scene3 : SceneBase
{
    public readonly string scene_name = "Scene3";
    public override void EnterScene()
    {
        Debug.Log("进入Scene3");
    }
    public override void ExitScene()
    {
        Debug.Log("退出Scene3");
    }

}
