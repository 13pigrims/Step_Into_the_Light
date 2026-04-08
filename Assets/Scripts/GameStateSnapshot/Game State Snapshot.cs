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
        // Debug用
        Debug.Log($"characterState: {characterState}");
        Debug.Log($"characterState.GetColor().GetState(): {characterState?.GetColor().GetState()}");
        Debug.Log($"characterState.GetTransform().position: {characterState?.GetTransform().position}");
        Debug.Log($" objectStates[0]: {objectStates[0]}");
        Debug.Log($"worldState: {worldState}");
        Debug.Log($"objectStates[0].GetTransform().position: {(objectStates[0] != null ? objectStates[0].GetTransform().position.ToString() : "null")}");
        Debug.Log($"objectStates[0].GetColor().GetState(): {(objectStates[0] != null ? objectStates[0].GetColor().GetState().ToString() : "null")}");
        Debug.Log($"worldState.GetColor().GetState(): {worldState?.GetColor().GetState()}");

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
            objectStates[i].GetTransform().position);
        }
    }
}
