using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using QPath;

public class HexMap : MonoBehaviour, IQPathWorld
{
	public int numColumns = 80;
	public int numRows = 40;

    public GameObject HexPrefab;
    public GameObject PawnPrefab;
    public GameObject ScoutPrefab;
    public GameObject CityPrefab;
    public GameObject RiverBranchPrefab;
    public GameObject TerritoryBorderPrefab;
    public GameObject FogPrefab;
    public GameObject HighlightPrefab;
    // Features
    public GameObject ForestPrefab;
    public GameObject JunglePrefab;
    public GameObject ReefPrefab;
    // Resources
    public GameObject CoralPrefab;
    public GameObject FishPrefab;
    public GameObject FruitsPrefab;
    public GameObject GeyserPrefab;
    public GameObject HuntPrefab;
    public GameObject OasisPrefab;
    public GameObject SalarPrefab;
    
    //TODO: manage the buildings models in a dedicated controller
    public GameObject ConstructionPrefab;

    public Mesh MeshWater;
    public Mesh MeshPlain;
    public Mesh MeshHill;
    public Mesh MeshMountain;
    public Mesh MeshUnderground;

    public Material MatCoast;
    public Material MatLake;
    public Material MatOcean;
    public Material MatTropical;
    public Material MatSavanna;
    public Material MatDesert;
    public Material MatSteppe;
    public Material MatTemperate;
    public Material MatTaiga;
    public Material MatTundra;
    public Material MatUnexplored;

    protected Hex[,] hexes;
    private Dictionary<Hex, GameObject> hexToGameObjectMap;
    private Dictionary<GameObject, Hex> gameObjectToHexMap;
    protected Edge[,,,] edges; //[a.col, a.row, b.col, b.row]
    private Dictionary<People, List<GameObject>> territoryBorders;
    public Dictionary<string, Biome> Biomes;
    public Dictionary<string, Relief> Reliefs;
    protected Dictionary<string, Feature> features;
    protected Dictionary<string, Resource> resources;
    
    protected List<River> rivers;

    //DEPRECATED
    Pointer pointer;
    public GameObject PointerPrefab;

    public delegate void OnCityCreatedDelegate( City city );
    public event OnCityCreatedDelegate OnCityCreated;

