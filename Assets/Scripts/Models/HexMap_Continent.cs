using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class HexMap_Continent : HexMap
{

	public int minNumLands = 8;
	public int maxNumLands = 10;
	public int minRangeLands = 8;
	public int maxRangeLands = 12;

	private float coastElevation = -0.5f;
	private float hillElevation = 0.55f;
	private float mountainElevation = 0.9f;
    
    override public void GenerateMap(int seed = 0) {
	    
	    // For testing, we can seed the randomness to have predictable outcomes
	    if(seed == 0)
			seed = Random.Range(1, 999999);
	    
	    Debug.Log("Map seed: " + seed);
	    Random.InitState(seed);

    	// Call the base version to make all the hexes needed as a giant ocean
    	base.GenerateMap();

    	// Make some kind of raised area to create a continent
    	int numLands = Random.Range(minNumLands, maxNumLands + 1);
    	for(int i = 0; i < numLands; i++) {

    		bool done = false;

    		while(!done) {

	    		int range = Random.Range(minRangeLands, maxRangeLands + 1);
	    		int q = Random.Range(0, numColumns);
	    		int r = Random.Range(range / 2, numRows - range / 2);
	    		done = ElevateArea(q, r, range);
	    	}
    	}

    	// Make some noise on elevation
    	Vector2 noiseOffset = new Vector2( Random.Range(0f, 1f), Random.Range(0f, 1f) ); //Seeding of the noise
    	float noiseScale = 0.6f; //Scale of effect of the noise
    	for(int column = 0; column < numColumns; column ++) {

    		for(int row = 0; row < numRows; row ++) {

    			Hex h = GetHexAt(column, row);
    			h.Elevation += (Mathf.PerlinNoise( (float)column / numColumns + noiseOffset.x, (float)row / numRows + noiseOffset.y ) - 0.5f) * noiseScale;

                // To add islands, lakes, and valleys, add a small chance to change elevation
                if(Random.Range(0, 25) == 0) {
                    if(h.Elevation > 0.5f) {
                        h.Elevation = Random.Range(-0.3f, 0.3f);
                    }
                    else if(h.Elevation < 0f && h.Elevation > -0.5f) {
                        h.Elevation = Random.Range(0f, 0.3f);
                    }
                }
    		}
    	}
        
        // Add some further variation in mountains blobs
        ValleysAndLakes();
        
        // Make sure any water tile near land is coastal
        CoastalTiles();
    	
        // Now that elevation is done, identify the relief of each tile
        IdentifyRelief();
        
        // Distinguish lakes from seas
        IdentifySeas();

    	// Simulate moisture through Perlin Noise and elevation
        // RandomMoisture(0.5f);
        // ElevationMoisture(0.5f);
        
        // Moisture through cloud simulations plus a little bit of randomness
        CloudMoisture(0.8f);
        RandomMoisture(0.2f);
        
        // Make rivers
        MakeRivers();

    	// Take in consideration latitude and elevation to calculate temperature
    	int equator = numRows / 2;
    	int distanceToEquator;
    	noiseOffset = new Vector2( Random.Range(0f, 1f), Random.Range(0f, 1f) ); //Seeding of the noise
    	for(int column = 0; column < numColumns; column ++) {

    		for(int row = 0; row < numRows; row ++) {

    			Hex h = GetHexAt(column, row);

    			distanceToEquator = Mathf.Abs(equator - row);

    			h.Temperature = Mathf.Lerp( 1f, 0f,  (float)distanceToEquator / equator );

    			// Another discreet Perlin Noise to give some randomness
                float noise = Mathf.PerlinNoise( (float)column / numColumns + noiseOffset.x, (float)row / numRows + noiseOffset.y ) * 0.2f;
                h.Temperature += noise;

    			// Temperature decreases exponentially with elevation
    			if(h.Elevation > 0) {
    				//float modifier = Mathf.Pow(h.Elevation, 2f) / 2.5f;
                    float modifier = h.Elevation / 3f;
    				h.Temperature -= modifier;
    			}

                h.Temperature = Mathf.Clamp(h.Temperature, 0f, 1f);
    		}
    	}
        
    	// Pick biome depending of temperature and moisture
        IdentifyBiomes();
        
        // Add resources depending on biomes and other characteristics
        IdentifyResources();
        
        // Add features depending on biomes and other characteristics
        IdentifyFeatures();
        
        // Log some stats concerning the map
        LogStats();
    		
    }

    void RandomMoisture(float modif = 1f)
    {
	    // Simulate moisture through Perlin Noise
	    Vector2 noiseOffset = new Vector2( Random.Range(0f, 1f), Random.Range(0f, 1f) ); //Seeding of the noise
	    for(int column = 0; column < numColumns; column ++) {

		    for(int row = 0; row < numRows; row ++) {

			    Hex h = GetHexAt(column, row);
			    h.Moisture += Mathf.PerlinNoise( (float)column / numColumns + noiseOffset.x, (float)row / numRows + noiseOffset.y ) * modif;
		    }
	    }
    }

    void ElevationMoisture(float modif = 1f)
    {
	    // Simulate moisture based solely on the elevation
	    foreach (Hex h in hexes)
	    {
		    // Being near sea level increases moisture
		    if(h.Elevation > 0) {
			    h.Moisture += Mathf.Lerp( 1f, 0f,  h.Elevation ) * modif;
		    }
	    }
    }

    void CloudMoisture(float modif = 1f)
    {
	    // Simulate moisture through clouds distributing wetness
	    // TODO: better simulate wind flow, by creating prevailing winds

	    float maxPrecipitation = (minRangeLands + maxRangeLands) / 10f;
	    //Debug.Log("Horse latitudes at rows " + (numRows / 9f * 3f) + " and " + (numRows / 9f * 6f));
	    
	    for(int row = 0; row < numRows; row ++) {
		    
		    // First, simulate wind blowing west to east
		    float precipitation = 0f;
		    
		    // Take into consideration the "horse latitudes" around the subtropical ridge with lower precipitations
		    // We roughly put the subtropics around 3/9 and 6/9 of map height, with 1/9 of range...
		    // ...so it covers between 2/9-4/9 and 5/9-7/9 of map height
		    float latitudeModifier = 1f;
		    
		    if(row > (numRows / 9f * 2f) && row < (numRows / 9f * 4f))
		    {
			    float distanceRatio = 1f - Mathf.Abs(row - numRows / 9f * 3f) / (numRows / 9f);
			    latitudeModifier = 1f - 0.7f * distanceRatio;
			    //Debug.Log("Row " + row + " rain latitude modifier: " + latitudeModifier);
		    }
		    else if(row > (numRows / 9f * 5f) && row < (numRows / 9f * 7f))
		    {
			    float distanceRatio = 1f - Mathf.Abs(row - numRows / 9f * 6f) / (numRows / 9f);
			    latitudeModifier = 1f - 0.7f * distanceRatio;
			    //Debug.Log("Row " + row + " rain latitude modifier: " + latitudeModifier);
		    }
		    
		    // We'll make two passes, so the first hexes can also be humidified
		    int column = 0;

		    for(int columnLoop = 0; columnLoop < numColumns * 2; columnLoop ++) {
			    Hex hex = GetHexAt(column, row);
			    Hex nextHex = GetHexAt(column + 1, row);

			    if (hex.Elevation < 0)
			    {
				    // The current hex is water: let's accumulate humidity
				    // Oceans give twice as much humidity than coasts and lakes
				    if (hex.Elevation < coastElevation)
					    precipitation++;
				    else
					    precipitation += 0.5f;
				    
				    if (precipitation > maxPrecipitation)
					    precipitation = maxPrecipitation;
				    
				    hex.Rain = 1f;
			    }
			    else if (precipitation > 0)
			    {
				    // The current hex isn't water: make it rain!
				    float rain = precipitation / maxPrecipitation;
				    
				    // The next hex elevation blocks precipitation from going through, known as "rain shadow"
				    float rainShadow = Mathf.Pow(nextHex.Elevation, 2);
				    
				    rain += Mathf.Min(precipitation, rainShadow);
				    
				    precipitation -= rainShadow;
				    if (precipitation < 0)
					    precipitation = 0;

				    rain *= latitudeModifier;
				    hex.Rain = rain;
				    if (hex.Rain > 1f)
					    hex.Rain = 1f;
			    }

			    column++;
			    if (column > numColumns)
				    column = 0;
		    }
		    
		    // Then, let's do east to west
		    precipitation = 0f;
		    column = numColumns;

		    for(int columnLoop = 0; columnLoop < numColumns * 2; columnLoop ++) {
			    Hex hex = GetHexAt(column, row);
			    Hex nextHex = GetHexAt(column - 1, row);

			    if (hex.Elevation < 0)
			    {
				    precipitation ++;
				    if (precipitation > maxPrecipitation)
					    precipitation = maxPrecipitation;
				    
				    hex.Rain = 1f;
			    }
			    else if (precipitation > 0)
			    {
				    // The current hex isn't water: make it rain!
				    float rain = precipitation / maxPrecipitation;
				    
				    // The next hex elevation blocks precipitation from going through, known as "rain shadow"
				    float rainShadow = Mathf.Pow(nextHex.Elevation, 2);
				    
				    rain += Mathf.Min(precipitation, rainShadow);
				    
				    precipitation -= rainShadow;
				    if (precipitation < 0)
					    precipitation = 0;

				    rain *= latitudeModifier;
				    if(rain > hex.Rain)
						hex.Rain = rain;
				    
				    if (hex.Rain > 1f)
					    hex.Rain = 1f;
			    }

			    column--;
			    if (column < 0)
				    column = numColumns;
		    }
	    }
		    
	    // Finally, smooth it by taking the average of neighbouring tiles + the current tile (counted double)
	    foreach (Hex h in hexes)
	    {
		    Hex[] neighbours = GetHexesWithinRangeOf(h, 1);
		    float sum = h.Rain;

		    foreach (Hex neighbour in neighbours)
		    {
			    sum += neighbour.Rain;
		    }

		    h.Moisture += sum / (neighbours.Length + 1) * modif;
		    //Debug.Log("Tile " + hex + ". Rain sum: " + sum + ", Moisture: " + hex.Moisture);
	    }
    }

    void MakeRivers()
    {
    	// Create rivers
	    // More efficient to iterate over edges of hexes[], than to foreach edges[]
	    foreach (Hex h in hexes)
	    {
		    foreach (Edge e in h.GetEdges(false))
		    {
			    if (e != null)
			    {
				    if (e.River != null || e.HexA.Elevation < 0 || e.HexB.Elevation < 0)
					    continue;

				    float riverScore = Mathf.Max(e.HexA.Moisture, e.HexB.Moisture) +
				                       (e.HexA.Elevation + e.HexB.Elevation) / 2f;
				    if (riverScore > 1f)
				    {
					    int riverChance = (int)(riverScore * 4f - 2f);
					    if (Random.Range(0, 100) < riverChance)
					    {
						    Debug.Log("Adding river with source " + e);
						    River river = new River(e);
						    river.CalculatePath();
						    rivers.Add(river);
					    }
				    }
			    }
		    }
	    }
	    
	    // Increase moisture and decrease elevation in hexes bordering rivers
	    foreach (Hex h in hexes)
	    {
		    if(h.Elevation >= 0) { 
			    foreach (Edge e in h.GetEdges(true))
			    {
				    if (e.River != null)
				    {
					    float random = Random.Range(0.2f, 0.3f);
					    h.Moisture += random;
					    if (h.Moisture > 1f)
						    h.Moisture = 1f;
					    h.Elevation *= 0.8f;
					    //Debug.Log("Tile " + h + " hydrated by " + random + " to " + h.Moisture);
					    break;
				    }
			    }
		    }
	    }
    }

    void IdentifyRelief()
    {
	    foreach (Hex h in hexes)
	    {
		    if(h.Elevation > mountainElevation)
		    {
			    h.Relief = reliefs["Mountain"];
		    }
    		else if(h.Elevation > hillElevation) 
		    {
			    h.Relief = reliefs["Hill"];
            }
    		else if(h.Elevation > 0) 
		    {
			    h.Relief = reliefs["Plain"];
    		}
            else 
		    {
			    h.Relief = reliefs["Water"];
            }
	    }
    }

    void IdentifyBiomes()
    {
	    foreach (Hex h in hexes)
	    {
		    /*
		     * Tropical = high temperature, high moisture
		     * Savanna = high temperature, medium moisture
		     * Desert = high temperature, very low moisture
		     * Steppe = medium-high temperature, medium-low moisture
		     * Temperate = medium-low temperature, medium-high moisture
		     * Taiga = low temperature, high moisture
		     * Tundra = very low temperature
		     *
		     * (Alpine / Rocky is inside Tundra)
		     */
            float tropicalScore = 0f; // high temperature, high moisture
            float savannaScore = 0f; // high temperature, medium moisture
            float desertScore = 0f; // high temperature, very low moisture
            float steppeScore = 0f; // medium-high temperature, low moisture
            float temperateScore = 0f; // medium-low temperature, high moisture
            float taigaScore = 0f; // low temperature, high moisture
            float tundraScore = 0f; // very low temperature

            //Identify terrain
    		if(h.Elevation > 0) {
                if (h.Temperature >= 0.5f)
                {
	                //Unlocks the "high temperature" biomes
	                tropicalScore = (h.Temperature + h.Moisture) / 2;
	                savannaScore = (h.Temperature + (1f - Mathf.Abs(h.Moisture - 0.5f))) / 2;
	                desertScore = (h.Temperature * 0.5f + (Mathf.Pow(1f - h.Moisture, 2) * 1.5f) * 1.5f) / 2;
                }

                if (h.Temperature > 0.2f && h.Temperature < 0.8f)
                {
	                //Unlocks the "medium temperature" biomes
	                steppeScore = (1f - Mathf.Abs(h.Temperature - 0.5f) + 1f - h.Moisture) / 2;
	                temperateScore = (Mathf.Pow(1f - Mathf.Abs(h.Temperature - 0.5f), 2) + h.Moisture) / 2;
                }

                if (h.Temperature < 0.5f)
                {
	                //Unlocks the "low temperature" biomes
	                taigaScore = ((1f - h.Temperature) * 1.2f + h.Moisture * 0.8f) / 2;
	                tundraScore = Mathf.Pow(1f - h.Temperature, 2) * 1.1f;
	                
	                if (h.Elevation >= mountainElevation)
	                {
		                //High mountain bonus for tundra
		                tundraScore += (h.Elevation - mountainElevation) / 2;
	                }
                }
                
                //Compute the results to finally assign the biome
                float max = Mathf.Max(tropicalScore, savannaScore, desertScore, steppeScore, temperateScore,
	                taigaScore, tundraScore);

                if (max == tropicalScore)
	                h.Biome = biomes["Tropical"];
                else if (max == savannaScore)
					h.Biome = biomes["Savanna"];
                else if (max == desertScore)
					h.Biome = biomes["Desert"];
                else if (max == steppeScore)
					h.Biome = biomes["Steppe"];
                else if (max == temperateScore)
					h.Biome = biomes["Temperate"];
                else if (max == taigaScore)
					h.Biome = biomes["Taiga"];
                else if (max == tundraScore)
					h.Biome = biomes["Tundra"];
            }
            else
            {
                if (h.IsSea)
                {
	                if(h.Elevation > -0.7) {
		                h.Biome = biomes["Coast"];
	                }
	                else
	                {
		                h.Biome = biomes["Ocean"];
	                }
                }
                else
                {
	                h.Biome = biomes["Lake"];
                }
            }

    		//Debug.Log("Hex " + column + " " + row + " : Moisture = " + h.Moisture + ", Temperature = " + h.Temperature + ", Elevation = " + h.Elevation + ", Terrain = " + h.MeshName + " " + h.TerrainName);
	    }
    }

    void IdentifyFeatures()
    {
	    foreach (Hex h in hexes)
	    {
		    // Features can only spawn if there aren't resources
		    if(h.Resource != null)
			    continue;
		    
		    //Indentify features (forests, jungle, reef...)
		    if ((h.Relief.Name == "Plain" || h.Relief.Name == "Hill") &&
		        (h.Biome.Name != "Desert" && h.Biome.Name != "Tundra"))
		    {

			    int forestChance = (int) (h.Moisture * 30f);
			    if (h.HasRiver())
				    forestChance += 20;

			    if (h.Biome.Name == "Tropical" || h.Biome.Name == "Savanna")
			    {
				    if (Random.Range(0, 100) < forestChance)
				    {
					    h.Feature = features["Jungle"];
				    }

			    }
			    else
			    {
				    if (Random.Range(0, 100) < forestChance)
				    {
					    h.Feature = features["Forest"];
				    }
			    }
		    }
		    else if (h.Biome.Name == "Coast")
		    {
			    int reefChance = (int) ((h.Elevation + 1f) * 4f + 2);
			    if (Random.Range(0, 100) < reefChance)
			    {
				    h.Feature = features["Reef"];
			    }
		    }
	    }
    }

    void IdentifyResources()
    {
	    /*
	     * Other way we could do this: give to each tile its spawn score for each resource, then spawn X resources of each type
	     * on times that have the highest score (and add a negative effect to the spawn score of this resource on close tiles
	     * if we want to avoid clusters)
	     */
	    foreach (Hex h in hexes)
	    {
		    // Convert each entry of resources as a probability
		    List<KeyValuePair<Resource, int>> resourceChances = new List<KeyValuePair<Resource, int>>();
		    foreach (KeyValuePair<string, Resource> kv in resources)
		    {
			    // Is the local biome / relief allowed for this resource?
			    if (kv.Value.AllowedBiomes.Contains(h.Biome) && kv.Value.AllowedReliefs.Contains(h.Relief))
			    {
				    int chance = kv.Value.HexProbability(h);
				    if(chance > 0)
					    resourceChances.Add(new KeyValuePair<Resource, int>(kv.Value, chance));
			    }
		    }
		    
		    // If no resource can spawn here, let's move to next tile
		    if(resourceChances.Count < 1)
			    continue;
		    
		    // Sort the resource list based on their respective probability
		    resourceChances.Sort((x,y)=>x.Value.CompareTo(y.Value));
		    
		    // Go through the resource list until one is picked
		    foreach (KeyValuePair<Resource, int> kv in resourceChances)
		    {
			    if (Random.Range(0, 100) < kv.Value)
			    {
				    h.Resource = kv.Key;
				    Debug.Log("Resource of hex " + h + " : " + h.Resource.Name);
				    break;
			    }
		    }
	    }
    }
    
    void IdentifyTerrain_Old()
    {
	    /*for(int column = 0; column < numColumns; column ++) {

    		for(int row = 0; row < numRows; row ++) {

    			Hex h = GetHexAt(column, row);

                //Identify terrain
    			if(h.Elevation > 0) {
    				if(h.Temperature - h.Moisture * 0.75f > 0.55) {
	    				h.Biome.Name = "Desert";
    				}
    				else if(h.Temperature - h.Moisture / 5f <= 0.2) {
	    				h.Biome.Name = "Snow";
    				}
                    else if (h.Moisture + h.Temperature * 0.5f > 1.15f)
                    {
	                    h.Biome.Name = "Tropical";
                    }
    				else if(h.Elevation - h.Moisture > 0.4) {
	    				h.Biome.Name = "Rocky";
    				}
    				else {
	    				h.Biome.Name = "Plain";
    				}
    			}
                else
                {
	                if (h.IsSea)
	                {
		                if(h.Elevation > -0.7) {
			                h.Biome.Name = "Coast";
		                }
		                else
		                {
			                h.Biome.Name = "Ocean";
		                }
	                }
	                else
	                {
		                h.Biome.Name = "Lake";
	                }
                }

                //Indentify features (forests, jungle, reef...)
                if((h.ReliefName == "Plain" || h.ReliefName == "Hill") && (h.Biome.Name == "Plain" || h.Biome.Name == "Snow")) {

                    float jungleScore = h.Moisture + h.Temperature * 0.75f - 0.75f;
                    float forestScore = h.Moisture;

                    if(h.Biome.Name == "Tropical" || h.Biome.Name == "Desert") {
                        //Can spawn jungle
                        int jungleChance = (int)Mathf.Lerp( 50, 100,  jungleScore );
                        if( Random.Range (0, 100) < jungleChance)
                        {
	                        h.Feature = features["Jungle"];
                        }

                    }
                    else {
                        //Can spawn forest
                        int forestChance = (int)Mathf.Lerp( 0, 50,  forestScore );
                        if( Random.Range (0, 100) < forestChance) 
                        {
	                        h.Feature = features["Forest"];
                        }
                    }
                }
                else if(h.Biome.Name == "Water" || h.Biome.Name == "Ocean") {
                    if(h.Elevation > -0.8) {
                        if(Random.Range (0, 100) > ((h.Elevation + 1f) * 5f + 3)) {
                            //TODO: Spawn a reef
                        }
                    }
                }

    			//Debug.Log("Hex " + column + " " + row + " : Moisture = " + h.Moisture + ", Temperature = " + h.Temperature + ", Elevation = " + h.Elevation + ", Terrain = " + h.MeshName + " " + h.TerrainName);
            }
    	}*/
    }

    void IdentifySeas()
    {
	    // A sea is a block of contiguous water tiles with at least one ocean tile (elevation < 0.7)
	    // Make a pass on every tile to propagate seas from ocean tiles. The leftovers will be lands or lakes.
	    foreach (Hex h in hexes)
	    {
		    if (h.Elevation < -0.7 && !h.IsSea)
		    {
			    SetAsSea(h);
		    }
	    }
    }

    void SetAsSea(Hex h)
    {
	    h.IsSea = true;
	    Hex[] neighbours = GetHexesWithinRangeOf(h, 1);
	    foreach (Hex neighbour in neighbours)
	    {
		    if (neighbour.Elevation < 0 && !neighbour.IsSea)
		    {
			    SetAsSea(neighbour);
		    }
	    }
    }

    bool ElevateArea(int q, int r, int range) {

    	float centerHeight = Random.Range(190, 200) / 100f;
    	float distanceRatio;
    	float elevation;
    	int variability;
    	int modifier;

    	Hex centerHex = GetHexAt(q, r);

    	if(centerHex.Elevation >= mountainElevation / 3f) {
    		// Already too high : let's pick somewhere else
    		return false;
    	}

    	Debug.Log(string.Format("Volcano of height {3} at {0} {1} - range {2}", q, r, range, centerHeight));

    	Hex[] areaHexes = GetHexesWithinRangeOf(centerHex, range);

    	foreach(Hex h in areaHexes) {

    		distanceRatio = Hex.Distance(centerHex, h) / range;
    		elevation = centerHeight * Mathf.Lerp( 1f, Random.Range(0, 15) / 100f,  distanceRatio );
            //Debug.Log("Elevation: " + elevation);

    		/*if(distanceRatio < 0.5) {
    			variability = (int)((0.25f - Mathf.Abs(distanceRatio - 0.25f)) * 4f * 0.3f * 100f);
    			modifier = Random.Range(-variability, variability);
    		}
    		else {
    			variability = (int)((0.25f - Mathf.Abs(distanceRatio - 0.75f)) * 4f * 0.3f * 100f);
    			modifier = Random.Range(0, variability * 2);
    		}*/
    		variability = (int)((0.5f - Mathf.Abs(distanceRatio - 0.5f)) * 2f * 0.3f * 100f);
    		modifier = Random.Range(-variability, variability);
    		//Debug.Log("Variability : " + variability + ", distanceRatio : " + distanceRatio);

    		elevation += ((float)(modifier) / 100f);

    		// Diminish elevation given if the hex is already higher than sea, to nerf stacked mountain ranges
    		if(h.Elevation > 0) {
    			elevation = Mathf.Lerp( elevation, elevation * 0.2f, Mathf.Min(elevation, 1) );
    		}
    		h.Elevation += elevation;
            
    	}
    	
    	return true;
    }

    void ValleysAndLakes()
    {
	    // Break mountain blobs with hill valleys or lakes.
	    foreach (Hex h in hexes)
	    {
		    if (h.Elevation >= mountainElevation)
		    {
			    int countMoutains = 0;
			    Hex[] neighbours = GetHexesWithinRangeOf(h, 1);
				    
			    foreach (Hex neighbour in neighbours)
			    {
				    if (neighbour.Elevation >= mountainElevation)
					    countMoutains++;
				    else
					    break;
			    }

			    if (countMoutains == neighbours.Length)
			    {
				    // This tile is a mountain surrounded by moutains: it can become a lake, a hill, or stay as it is
				    int random = Random.Range(1, 4);
				    if (random == 1)
				    {
					    h.Elevation = Random.Range(coastElevation / 2, 0f);
				    }
				    else if(random == 2)
				    {
					    h.Elevation = Random.Range(hillElevation, mountainElevation);
				    }
			    }
		    }
	    }
    }

    void CoastalTiles()
    {
	    // Make sure any water tile near land is coastal
	    foreach (Hex h in hexes)
	    {
		    if (h.Elevation <= coastElevation)
		    {
			    Hex[] neighbours = GetHexesWithinRangeOf(h, 1);

			    foreach (Hex neighbour in neighbours)
			    {
				    if (neighbour.Elevation >= 0f)
				    {
					    h.Elevation = Random.Range(coastElevation, coastElevation + neighbour.Elevation);
					    break;
				    }
			    }
		    }
	    }
    }
}