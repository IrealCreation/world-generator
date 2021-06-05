using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using QPath;

public class Unit : MapObject, IQPathUnit
{
    public int Movement = 3; // Base movement of the unit
    public int MovePoints; // Movement points currently available for the turn
    public bool CanSearch;
    public bool CanBuildCity;
    public People People;

    // A delegate describes the signature of a function. It is not a function. When we set up an event listener, it expect a delegate name.
    public delegate void OnMoveDelegate( Hex newHex );
    // An event is a variable that will hold one or more functions who all get called when it happens, kinda like an array.
    // The "event" keyword makes it semi-public : we can add functions to it outside the object, but execute it only inside the object
    public event OnMoveDelegate OnMove;

    Queue<Hex> hexPath; // Path this unit is going to follow. First item is the hex we're standing in
    
    public Unit(string name, Hex hex, GameObject go, People people, int movement, bool canSearch = false, bool canBuildCity = false) {
        base.Hex = hex;
        GO = go;
        People = people;
        Name = name;
        MapObjectType = "Unit";
        Life = 100;
        Strength = 10;
        Movement = movement;
        CanSearch = canSearch;
        CanBuildCity = canBuildCity;
        
        MovePoints = Movement;
    }

    /// <summary>
    /// Move to a new hex and handle its animation
    /// </summary>
    /// <param name="hex">The hex to move to</param>
    public void SetHex(Hex hex) { 
		if(base.Hex != null) {
    		base.Hex.RemoveUnit(this);
    	}

    	hex.AddUnit(this);

        if (People != null)
        {
            People.Explore(hex, 2);
            if(CanSearch)
                hex.HexMap.HighlightSearchableHexes(People);
        }

        OnMove(hex);

    	base.Hex = hex;
    }

    public void SetHexPath( Hex[] hexPath ) {
        this.hexPath = new Queue<Hex>( hexPath );
        DoPath();
    }
    public Hex[] GetHexPath()
    {
        return (this.hexPath == null) ? null : this.hexPath.ToArray();
    }

    public void ClearHexPath()
    {
        this.hexPath = new Queue<Hex>();
    }

    public bool IsWaitingForOrders()
    {
        if (MovePoints > 0 && (hexPath == null || hexPath.Count == 0))
        {
            //TODO: return false if this unit has been told to wait, fortify...
            return true;
        }

        return false;
    }

    public void StartTurn()
    {
        // Debug.Log("unit StartTurn");
        
        // Start of the turn : heal, get movepoints...
        MovePoints = Movement;
    }

    public bool EndTurn()
    {
        DoPath();
        return IsWaitingForOrders();
    }

    /// <summary>
    /// Tries to advance along the hexPath of the unit
    /// </summary>
    public void DoPath()
    {
        //Debug.Log("unit DoTurn");
        
        // If no movepoints left, we stay were we are
        if (MovePoints > 0)
        {
            // The path of the unit starts at the current hex
            SetHex(Hex);

            //StartCoroutine(UnitMoveCoroutine());
            while (DoMove())
            {
                //Debug.Log("DoMove returned true -- will be called again");
            }
        }
    }
    
    /// <summary>
    /// Processes one tile worth of movement for the unit.
    /// </summary>
    /// <returns>Returns true if this should be called again.</returns>
    private bool DoMove()
    {
        // Debug.Log("Unit DoMove");

        // If no move is planned, let's leave and don't come back
        if (hexPath == null || hexPath.Count == 0)
        {
            return false;
        }
        
        // Leave the first hex from our path queue, and move to the next one
        Hex oldHex = hexPath.Dequeue();
        Hex newHex = hexPath.Peek();
        
        if(hexPath.Count == 1)
            ClearHexPath(); // Let's clear the queue if the only hex remaining is the current one
        
        // Move to the new hex
        SetHex(newHex);
        
        // Substract movepoints (or set it to 0 if negative)
        MovePoints = Mathf.Max(MovePoints - CostToEnterTile(oldHex, newHex), 0);
    
        // Returns true if we have movement left and something in queue
        return hexPath != null && MovePoints > 0;
    }
    
    public int CostToEnterTile(IQPathTile sourceTile, IQPathTile destinationTile)
    {
        Hex sourceHex = (Hex) sourceTile;
        Hex destinationHex = (Hex) destinationTile;

        int cost = (int)destinationHex.CostToEnter();

        if(destinationHex.Relief.IsWater != sourceHex.Relief.IsWater) //Amphibious transition
            cost = 99999; //Automatically ends the turn on this move
        
        //Debug.Log("Cost to go from " + sourceTile.ToString() + " to " + destinationTile.ToString() + " = " + cost);

        return cost;
    }

    public float AggregateTurnsToEnterTile( IQPathTile sourceTile, IQPathTile destinationTile, float turnsToDate )
    {
        //returns turnsToDate + turns for this move

        float baseTurnsToEnterTile = (float)CostToEnterTile(sourceTile, destinationTile) / Movement;
        //Debug.Log("baseTurnsToEnterTile " + destinationTile + " = " + baseTurnsToEnterTile);

        float turnsToDateWhole = Mathf.Floor(turnsToDate);
        float turnsToDateFraction = turnsToDate - turnsToDateWhole;

        if ( ( turnsToDateFraction > 0 && turnsToDateFraction < 0.01f ) || turnsToDateFraction > 0.99f)
        {
            Debug.LogError("We've got floating point drift in AggregateTurnsToEnterTile: " + turnsToDateFraction);

            if(turnsToDateFraction < 0.01f)
                turnsToDateFraction = 0;
            if (turnsToDateFraction > 0.99f)
            {
                turnsToDateWhole += 1;
                turnsToDateFraction = 0;
            }

        }

        float turnsUsedAfterThisMove = turnsToDateFraction + baseTurnsToEnterTile;

        if (turnsUsedAfterThisMove > 1)
        {
            //Civ 5 rule : trying to move to an hex with a movecost > moveremaining allows you to enter the hex 
            turnsUsedAfterThisMove = 1;
        }

        return turnsToDateWhole + turnsUsedAfterThisMove;
    }
}
