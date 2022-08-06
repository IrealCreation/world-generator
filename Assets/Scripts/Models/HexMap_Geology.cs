using System.Collections;
using System.Collections.Generic;
using QPath;
using UnityEngine;

/// <summary>
/// Creates a map based on mountain ranges, geological rifts, and isolated volcanoes for elevation.
/// Uses tropics and equators latitudes for temperature.
/// </summary>
public class HexMap_Geology : HexMap
{

	public int minNumLands = 8;
	public int maxNumLands = 10;
	public int minRangeLands = 8;
	public int maxRangeLands = 12;

	public int geologyLengthMin = 8;
	public int geologyLengthMax = 14;
	public int geologyNumMin = 5;
	public int geologyNumMax = 7;
	public int elevationRangeMin = 6;
	public int elevationRangeMax = 8;
	public int volcanoesNumMin = 8;
	public int volcanoesNumMax = 10;

	private GeologyPathfinder pathfinder;
    
    override public void GenerateMap(int seed) {
	    
	    // For testing, we can seed the randomness to have predictable outcomes
	    //Random.InitState(756);

    	// Call the base version to make all the hexes needed as a giant ocean
    	base.GenerateMap();
        
        // Prepare the pathfinder
        pathfinder = new GeologyPathfinder(); 

    	// Create mountain ranges
    	int numRanges = Random.Range(geologyNumMin, geologyNumMax + 1);
    	for(int i = 0; i < numRanges; i++) {

    		bool done = false;

    		while(!done) {
	    		done = MountainRange();
	    	}
    	}
        
        // Create volcanoes
        
        // Elevate the area around peaks and volcanoes to create islands and continents
        for(int column = 0; column < numColumns; column ++) {

    		for(int row = 0; row < numRows; row ++) {

    			Hex h = GetHexAt(column, row);
                if(h.GeologyName == "Peak")
	                ElevateArea(h, Random.Range(elevationRangeMin, elevationRangeMax));
                else if (h.GeologyName == "Volcano")
	                ElevateArea(h, Random.Range(elevationRangeMin, elevationRangeMax) / 2);
    		}
    	}
        
        //TODO: create rifts

    	// Make some noise!
    	Vector2 noiseOffset = new Vector2( Random.Range(0f, 1f), Random.Range(0f, 1f) ); //Seeding of the noise
    	float noiseScale = 0.6f; //Scale of effect of the noise
    	for(int column = 0; column < numColumns; column ++) {

    		for(int row = 0; row < numRows; row ++) {

    			Hex h = GetHexAt(column, row);
    			h.Elevation += (Mathf.PerlinNoise( (float)column / numColumns + noiseOffset.x, (float)row / numRows + noiseOffset.y ) - 0.5f) * noiseScale;

                // To add islands, lakes, and valleys, add a small chance to change elevation
                if(Random.Range(0, 25) == 0) {
                    if(h.Elevation > 0.3) {
                        h.Elevation = Random.Range(-0.3f, 0.3f);
                    }
                    else if(h.Elevation < -0.1f && h.Elevation > 0.99f) {
                        h.Elevation = Random.Range(-0.3f, 0.3f);
                    }
                }
    		}
    	}
//
//    	// Simulate moisture through Perlin Noise
//    	noiseOffset = new Vector2( Random.Range(0f, 1f), Random.Range(0f, 1f) ); //Seeding of the noise
//    	for(int column = 0; column < numColumns; column ++) {
//
//    		for(int row = 0; row < numRows; row ++) {
//
//    			Hex h = GetHexAt(column, row);
//    			h.Moisture = Mathf.PerlinNoise( (float)column / numColumns + noiseOffset.x, (float)row / numRows + noiseOffset.y );
//
//    			// Being near sea level increases moisture
//    			if(h.Elevation > 0) {
//    				h.Moisture += Mathf.Lerp( 0.25f, -0.25f,  h.Elevation );
//    			}
//    		}
//    	}
//
//    	// Take in consideration latitude and elevation to calculate temperature
//    	int equator = numRows / 2;
//    	int distanceToEquator;
//    	noiseOffset = new Vector2( Random.Range(0f, 1f), Random.Range(0f, 1f) ); //Seeding of the noise
//    	for(int column = 0; column < numColumns; column ++) {
//
//    		for(int row = 0; row < numRows; row ++) {
//
//    			Hex h = GetHexAt(column, row);
//
//    			distanceToEquator = Mathf.Abs(equator - row);
//
//    			h.Temperature = Mathf.Lerp( 1f, 0f,  (float)distanceToEquator / equator );
//
//    			// Another discreet Perlin Noise to give some randomness
//    			h.Temperature += Mathf.PerlinNoise( (float)column / numColumns + noiseOffset.x, (float)row / numRows + noiseOffset.y ) * 0.2f;
//
//    			// Temperature decreases exponentially with elevation
//    			if(h.Elevation > 0) {
//    				float modifier = Mathf.Pow(h.Elevation, 2f) / 2.5f;
//    				h.Temperature *= 1f - modifier;
//    			}
//    		}
//    	}

    	// Update hex visuals to match data

    	UpdateHexesVisuals();
    		
    }