    /// <summary>
    /// Instantiate the lists of objects necessary to generate maps (biomes, resources...)
    /// </summary>
    public void Instantiate()
    {
	    // Instantiate the terrain biomes
	    Biomes = new Dictionary<string, Biome>();
	    Biomes.Add("Coast", new Biome("Coast", 
		    new Yields(2, 2), 
		    MatCoast, 0.9f));
	    Biomes.Add("Lake", new Biome("Lake", 
		    new Yields(3, 1), 
		    MatLake));
	    Biomes.Add("Ocean", new Biome("Ocean", 
		    new Yields(), 
		    MatOcean, 0));
	    Biomes.Add("Tropical", new Biome("Tropical", 
		    new Yields(2, 1, 0, 1), 
		    MatTropical));
	    Biomes.Add("Savanna", new Biome("Savanna", 
		    new Yields(2, 1, 1), 
		    MatSavanna));
	    Biomes.Add("Desert", new Biome("Desert", 
		    new Yields(1, 2, 1), 
		    MatDesert));
	    Biomes.Add("Steppe", new Biome("Steppe", 
		    new Yields(1, 0, 2, 0, 1), 
		    MatSteppe));
	    Biomes.Add("Temperate", new Biome("Temperate", 
		    new Yields(2, 1, 0, 0, 1), 
		    MatTemperate));
	    Biomes.Add("Taiga", new Biome("Taiga", 
		    new Yields(2, 0, 1, 0, 1), 
		    MatTaiga));
	    Biomes.Add("Tundra", new Biome("Tundra", 
		    new Yields(1, 0, 2, 1), 
		    MatTundra));
	    
	    // Instantiate the terrain reliefs
	    Reliefs = new Dictionary<string, Relief>();
	    Reliefs.Add("Plain", new Relief("Plain", MeshPlain, MeshUnderground));
	    Reliefs.Add("Hill", new Relief("Hill", MeshHill, MeshUnderground, 1, 0.75f, false, 0.25f));
	    Reliefs.Add("Mountain", new Relief("Mountain", MeshMountain, MeshUnderground, 2, 0.5f, false, 0.5f));
	    Reliefs.Add("Water", new Relief("Water", MeshWater, MeshPlain, 0, 1, true, -0.2f, -0.1f, -1f));
	    
	    // Instantiate the terrain features
	    features = new Dictionary<string, Feature>();
	    features.Add("Forest", new Feature("Forest",
		    ForestPrefab, 1, 0.8f));
	    features.Add("Jungle", new Feature("Jungle",
		    JunglePrefab, 1, 0.8f));
	    features.Add("Reef", new Feature("Reef",
		    ReefPrefab, 1, 0.8f));
	    
	    // Instantiate the terrain resources
	    resources = new Dictionary<string, Resource>();
	    resources.Add("Coral", new Resource("Coral",
		    new Yields(0, 0, 0, 1, 1), 
		    new List<Biome>() { Biomes["Coast"] }, 
		    new List<Relief>() { Reliefs["Water"] }, 
		    hex => (int) (hex.Temperature * 8f - 2),
		    CoralPrefab, 1));
	    resources.Add("Fish", new Resource("Fish",
		    new Yields(1, 1), 
		    new List<Biome>() { Biomes["Coast"], Biomes["Lake"] }, 
		    new List<Relief>() { Reliefs["Water"] }, 
		    hex => (int) ((0.5f - Mathf.Abs(hex.Temperature - 0.5f)) * 4f + 5f),
		    FishPrefab));
	    resources.Add("Fruits", new Resource("Fruits",
		    new Yields(1, 0, 0, 0, 1), 
		    new List<Biome>() { Biomes["Temperate"], Biomes["Savanna"], Biomes["Tropical"] }, 
		    new List<Relief>() { Reliefs["Plain"] }, 
		    hex => (int) (hex.Moisture * 2f + 4),
		    FruitsPrefab, 1));
	    resources.Add("Geyser", new Resource("Geyser",
		    new Yields(0, 0, 0, 1, 1), 
		    new List<Biome>() { Biomes["Tundra"], Biomes["Taiga"], Biomes["Steppe"] }, 
		    new List<Relief>() { Reliefs["Plain"], Reliefs["Hill"] }, 
		    hex => (int) (hex.Moisture * 3f + 2),
		    GeyserPrefab));
	    resources.Add("Hunt", new Resource("Hunt",
		    new Yields(1, 0, 1), 
		    new List<Biome>() { Biomes["Temperate"], Biomes["Taiga"], Biomes["Steppe"], Biomes["Savanna"], Biomes["Tropical"] }, 
		    new List<Relief>() { Reliefs["Plain"], Reliefs["Hill"] }, 
		    hex => 4,
		    HuntPrefab));
	    resources.Add("Oasis", new Resource("Oasis",
		    new Yields(1, 1), 
		    new List<Biome>() { Biomes["Desert"] },
		    new List<Relief>() { Reliefs["Plain"] }, 
		    hex => (int) (hex.Moisture * 9f + 4f),
		    OasisPrefab));
	    resources.Add("Salar", new Resource("Salar",
		    new Yields(0, 1, 0, 1),
		    new List<Biome>() { Biomes["Desert"], Biomes["Steppe"] }, 
		    new List<Relief>() { Reliefs["Plain"] }, 
		    hex => (int) ((1f - hex.Moisture) * 6f + 1f),
		    SalarPrefab));
	    // Ajouter : Cenote
	    
    }

