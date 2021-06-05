
using System;
using System.Collections.Generic;
using UnityEngine;

public class Resource
{
    public string Name;
    public Yields Yields;
    public GameObject Prefab;
    public int MovementCost; // Additional movement cost for units moving here
    public List<Biome> AllowedBiomes; // The biomes in which this feature can spawn
    public List<Relief> AllowedReliefs; // The reliefs in which this feature can spawn
    public Func<Hex, int> HexProbability;

    public Resource(string name, Yields yields, List<Biome> allowedBiomes, List<Relief> allowedReliefs, Func<Hex, int> hexProbability, GameObject prefab, int movementCost = 0)
    {
        Name = name;
        Yields = yields;
        Prefab = prefab;
        AllowedBiomes = allowedBiomes;
        AllowedReliefs = allowedReliefs;
        MovementCost = movementCost;
        HexProbability = hexProbability;
    }
}