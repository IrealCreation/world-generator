using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using QPath;

public class River : IQPathUnit
{
    public string Name = "Lunain";
    
    public Edge Source; //Origin of the river
    
    public Edge[] Path; //Path of the river

    public River(Edge source)
    {
        this.Source = source;
    }

    public bool CalculatePath()
    {
        // We need to calculate the optimal path from the source to the sea
        // We're currently testing it by pressing "r" (see MouseController.Update)
        
        // Do a pathfinding search
        Path =
            QPath.QPathRiver.FindPath(Source.HexMap, this, Source, Edge.RiverCostEstimate, Edge.RiverEndValidate);
        
        // If a river is shorter than four tiles, she doesn't deserve to live
        if (Path.Length < 4)
            return false;

        //Debug.Log("River path:");

        foreach (Edge e in Path)
        {
            e.SetRiver(this);
            //Debug.Log(e);
        }

        Source.HexMap.DrawRiver(this);

        return true;
    }
    
    private float CostToEnterTile(IQPathTile sourceTile, IQPathTile destinationTile)
    {
        Edge sourceEdge = (Edge) sourceTile;
        Edge destinationEdge = (Edge) destinationTile;
        
        float cost = destinationEdge.GetElevation();
        if (destinationEdge.GetElevation() > sourceEdge.GetElevation())
            cost += Mathf.Pow(destinationEdge.GetElevation() - sourceEdge.GetElevation(), 2) * 4;
        
        //Debug.Log("River cost to go from " + sourceTile.ToString() + " to " + destinationTile.ToString() + " = " + cost);

        return cost;
    }

    public float AggregateTurnsToEnterTile( IQPathTile sourceTile, IQPathTile destinationTile, float costSoFar )
    {
        //returns turnsToDate + turns for this move

        float additionalCost = CostToEnterTile(sourceTile, destinationTile);

        return costSoFar + additionalCost;
    }
}