using System.Collections.Generic;

/// <summary>
/// A type of building (common or wonder) that can be constructed on a hex.
/// </summary>
public class Building
{
    public string Name;
    public int Cost;
    public Yields Yields;
    public List<Biome> RequiredBiomes;
    public List<Relief> RequiredRelief;
    public Choice Choice; // Choice presented when the building is first built by a people
    public bool IsWonder;
    public bool WonderBuilt = false; // Becomes true when someone has built this wonder

    public Building(string name, int cost, Yields yields, Choice choice = null, List<Biome> requiredBiomes = null, List<Relief> requiredRelief = null, bool isWonder = false)
    {
        Name = name;
        Cost = cost;
        Yields = yields;
        Choice = choice;
        RequiredBiomes = requiredBiomes;
        RequiredRelief = requiredRelief;
        IsWonder = isWonder;
    }

    public override string ToString()
    {
        return Name;
    }

    public bool CanBeBuiltOn(Hex hex)
    {
        // Do we have the required biome?
        if (RequiredBiomes != null && !RequiredBiomes.Contains(hex.Biome))
            return false;
        
        // Do we have the required relief?
        if (RequiredRelief != null && !RequiredRelief.Contains(hex.Relief))
            return false;
        
        return true;
    }
}
