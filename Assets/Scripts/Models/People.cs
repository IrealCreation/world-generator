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
    public Color Color;

    public Yields YieldsStock;
    public Dictionary<string, int> Interests;
    public Dictionary<string, int> Ethos;

    public HashSet<Hex> ExploredHexes { get; protected set; }
    public HashSet<Hex> SearchedHexes { get; protected set; } // Hexes already searched by a scout, giving resources
    public HashSet<Hex> Territory { get; private set; }
    public List<City> Cities; // Not sure we keep the cities, but I put it there in the meantime
    public List<Unit> Units;

    public People(string name, bool isPlayer, HexMap hexMap, Hex startingHex = null)
    {
        Name = name;
        IsPlayer = isPlayer;
        HexMap = hexMap;
        Territory = new HashSet<Hex>();
        Cities = new List<City>();
        Color = Color.red;

        SearchedHexes = new HashSet<Hex>();
        ExploredHexes = new HashSet<Hex>();
        if(startingHex == null)
            ExploredHexes = new HashSet<Hex>(HexMap.GetAllHexes());
        else
        {
            Explore(startingHex, 3);
        }
        
        YieldsStock = new Yields();
        
        // Instantiate the interests
        Interests = new Dictionary<string, int>();
        Interests["culture"] = 0; // "Inspiration"
        Interests["science"] = 0; // "Curiosity"
        Interests["military"] = 0; // "Bellicosity"
        Interests["wealth"] = 0; // "Industry"
        Interests["food"] = 0; // "Growth"
        
        // Instantiate the political ethos
        Ethos = new Dictionary<string, int>();
        Ethos["equality"] = 0;
        Ethos["elitism"] = 0;
        Ethos["liberty"] = 0;
        Ethos["authority"] = 0;
    }

    public bool HasExploredHex(Hex h)
    {
        return ExploredHexes.Contains(h);
    }

    public bool HasSearchedHex(Hex h)
    {
        return SearchedHexes.Contains(h);
    }

    public void Explore(Hex h, int range = 0)
    {
        if (range > 0)
            ExploredHexes.UnionWith(HexMap.GetHexesWithinRangeOf(h, range));
        else
            ExploredHexes.Add(h);
        HexMap.UpdateHexesVisuals();
    }

    public void SearchHex(Hex h)
    {
        SearchedHexes.Add(h);
        HexMap.UpdateHexVisuals(h);
        GainYields(h.Resource.Yields);
    }

    public void TerritoryAdd(Hex h)
    {
        if(Territory.Contains(h))
            Debug.Log("People "+ Name + " already owns Hex " + h);
        Territory.Add(h);
        HexMap.DrawTerritory(this);
    }

    public void GainYields(Yields yields)
    {
        YieldsStock += yields;
        HexMap.ScreenUIController.UpdateYieldsStock(YieldsStock);
    }

    public override string ToString()
    {
        return Name;
    }
}