    virtual public void GenerateMap(int seed = 0) {
    	// Virtual : allow the method to be overriden by children

    	hexes = new Hex[numColumns, numRows];
        hexToGameObjectMap = new Dictionary<Hex, GameObject>();
        gameObjectToHexMap = new Dictionary<GameObject, Hex>();
        territoryBorders = new Dictionary<People, List<GameObject>>();

    	// Default generation : generate a map filled with water

    	for(int column = 0; column < numColumns; column ++) {

    		for(int row = 0; row < numRows; row ++) {

    			// Instantiate a Hex
    			Hex h = new Hex(this, column, row);

    			hexes[column, row] = h;

    			// Adapt the position to the camera
    			Vector3 pos = h.PositionFromCamera( 
                    Camera.main.transform.position, 
                    numRows, 
                    numColumns 
				);

    			// Create the GameObject on the map
    			GameObject hexGO = (GameObject)Instantiate(
    				HexPrefab, 
    				pos,
    				Quaternion.identity,
    				this.transform
    			);

    			h.GO = hexGO;
    			h.HexMap = this;

                hexToGameObjectMap[h] = hexGO;
                gameObjectToHexMap[hexGO] = h;

    			string labelCoord = string.Format("{0},{1}", column, row);
    			hexGO.name = string.Concat("Hex ", labelCoord);
    			//hexGO.GetComponentInChildren<TextMesh>().text = labelCoord;

    			hexGO.GetComponent<HexBehaviour>().Hex = h;
    			hexGO.GetComponent<HexBehaviour>().HexMap = this;

            }
    	}
        
        // Generate edges
        // We store the edges in the array by the coordinates of the two hexes
        edges = new Edge[numColumns, numRows, numColumns, numRows];
        
        for(int column = 0; column < numColumns; column ++) {

	        for(int row = 0; row < numRows; row ++) {
		        
		        Hex a = GetHexAt(column, row);
		        
		        // Hexes can instantiate three edges with neighbours: column + 1, row + 1, column + 1 row - 1
		        if (row < numRows - 1)
		        {
			        Hex b = GetHexAt(column, row + 1);
	                edges[a.Q, a.R, b.Q, b.R] = new Edge(this, a, b, 'q');
		        }

		        if (column < numColumns - 1)
		        {
			        Hex b = GetHexAt(column + 1, row);
			        edges[a.Q, a.R, b.Q, b.R] = new Edge(this, a, b, 'r');
		        }

		        if (row > 0)
		        {
			        Hex b = GetHexAt(column + 1, row - 1);
			        edges[a.Q, a.R, b.Q, b.R] = new Edge(this, a, b, 's');
		        }
	        }
	        
	        rivers = new List<River>();
        }
    }

    public void DestroyMap()
    {
	    if (hexes.Length == 0)
		    return;
	    foreach (Hex hex in hexes)
	    {
		    Destroy(hexToGameObjectMap[hex]);
	    }
    }

    public IQPathTile GetTileAt(int x, int y) {
        return GetHexAt(x, y);
    }
    public Hex GetHexAt(int q, int r) {

    	if(hexes == null) {

    		Debug.LogError("Hexes array not instantiated!");
    		return null;
    	}

		if(r >= numRows || r < 0) {
			//Too high : no hex to select
			return null;
		}

		if(q >= numColumns || q < 0) {
			//We loop around the edge
			q = Mathf.Abs(Mathf.Abs(q) - numColumns);
		}

    	return hexes[q, r];
    }

    public List<Hex> GetAllHexes()
    {
	    // Maybe this method won't be necessary later
	    List<Hex> hexList = new List<Hex>();
	    foreach (Hex h in hexes)
	    {
		    hexList.Add(h);
	    }
	    return hexList;
    }

