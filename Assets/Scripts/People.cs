using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A people is the equivalent of an empire, or civilization, ruled by a player or by the computer,
/// existing over a territory, producing resources, making choices...
/// </summary>
public class People
{
    public readonly HexMap HexMap;
    public readonly string Name;
    public bool IsPlayer;

    private List<Hex> exploredHexes;

    public People(string name, bool isPlayer, HexMap hexMap)
    {
        Name = name;
        IsPlayer = isPlayer;
        HexMap = hexMap;

        //For now, all the hexes are explored
        exploredHexes = HexMap.GetAllHexes();
    }

    public bool HasExploredHex(Hex h)
    {
        return exploredHexes.Contains(h);
    }
}