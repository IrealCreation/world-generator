using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using QPath;

/*
	Defines grid position, world space position, size, neighbours... of a Hex Tile.
	It does not interact with Unity directly.
*/
public class Hex : IQPathTile {

	// q + r + s = 0
	// s = -(q + r)

	public readonly int Q; // Column
	public readonly int R; // Row
	public readonly int S; // Sum

	public float Elevation = -1;
	public float Rain = 0; // Used to calculate Moisture, as the average of neighbours' rain
	public float Moisture = 0;
	public float Temperature = 0;

	public Relief Relief;
	public Biome Biome; // Coast, Ocean, Tropical, Savanna, Desert, Steppe, Temperate, Taiga, Tundra
	public Feature Feature;
	public string GeologyName; // Peak, Rift, Volcano (in HexMap_Geology)
	public bool IsSea = false; // Distinguish seas from lakes on water tiles

	public bool? IsVisible = null; // Remember if this hex is visible or not (so we know if we have to visually update it)
	// (We use a nullable bool so "null" means it was never updated)

	public GameObject hexGO;

	public HexMap HexMap;

	private Hex[] neighbours; //List of adjacents hexes

	public List<Unit> units;
	public City city;

	public static readonly float RADIUS = 1f; //From the middle to the pointy edge
	public static readonly float WIDTH_MULTIPLIER = Mathf.Sqrt(3f) / 2f * RADIUS;

	public Hex(HexMap hexMap, int q, int r) {
		// Constructor
		this.HexMap = hexMap;

		this.Q = q;
		this.R = r;
		this.S = -(q + r);
	}

	public override string ToString()
	{
		return Q + ", " + R;
	}

	public Vector3 Position() {
		//Returns the world-space position of this hex

		return new Vector3(
			HorizontalSpacing() * (this.Q + this.R / 2f),
			0,
			VerticalSpacing() * this.R
		);

	}

	public Vector3 ReversePosition()
	{
		//Returns the world-space position with -Q
		return new Vector3(
			HorizontalSpacing() * (this.Q - HexMap.numColumns + this.R / 2f),
			0,
			VerticalSpacing() * this.R
		);
	}

	public float Width() {
		return WIDTH_MULTIPLIER * Height();
	}
	public float Height() {
		return RADIUS * 2;
	}
	public float HorizontalSpacing() {
		return Width();
	}
	public float VerticalSpacing() {
		return Height() * 3 / 4;
	}

	public Vector3 PositionFromCamera(Vector3 cameraPosition, float numRows, float numColumns) {

		float mapHeight = numRows * VerticalSpacing();
		float mapWidth = numColumns * HorizontalSpacing();

		Vector3 position = Position();

		float howManyWidthsFromCamera = (position.x - cameraPosition.x) / mapWidth;

		if(howManyWidthsFromCamera > 0) {
			howManyWidthsFromCamera += 0.5f;
		}
		else {
			howManyWidthsFromCamera -= 0.5f;
		}

		int howManyWidthsToFix = (int)howManyWidthsFromCamera; //Remove the decimal part

		position.x -= howManyWidthsToFix * mapWidth;

		return position;

	}
	public Vector3 PositionFromCamera() {
		//Return the PositionFromCamera of the current hex
		return this.PositionFromCamera( Camera.main.transform.position, this.HexMap.numRows, this.HexMap.numColumns );
	}
	public static float Distance(Hex a, Hex b) {
		// Returns the distance between two hexes

		int dQ = Mathf.Abs(a.Q - b.Q);
        if(dQ > a.HexMap.numColumns / 2) {
            dQ = a.HexMap.numColumns - dQ;
        }

        int dR = Mathf.Abs(a.R - b.R);
		
        int dS = Mathf.Abs(a.S - b.S);
        if(dS > a.HexMap.numColumns / 2) {
	        dS = a.HexMap.numColumns - dS;
        }

        //Debug.Log("Distance: dQ = " + dQ + ", dR = " + dR + ", dS = " + dS);

        return Mathf.Max( dQ, dR, dS);
	}

	public static Vector3 WorldDistance(Hex a, Hex b)
	{
		// Returns the distance between two hexes in world-space coordinates
		float dist1 = Vector3.Distance(b.Position(), a.Position());
		// To correctly calculate distance between hexes near the Q0 junction...
		float dist2 = Vector3.Distance(b.ReversePosition(), a.Position());
		float dist3 = Vector3.Distance(b.Position(), a.ReversePosition());
		
		if(Mathf.Min(dist1, dist2, dist3) == dist1)
			return b.Position() - a.Position();
		else if (Mathf.Min(dist1, dist2, dist3) == dist2)
			return b.ReversePosition() - a.Position();
		else
			return b.Position() - a.ReversePosition();
	}

