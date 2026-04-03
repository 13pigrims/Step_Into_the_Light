using UnityEngine;

public class ObjectSnapshot
{
    public int objectID;
    public ColorType.State color;
    public Vector3 position;

    // ¹¹Ôìº¯Êý
    public ObjectSnapshot(int id, ColorType.State color, Vector3 position)
    {
        this.objectID = id;
        this.color = color;
        this.position = position;
    }
}
