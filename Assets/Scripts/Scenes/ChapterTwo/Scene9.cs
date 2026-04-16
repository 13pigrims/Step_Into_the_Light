using UnityEngine;

public class Scene9 : SceneBase
{
    public readonly string scene_name = "Scene9";
    public override void EnterScene()
    {
        Debug.Log("进入Scene9");
    }
    public override void ExitScene()
    {
        Debug.Log("退出Scene9");
    }

}
