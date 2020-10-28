using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A production job that can be performed by a city
/// </summary>
public class CityJob
{
    public CityJob( string name, float productionNeeded, float overflowedProduction)
    {
        Name = name;
        ProductionNeeded = productionNeeded;
        ProductionDone = overflowedProduction;
    }

    public string Name;

    public float ProductionNeeded;
    public float ProductionDone;

    // I'm not sure I like this way of doing things with delegate... maybe rather add things like BuildingProduced, UnitProduced... 
    public delegate void ProductionCompleteDelegate(City city);
    public ProductionCompleteDelegate OnProductionComplete;

    public void AddProduction( float rawProduction )
    {
        // Possibly, check for production bonuses
        ProductionDone += rawProduction;

        if (ProductionDone >= ProductionNeeded)
        {
            CompleteProduction();
        }
    }

    public void CompleteProduction()
    {
        // TODO
    }
}
