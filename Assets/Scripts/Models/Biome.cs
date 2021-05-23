
using System.Collections.Generic;
using UnityEngine;

public class Biome
{
    public string Name;
    public Dictionary<string, float> Resources;
    public Material Material;

    public Biome(string name, Dictionary<string, float> resources, Material material)
    {
        Name = name;
        Resources = resources;
        Material = material;
    }
}