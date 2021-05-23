using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A structure built in one hex by a people. A hex can only have one structure at a time.
/// It is created based on a StructurePrototype, which contains the common infos of all identical structures
/// </summary>
public class Structure
{
    public readonly StructurePrototype Proto;
    
    private Hex hex;
    private People people;

    public Structure(StructurePrototype proto, Hex hex, bool isComplete = false)
    {
        //Instantiating the actual structure to place it on the map
        this.Proto = proto;
        this.hex = hex;
    }

    public void Turn()
    {
        //TODO: gain and consume resources...
    }

    public override string ToString()
    {
        return Proto.Name + " " + hex;
    }
}

public class StructurePrototype
{
    public readonly string Name;

    protected static Dictionary<string, StructurePrototype> protoDictionnary;
    public readonly string[] BuildableTerrain;
    public readonly Dictionary<string, float> ResourceGain;
    public readonly Dictionary<string, int> ResourceCost;
    
    public StructurePrototype(string name, string[] buildableTerrain, Dictionary<string, float> resourceGain, Dictionary<string, int> resourceCost)
    {
        //Let's check if the static dictionary of all StructurePrototypes is already instantiated
        if (protoDictionnary == null)
        {
            protoDictionnary = new Dictionary<string, StructurePrototype>();
        }
        
        //Instantiating the prototype version of the structure before it's true installation
        this.Name = name;
        this.BuildableTerrain = buildableTerrain;
        this.ResourceGain = resourceGain;
        this.ResourceCost = resourceCost;
        
        protoDictionnary[name] = this;
    }
    
    public static StructurePrototype GetProto(string name)
    {
        if (!protoDictionnary.ContainsKey(name) || protoDictionnary == null)
        {
            Debug.LogError("Structure prototype not found: " + name);
            return null;
        }
        
        return protoDictionnary[name];
    }
}