	public List<Edge> GetEdges(bool AOrB = true)
	{
		List<Edge> result = new List<Edge>();
		Edge e;

		e = HexMap.GetEdgeAt(this.Q, this.R, this.Q + 1, this.R);
		if(e != null)
			result.Add(e);
		e = HexMap.GetEdgeAt(this.Q, this.R, this.Q, this.R + 1);
		if(e != null)
			result.Add(e);
		e = HexMap.GetEdgeAt(this.Q, this.R, this.Q + 1, this.R - 1);
		if(e != null)
			result.Add(e);

		if (AOrB)
		{
			e = HexMap.GetEdgeAt(this.Q - 1, this.R, this.Q, this.R);
			if(e != null)
				result.Add(e);
			e = HexMap.GetEdgeAt(this.Q, this.R - 1, this.Q, this.R);
			if(e != null)
				result.Add(e);
			e = HexMap.GetEdgeAt(this.Q - 1, this.R + 1, this.Q, this.R);
			if(e != null)
				result.Add(e);
		}

		return result;
	}

	public void AddUnit( Unit unit ) {

		if(units == null) {
			units = new List<Unit>();
		}

		units.Add(unit);
	}

	public void RemoveUnit( Unit unit ) {

		units.Remove(unit);
	}

	public Unit[] Units() 
	{
		if(units != null && units.Count > 0)
			return units.ToArray();

		return null;
	}

	public void AddCity(City city)
	{
		this.city = city;
	}
	public void RemoveCity()
	{
		this.city = null;
	}

	public bool CanBuildCity()
	{
		if (city != null)
			return false;
		return true;
	}

	public Dictionary<string, float> GetResourceGain()
	{
		Dictionary<string, float> resources = Biome.Resources;
		if (Relief.Name == "Flat" || Relief.Name == "Water")
			resources["food"] += 1;
		else if (Relief.Name == "Hill")
		{
			if (resources["military"] > 0)
				resources["military"] += 1;
			else
				resources["wealth"] += 1;
		}
		else if (Relief.Name == "Mountain")
		{
			if (resources["wealth"] > 0)
				resources["wealth"] += 1;
			else if(resources["culture"] > 0)
				resources["culture"] += 1;
			else if(resources["science"] > 0)
				resources["science"] += 1;
		}

		resources = Utils.SumValueDictionaries(resources, Feature.Resources);
		return resources;
	}

	public string Description()
	{
		string log = ToString() + ". Elevation: " + Elevation +
		             ", Temperature: " + Temperature +
		             ", Moisture: " + Moisture +
		             ". " + Biome.Name + " " + Relief.Name;
		string description = Biome.Name + " " + Relief.Name;

		if (Feature != null)
			description += ", " + Feature.Name;

		foreach (Edge e in GetEdges())
		{
			if (e.River != null)
			{
				log += ", River";
				description += ", River";
				break;
			}
		}
		
		description += ".\nElevation: " + (int) (Elevation * 4000) + "m" +
		               "\nTemperature: " + Math.Round(Temperature * 70 - 30, 1) + "°C" +
		               "\nHumidity: " + Math.Round(Moisture * 100, 1) + "%";

		//Debug.Log(log);
		return description;
	}

	public float CostToEnter()
	{
		// Base cost to enter. Can be overloaded to check for specific unit requirements
		float result = 1 + Relief.MovementCost;
		if (Feature != null)
			result += Feature.MovementCost;
		return result;
	}

	public static float CostEstimate(IQPathTile a, IQPathTile b)
	{
		// We multiply the estimate by 0.5 as it results in a slower search, but a better path (I guess?)
		return 0.5f * Distance((Hex) a, (Hex) b) + ((Hex) b).CostToEnter();
	}

	public IQPathTile[] GetNeighbours()
	{
		if (neighbours == null) {
			this.neighbours = this.HexMap.GetHexesWithinRangeOf(this, 1);
		}

		return this.neighbours;
	}

	public float AggregateCostToEnter(float costSoFar, IQPathTile sourceTile, IQPathUnit unit)
	{
		return unit.AggregateTurnsToEnterTile(sourceTile, this, costSoFar);
	}
}
