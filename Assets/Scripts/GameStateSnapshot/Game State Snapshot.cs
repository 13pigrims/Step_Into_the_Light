using UnityEngine;

public class GameStateSnapshot
{
    // 多个世界状态的快照
    public WorldSnapshot[] worldSnapshots;

    // 角色颜色和位置
    public ColorType.State characterColor;
    public Vector3 characterPosition;

    // 所有物体的状态快照
    public ObjectSnapshot[] objectSnapshots;

    // 所有同步世界物体的状态快照
    public SyncObjSnapshot[] syncObjSnapshots;

    public GameStateSnapshot(WorldState[] worldStates, CharacterState characterState, ObjectState[] objectStates, SyncWorldObjState[] syncObjStates)
    {
        // 记录所有 WorldState
        worldSnapshots = new WorldSnapshot[worldStates.Length];
        for (int i = 0; i < worldStates.Length; i++)
        {
            worldSnapshots[i] = new WorldSnapshot(
                worldStates[i].worldID,
                worldStates[i].GetColor().GetState()
            );
        }

        // 记录角色
        characterColor = characterState.GetColor().GetState();
        characterPosition = characterState.GetTransform().position;

        // 记录所有物体
        objectSnapshots = new ObjectSnapshot[objectStates.Length];
        for (int i = 0; i < objectStates.Length; i++)
        {
            objectSnapshots[i] = new ObjectSnapshot(
                objectStates[i].objectID,
                objectStates[i].GetColor().GetState(),
                objectStates[i].GetTransform().position
            );
        }

        // 记录所有同步世界物体
        syncObjSnapshots = new SyncObjSnapshot[syncObjStates.Length];
        for (int i = 0; i < syncObjStates.Length; i++)
        {
            syncObjSnapshots[i] = new SyncObjSnapshot(
                syncObjStates[i].syncObjID,
                syncObjStates[i].GetColor().GetState(),
                syncObjStates[i].GetTransform().position
            );
        }
    }
}