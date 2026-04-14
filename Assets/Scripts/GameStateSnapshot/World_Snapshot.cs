using UnityEngine;

public class WorldSnapshot
{
    public int worldID;
    public ColorType.State color;

    public WorldSnapshot(int id, ColorType.State color)
    {
        this.worldID = id;
        this.color = color;
    }
}
