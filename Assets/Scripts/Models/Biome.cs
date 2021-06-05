
using System.Collections.Generic;
using UnityEngine;

public class Biome
{
    public string Name;
    public Yields Yields;
    public Material Material;

    public Biome(string name, Yields yields, Material material)
    {
        Name = name;
        Yields = yields;
        Material = material;
    }
}