    public Edge GetEdgeAt(int qA, int rA, int qB, int rB)
    {
	    if (qA < 0)
		    qA = numColumns + qA;
	    if (qB < 0)
		    qB = numColumns + qB;
	    if (qA >= numColumns)
		    qA -= numColumns;
	    if (qB >= numColumns)
		    qB -= numColumns;

	    if (rA < 0 || rA >= numRows || rB < 0 || rB >= numRows)
	    {
		    //Debug.Log("Trying to get an illegal edge at " + qA + ", " + rA + " / " + qB + ", " + rB);
		    return null;
	    }

	    return edges[qA, rA, qB, rB];
    }
    
    
    /*** VISUAL RENDERING ***/
    
    public void DrawRiver(River river)
    {
	    //Vector3[] positions = new Vector3[river.Path.Length + 1];
	    List<Vector3> positions = new List<Vector3>();

	    Vector3[] prevPos = new Vector3[2];
	    Vector3[] edgePos = river.Source.Positions(new Vector3(0, 0.05f, 0));
	    int i = 0;
	    
	    foreach (Edge edge in river.Path)
	    {
		    prevPos = edgePos;
		    //Debug.Log("Edge " + edge);
		    i++;
		    if (i == 1)
		    {
			    // First iteration: skip the source to compare it next turn
			    continue;
		    }
		    
		    // Find the position difference between the two hexes in game world
		    Vector3 hexDif = Hex.WorldDistance(river.Source.HexA, edge.HexA);
		    //Debug.Log("Position of " + edge.HexA + " : " + edge.HexA.Position() + ". Position of " + edge.HexB + " : " + edge.HexB.Position() + ". Dif :" + hexDif);
		    
		    // Get the positions of this edge, offset by the hexDif and some height
		    edgePos = edge.Positions(new Vector3(0, 0.05f, 0) + hexDif);

		    if (i == 2)
		    {
			    // On second turn, draw the source edge: compare it and its next edge in order to find the origin of the river
			    if (Vector3.Distance(prevPos[1], edgePos[0]) < 0.01 || Vector3.Distance(prevPos[1], edgePos[1]) < 0.01)
			    {
				    positions.Add(prevPos[0]);
				    positions.Add(prevPos[1]);
			    }
			    else
			    {
				    positions.Add(prevPos[1]);
				    positions.Add(prevPos[0]);
			    }
		    }

		    if (edge.HexA.Elevation < 0 || edge.HexB.Elevation < 0)
		    {
			    // It's a lake: complete the current lineRenderer and start a new one
			    if (positions.Count > 1)	
			    {
				    RenderRiver(positions.ToArray(), river);
			    }
			    positions = new List<Vector3>();
		    }
		    
		    // Only add the coordinates that haven't been previously registered
		    if (Vector3.Distance(prevPos[0], edgePos[0]) < 0.01 || Vector3.Distance(prevPos[1], edgePos[0]) < 0.01)
		    {
			    positions.Add(edgePos[1]);
		    }
		    else
		    {
			    positions.Add(edgePos[0]);
		    }
	    }

	    // Very specific case of a river branch having just one non-lake edge before its end: we add the lacking edge position
	    if (positions.Count == 1 && river.Path.Last().HexA.Elevation > 0 && river.Path.Last().HexB.Elevation > 0)
	    {
		    if (Vector3.Distance(prevPos[0], edgePos[0]) < 0.01 || Vector3.Distance(prevPos[1], edgePos[0]) < 0.01)
		    {
			    positions.Add(edgePos[0]);
		    }
		    else
		    {
			    positions.Add(edgePos[1]);
		    }
	    }

	    RenderRiver(positions.ToArray(), river);
    }

    public void RenderRiver(Vector3[] positions, River river)
    {
	    GameObject go = (GameObject) Instantiate(RiverBranchPrefab, river.Source.HexA.GO.transform.position, Quaternion.identity,
		    river.Source.HexA.GO.transform);
	    LineRenderer lineRenderer = go.GetComponentInChildren<LineRenderer>();
	    
	    lineRenderer.positionCount = positions.Length;
	    lineRenderer.SetPositions(positions);
    }

