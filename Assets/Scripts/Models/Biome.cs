
using System.Collections.Generic;
using UnityEngine;

public class Biome
{
    public string Name;
    public float ExpansionModifier; // Modifier to the expansion desire towards this hex (no expansion possible if 0)
    public Yields Yields;
    public Material Material;

    public Biome(string name, Yields yields, Material material, float expansionModifier = 1)
    {
        Name = name;
        Yields = yields;
        Material = material;
        ExpansionModifier = expansionModifier;
    }
}