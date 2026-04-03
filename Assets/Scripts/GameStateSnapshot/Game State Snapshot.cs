using UnityEngine;

public class GameStateSnapshot
{
    // 世界颜色
    public ColorType.State worldColor;

    // 角色颜色和位置
    public ColorType.State characterColor;
    public Vector3 characterPosition;

    // 所有物体的状态快照
    public ObjectSnapshot[] objectSnapshots;

    public GameStateSnapshot(WorldState worldState, CharacterState characterState, ObjectState[] objectStates)
    {
        worldColor = worldState.GetColor().GetState();
        characterColor = characterState.GetColor().GetState();
        characterPosition = characterState.GetTransform().position;
        // 根据objectStates内的个数顶objectSnapshots的长度
        objectSnapshots = new ObjectSnapshot[objectStates.Length];
        for (int i = 0; i < objectStates.Length; i++)
        {
            objectSnapshots[i] = new ObjectSnapshot(
            objectStates[i].objectID,
            objectStates[i].GetColor().GetState(),
            objectStates[i].GetTransform().position
          );
        }
    }
}