    public void UpdateHexesVisuals() {

    	for(int column = 0; column < numColumns; column ++) {

    		for(int row = 0; row < numRows; row ++) {

    			Hex h = hexes[column, row];
                UpdateHexVisuals(h);
            }
    	}
    }

    public void UpdateHexVisuals(Hex h)
    {
	    GameObject surface = h.GO.transform.Find("Surface").gameObject;
	    MeshFilter surfaceMeshFilter = surface.GetComponentInChildren<MeshFilter>();
        MeshRenderer surfaceMeshRenderer = surface.GetComponentInChildren<MeshRenderer>();
        
        GameObject underground = h.GO.transform.Find("Underground").gameObject;
        MeshFilter undergroundMeshFilter = underground.GetComponentInChildren<MeshFilter>();
        
        if (GameController.Main.Omniscience || GameController.Main.CurrentPeople().HasExploredHex(h))
        {
	        if (h.IsVisible == true) // The hex was already visible, no need to update it
		        return;
		        
            //Render relief
            surfaceMeshFilter.mesh = h.Relief.MeshSurface;
            undergroundMeshFilter.mesh = h.Relief.MeshUnderground;
            surface.transform.localPosition = new Vector3(0, h.Relief.SurfaceY, 0);
            underground.transform.localPosition = new Vector3(0, h.Relief.UndergroundY, 0);

            //Render material (biome)
            surfaceMeshRenderer.material = h.Biome.Material;

            //Render feature
            if (h.Feature != null)
            {
                Vector3 p = h.GO.transform.position;
                p.y += h.Relief.ObjectY;

                GameObject go = GameObject.Instantiate(h.Feature.Prefab, p, Quaternion.identity, h.GO.transform);
                go.name = "Feature";
            }

            //Render resource
            if (h.Resource != null)
            {
	            Vector3 p = h.GO.transform.position;
	            p.y += h.Relief.ObjectY;

	            Quaternion r = Quaternion.Euler(0, h.ResourceRotation, 0);

	            GameObject go = GameObject.Instantiate(h.Resource.Prefab, p, r, h.GO.transform);
	            go.name = "Resource";
            }
            
            // Remove the fog and make the underground visible
            if (h.IsVisible == false)
            {
	            GameObject fog = h.GO.transform.Find("Fog").gameObject;
	            Destroy(fog);
	            underground.SetActive(true);
            }

            // Remember the visibility
            h.IsVisible = true;
        }
        else
        {
	        //The current player hasn't explored this hex
	        
	        if (h.IsVisible == false) // The hex was already hidden, no need to update it
		        return;
	        
            surfaceMeshFilter.mesh = MeshPlain;
            surfaceMeshRenderer.material = MatUnexplored;
            
            // Hide the underground
            underground.SetActive(false);
            
            // Destroy the feature
            Transform feature = h.GO.transform.Find("Feature");
            if(feature != null)
	            Destroy(feature.gameObject);
            
            // Destroy the resource
            Transform resource = h.GO.transform.Find("Resource");
            if(resource != null)
	            Destroy(resource.gameObject);
            
            // Spawn the fog
            GameObject go = GameObject.Instantiate(FogPrefab, h.GO.transform.position, Quaternion.identity, h.GO.transform);
            go.name = "Fog";
            
            // Remember the visibility
            h.IsVisible = false;
        }
        
        // Update the Yields UI
        GameController.Main.MapUIController.ShowYields(h);
    }

    public void HighlightSearchableHexes(People people)
    {
	    foreach (Hex hex in people.ExploredHexes)
	    {
		    if (hex.CanBeSearched(people))
		    {
			    hexToGameObjectMap[hex].transform.Find("Highlight").gameObject.SetActive(true);
		    }
		    else
		    {
			    hexToGameObjectMap[hex].transform.Find("Highlight").gameObject.SetActive(false);
		    }
	    }
    }
    public void UnhighlightSearchableHexes()
    {
	    foreach (KeyValuePair<Hex, GameObject> kv in hexToGameObjectMap)
	    {
		    kv.Value.transform.Find("Highlight").gameObject.SetActive(false);
	    }
    }

