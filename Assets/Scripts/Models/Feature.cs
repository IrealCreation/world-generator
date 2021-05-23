
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A terrain feature that adds resources but is destroyed by improvements
/// </summary>
public class Feature
{
    public string Name;
    public Dictionary<string, float> Resources;
    public GameObject Prefab;
    public int MovementCost; // Additional movement cost for units moving here
    public List<Biome> AllowedBiomes; // The biomes in which this feature can spawn

    public delegate int HexProbabilityDelegate(Hex h);

    public Feature(string name, Dictionary<string, float> resources, GameObject prefab, int movementCost = 0)
    {
        Name = name;
        Resources = resources;
        Prefab = prefab;
        MovementCost = movementCost;
    }
}