using UnityEngine;

public class SyncObjSnapshot
{
    public int syncObjID;
    public ColorType.State color;
    public Vector3 position;

    public SyncObjSnapshot(int id, ColorType.State color, Vector3 position)
    {
        this.syncObjID = id;
        this.color = color;
        this.position = position;
    }
}