    public void SearchHex(Hex hex)
    {
	    GameController.Main.CurrentPeople().SearchHex(hex);
	    HighlightSearchableHexes(GameController.Main.CurrentPeople());
    }

    public Hex[] GetHexesWithinRangeOf(Hex centerHex, int range) {

    	List<Hex> results = new List<Hex>();

    	for(int dx = -range; dx <= range; dx++) {

    		for(int dy = Mathf.Max(-range, -dx - range); dy <= Mathf.Min(range, -dx + range); dy++) {

    			int row = centerHex.R + dy;
    			int column = centerHex.Q + dx;
    			//Debug.Log(string.Format("{0} {1}", column, row));

    			Hex h = GetHexAt(column, row);

    			if(h != null)
    				results.Add(h);
    		}
    	}

    	return results.ToArray();
    }
    
    public Hex[] GetHexesAtDistanceOf(Hex centerHex, int distance) {

	    List<Hex> results = new List<Hex>();
	    Hex[] hexesWithinRange = GetHexesWithinRangeOf(centerHex, distance);

	    foreach (Hex hex in hexesWithinRange)
	    {
		    if(Hex.Distance(centerHex, hex) == distance)
		    {
			    results.Add(hex);
		    }
	    }
	    
	    return results.ToArray();
    }

    public void SpawnUnitAt(string type, GameObject prefab, People people, Hex hex ) {

        GameObject go = Instantiate( prefab, hex.GO.transform.position, Quaternion.identity, hex.GO.transform );

        Unit unit = new Unit(type, hex, go, GameController.Main.CurrentPeople(), 3, true, false);

        people.Units.Add(unit);
        hex.AddUnit(unit);

        // Add the UnitView.OnMove function to the unit.OnMove event
        unit.OnMove += go.GetComponent<UnitBehaviour>().OnMove;
    }
    public bool SpawnCityAt(GameObject prefab, People people, Hex hex) {

	    if (hex.city != null)
	    {
		    Debug.Log("Impossible to build a new city: there is already one here!");
		    return false;
	    }

	    Vector3 position = hex.GO.transform.position;
	    position.y += hex.Relief.ObjectY;
	    GameObject go = Instantiate(prefab, position, Quaternion.identity,
		    hex.GO.transform);

	    City city = new City(hex, people, go);

	    people.Cities.Add(city);
	    hex.AddCity(city);
	    
	    if (OnCityCreated != null)
		    OnCityCreated(city);
	    
	    return true;
    }

    public bool SpawnPeopleAt(People people, Hex hex)
    {
	    territoryBorders.Add(people, new List<GameObject>());
	    
	    // Scenario where we spawn a city
	    SpawnCityAt(CityPrefab, people, hex);
	    
	    people.TerritoryAdd(hex);
	    foreach (Hex h in hex.GetNeighbours())
	    {
		    people.TerritoryAdd(h);
	    }
	    DrawTerritory(people);
	    
	    // Scenario where we spawn scouts
	    /* 
	    SpawnUnitAt("Scout", ScoutPrefab, hex);
	    */
	    
	    UpdateHexesVisuals();

	    return true;
    }

