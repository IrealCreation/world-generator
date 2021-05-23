
using UnityEngine;

public class Relief
{
    public string Name;
    public int MovementCost; // Additional movement cost of entering this hex
    public bool IsWater;
    public float ObjectY; // The shift in the Y axis of map objects on top of this relief
    public float SurfaceY; // The shift in the Y axis of surface part of this tile
    public float UndergroundY; // The shift in the Y axis of underground part of this tile
    public Mesh MeshSurface;
    public Mesh MeshUnderground;

    public Relief(string name, Mesh meshSurface, Mesh meshUnderground, int movementCost = 0, bool isWater = false, float objectY = 0f, 
        float surfaceY = 0f, float undergroundY = 0f)
    {
        Name = name;
        MeshSurface = meshSurface;
        MeshUnderground = meshUnderground;
        MovementCost = movementCost;
        IsWater = isWater;
        ObjectY = objectY;
        SurfaceY = surfaceY;
        UndergroundY = undergroundY;
    }
}