    bool MountainRange()
    {
	    //Pick an origin and end hex
	    int range = Random.Range(geologyLengthMin, geologyLengthMax);
	    int originQ = Random.Range(0, numColumns);
	    int originR = Random.Range(range / 2, numRows - range / 2);
	    
	    Hex originHex = GetHexAt(originQ, originR);
	    
	    Hex[] areaHexes = GetHexesAtDistanceOf(originHex, range);
	    Hex destinationHex = areaHexes[Random.Range(0, areaHexes.Length)];

	    Debug.Log("Mountain range of " + range + " from " + originHex + " to " + destinationHex);
	    
	    Hex[] hexPath = QPath.QPath.FindPath<Hex>(pathfinder, originHex, destinationHex, Hex.CostEstimate);
	    foreach (Hex hex in hexPath)
	    {
		    hex.Elevation = Random.Range(190, 200) / 100f;
		    hex.GeologyName = "Peak";
	    }

	    return true;
    }

    bool Volcano()
    {
	    int q = Random.Range(0, numColumns);
	    int r = Random.Range(0, numRows);

	    Hex hex = GetHexAt(q, r);
	    hex.Elevation = Random.Range(190, 200) / 100f;
	    hex.GeologyName = "Volcano";

	    Debug.Log("Volcano at " + hex);

	    return true;
    }

    bool ElevateArea(Hex centerHex, int range) {

	    float centerHeight = centerHex.Elevation;
	    
	    float distanceRatio;
	    float elevation;
	    int variability;
	    int modifier;

	    Hex[] areaHexes = GetHexesWithinRangeOf(centerHex, range);

	    foreach(Hex h in areaHexes) {

		    distanceRatio = (float)Hex.Distance(centerHex, h) / range;
		    elevation = centerHeight * Mathf.Lerp( 1f, Random.Range(0, 15) / 100f,  distanceRatio );
		    variability = (int)((0.5f - Mathf.Abs(distanceRatio - 0.5f)) * 2f * 0.3f * 100f);
		    modifier = Random.Range(-variability, variability);
		    //Debug.Log("Variability : " + variability + ", distanceRatio : " + distanceRatio);

		    elevation += ((float)(modifier) / 100f);

		    // Diminish elevation given if the hex is already higher than sea, to nerf stacked mountain ranges
//		    if(h.Elevation > 0) {
//			    elevation = Mathf.Lerp( elevation, elevation * 0.2f, Mathf.Min(elevation, 1) );
//		    }

		    h.Elevation = Mathf.Max(h.Elevation, elevation);
	    }
    	
	    return true;
    }
}

public class GeologyPathfinder : IQPathUnit
{
	public int CostToEnterTile(IQPathTile fromTile, IQPathTile toTile)
	{
		return 1;
	}

	public float AggregateTurnsToEnterTile(IQPathTile sourceTile, IQPathTile destinationTile, float turnsToDate)
	{
		return turnsToDate + CostToEnterTile(sourceTile, destinationTile);
	}
}