    public void DrawTerritory(People people)
    {
	    // Cleanup previous territory
	    if (territoryBorders[people].Count > 0)
	    {
		    foreach (GameObject go in territoryBorders[people])
		    {
			    Destroy(go);
		    }
	    }
	    
	    List<Edge> testedEdges = new List<Edge>();
	    
	    foreach (Hex hex in people.Territory)
	    {
		    // Let's start by giving it the TerritoryOverlay in the good color
		    GameObject overlay = hex.GO.transform.Find("TerritoryOverlay").gameObject;
		    overlay.SetActive(true);
		    Color overlayColor = people.Color;
		    overlayColor.a = 0.15f;
		    overlay.GetComponent<Renderer>().material.color = overlayColor;
		    
		    // Now, let's create the territory borders
		    //Debug.Log("Testing Hex " + hex);
		    List<Edge> edges = hex.GetEdges();
		    foreach (Edge edge in edges)
		    {
			    //Debug.Log("Testing Edge " + edge);
			    if (testedEdges.Contains(edge))
			    {
				    //Debug.Log("Already tested, skip");
				    continue;
			    }

			    if (!people.Territory.Contains(edge.HexA) || !people.Territory.Contains(edge.HexB))
			    {
				    //Debug.Log("Border between " + edge.HexA + " and " + edge.HexB);
				    GameObject go = Instantiate(TerritoryBorderPrefab, hex.GO.transform.position,
					    Quaternion.identity, hex.GO.transform);
				    go.name = "Border of " + people + " " + edge.HexA + " - " + edge.HexB;
				    territoryBorders[people].Add(go);
				    go.transform.rotation = edge.Angle(hex);
				    go.GetComponentInChildren<Renderer>().material.color = people.Color;
			    }
			    
			    testedEdges.Add(edge);
		    }
	    }
    }

    public Hex GetHexFromGameObject(GameObject hexGo)
    {
	    if (gameObjectToHexMap.ContainsKey(hexGo))
	    {
		    return gameObjectToHexMap[hexGo];
	    }

	    return null;
    }
    public GameObject GetGameObjectFromHex(Hex hex)
    {
	    if (hexToGameObjectMap.ContainsKey(hex))
	    {
		    return hexToGameObjectMap[hex];
	    }

	    return null;
    }

    protected void LogStats()
    {
	    int tileCount = 0;
	    int landTiles = 0;
	    int waterTiles = 0;
	    float totalMoisture = 0f;
	    Dictionary<string, int> biomesCount = new Dictionary<string, int>();
	    Dictionary<string, int> reliefsCount = new Dictionary<string, int>();
	    Dictionary<string, int> resourcesCount = new Dictionary<string, int>();
	    
	    foreach (Hex h in hexes)
	    {
		    tileCount++;
		    
		    if(!biomesCount.ContainsKey(h.Biome.Name))
			    biomesCount.Add(h.Biome.Name, 0);
		    biomesCount[h.Biome.Name]++;
		    
		    if(!reliefsCount.ContainsKey(h.Relief.Name))
			    reliefsCount.Add(h.Relief.Name, 0);
		    reliefsCount[h.Relief.Name]++;

		    if (h.Resource != null)
		    {
			    if (!resourcesCount.ContainsKey(h.Resource.Name))
				    resourcesCount.Add(h.Resource.Name, 0);
			    resourcesCount[h.Resource.Name]++;
		    }

		    if (h.Elevation > 0)
		    {
			    totalMoisture += h.Moisture;
			    landTiles++;
		    }
		    else
		    {
			    waterTiles++;
		    }
	    }

	    Debug.Log("--- Map Stats ---");
	    Debug.Log("Average Moisture: " + totalMoisture / landTiles);
	    foreach (KeyValuePair<string, int> entry in biomesCount)
	    {
		    Debug.Log(entry.Key + ": " + Math.Round((float)entry.Value / tileCount * 100f, 1) + "% (" + Math.Round((float)entry.Value / landTiles * 100f, 1) + "% of land)");
	    }
	    foreach (KeyValuePair<string, int> entry in reliefsCount)
	    {
		    Debug.Log(entry.Key + ": " + Math.Round((float)entry.Value / tileCount * 100f, 1) + "% (" + Math.Round((float)entry.Value / landTiles * 100f, 1) + "% of land)");
	    }
	    foreach (KeyValuePair<string, int> entry in resourcesCount)
	    {
		    Debug.Log(entry.Key + ": " + entry.Value);
	    }
    }
}
