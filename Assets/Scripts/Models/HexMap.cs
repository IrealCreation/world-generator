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
    public GameObject ForestPrefab;
    public GameObject JunglePrefab;
    public GameObject ReefPrefab;
    public GameObject PawnPrefab;
    public GameObject ScoutPrefab;
    public GameObject CityPrefab;
    public GameObject RiverBranchPrefab;
    public GameObject TerritoryBorderPrefab;
    public GameObject FogPrefab;

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
    protected Dictionary<string, Biome> biomes;
    protected Dictionary<string, Feature> features;
    protected Dictionary<string, Relief> reliefs;
    
    protected List<River> rivers;
    private List<Unit> units; // TODO: put it in People
    private List<City> cities; // TODO: put it in People
    private List<People> peoples;
    private int currentPeopleID;

    //DEPRECATED
    Pointer pointer;
    public GameObject PointerPrefab;

    public delegate void OnCityCreatedDelegate( City city );
    public event OnCityCreatedDelegate OnCityCreated;

    public InputController inputController;

    // Start is called before the first frame update
    void Start()
    {
	    // Instantiate the terrain biomes
	    biomes = new Dictionary<string, Biome>();
	    biomes.Add("Coast", new Biome("Coast", 
		    new Dictionary<string, float>(){ {"food", 3}, {"wealth", 1} }, 
		    MatCoast));
	    biomes.Add("Lake", new Biome("Lake", 
		    new Dictionary<string, float>(){ {"food", 3}, {"wealth", 1} }, 
		    MatLake));
	    biomes.Add("Ocean", new Biome("Ocean", 
		    new Dictionary<string, float>(), 
		    MatOcean));
	    biomes.Add("Tropical", new Biome("Tropical", 
		    new Dictionary<string, float>(){ {"food", 2}, {"wealth", 1}, {"culture", 1} }, 
		    MatTropical));
	    biomes.Add("Savanna", new Biome("Savanna", 
		    new Dictionary<string, float>(){ {"food", 2}, {"military", 1}, {"wealth", 1} }, 
		    MatSavanna));
	    biomes.Add("Desert", new Biome("Desert", 
		    new Dictionary<string, float>(){ {"food", 1}, {"military", 1}, {"wealth", 2} }, 
		    MatDesert));
	    biomes.Add("Steppe", new Biome("Steppe", 
		    new Dictionary<string, float>(){ {"food", 1}, {"military", 2}, {"culture", 1} }, 
		    MatSteppe));
	    biomes.Add("Temperate", new Biome("Temperate", 
		    new Dictionary<string, float>(){ {"food", 2}, {"wealth", 2} }, 
		    MatTemperate));
	    biomes.Add("Taiga", new Biome("Taiga", 
		    new Dictionary<string, float>(){ {"food", 2}, {"military", 1}, {"culture", 1} }, 
		    MatTaiga));
	    biomes.Add("Tundra", new Biome("Tundra", 
		    new Dictionary<string, float>(){ {"food", 1}, {"military", 1}, {"science", 2} }, 
		    MatTundra));
	    
	    // Instantiate the terrain reliefs
	    reliefs = new Dictionary<string, Relief>();
	    reliefs.Add("Plain", new Relief("Plain", MeshPlain, MeshUnderground));
	    reliefs.Add("Hill", new Relief("Hill", MeshHill, MeshUnderground, 1, false, 0.25f));
	    reliefs.Add("Mountain", new Relief("Mountain", MeshMountain, MeshUnderground, 2, false, 0.5f));
	    reliefs.Add("Water", new Relief("Water", MeshWater, MeshPlain, 0, true, -0.2f, -0.1f, -1f));
	    
	    // Instantiate the terrain features
	    features = new Dictionary<string, Feature>();
	    features.Add("Forest", new Feature("Forest",
		    new Dictionary<string, float>(){ {"food", 1}, {"wealth", 1} },
		    ForestPrefab, 1));
	    features.Add("Jungle", new Feature("Jungle",
		    new Dictionary<string, float>(){ {"food", 1}, {"science", 1} },
		    JunglePrefab, 1));
	    features.Add("Reef", new Feature("Reef",
		    new Dictionary<string, float>(){ {"military", 2} },
		    ReefPrefab, 1));
	    features.Add("Coral", new Feature("Coral",
		    new Dictionary<string, float>(){ {"science", 1}, {"culture", 1} },
		    ReefPrefab, 1));
	    
	    StartGame();
	    
	    // Instantiate the StructurePrototypes
	    new StructurePrototype("Hunters camp",
		    new string[] {"Tundra", "Taiga", "Temperate", "Steppe", "Savanna", "Tropical", "Desert"},
		    new Dictionary<string, float>(){ {"food", 0.75f}, {"materials", 0.75f}, {"military", 0.5f} },
		    new Dictionary<string, int>() { {"materials", 5} }
		    );
	    new StructurePrototype("Gatherers lodge",
		    new string[] {"Taiga", "Temperate", "Steppe", "Savanna", "Tropical"},
		    new Dictionary<string, float>(){ {"food", 1}, {"materials", 1} },
		    new Dictionary<string, int>() { {"materials", 5} }
	    );
    }

    void Update() {
	    
    }

    public void StartGame(int seed = 0)
    {
	    // Create empty dictionaries
	    peoples = new List<People>();
	    territoryBorders = new Dictionary<People, List<GameObject>>();
	    
	    // Generate the map
	    if(seed != 0)
			GenerateMap(seed);
	    else
		    GenerateMap();
        
	    // Create the peoples
	    currentPeopleID = 0;
	    //SpawnPeopleAt("Testeurs", true, GetHexAt(20, 20));

	    // Update hex visuals to match data
	    UpdateHexesVisuals();
    }

    public void NextTurn() {
	    
	    //TODO: render map depending on CurrentPeople (see UpdateHexVisuals)
	    
	    inputController.SelectedUnit = null;
	    inputController.SelectedCity = null;

        if(this.units != null) {

            foreach(Unit u in this.units) {

                u.StartTurn();
            }
            
            foreach(Unit u in this.units) {

	            u.EndTurn();
            }
        }
    }

    virtual public void GenerateMap(int seed = 0) {
    	// Virtual : allow the method to be overriden by children

    	hexes = new Hex[numColumns, numRows];
        hexToGameObjectMap = new Dictionary<Hex, GameObject>();
        gameObjectToHexMap = new Dictionary<GameObject, Hex>();

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

    			h.hexGO = hexGO;
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
        
        //UpdateHexVisuals(); //Called by HexMap_Continent

        // Put the pointer on the map
        /*this.pointer = new Pointer();

        GameObject PointerGO = (GameObject)Instantiate(
            PointerPrefab, 
            hexes[numColumns / 2, numRows / 2].hexGO.transform.position,
            Quaternion.identity,
            hexes[numColumns / 2, numRows / 2].hexGO.transform
        );

        this.pointer.GO = PointerGO;*/

        //SpawnUnitAt("pawn", PawnPrefab, hexes[numColumns / 2, numRows / 2]);
    }

    public void ResetMap(int seed = 0)
    {
	    foreach (Hex hex in hexes)
	    {
		    Destroy(hexToGameObjectMap[hex]);
	    }
	    StartGame(seed);
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

    public People CurrentPeople()
    {
	    return peoples[currentPeopleID];
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
	    GameObject go = (GameObject) Instantiate(RiverBranchPrefab, river.Source.HexA.hexGO.transform.position, Quaternion.identity,
		    river.Source.HexA.hexGO.transform);
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
	    GameObject surface = h.hexGO.transform.Find("Surface").gameObject;
	    MeshFilter surfaceMeshFilter = surface.GetComponentInChildren<MeshFilter>();
        MeshRenderer surfaceMeshRenderer = surface.GetComponentInChildren<MeshRenderer>();
        
        GameObject underground = h.hexGO.transform.Find("Underground").gameObject;
        MeshFilter undergroundMeshFilter = underground.GetComponentInChildren<MeshFilter>();
        
        if (peoples.Count == 0 || CurrentPeople().HasExploredHex(h))
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

            //Render features (forests, jungle, reef...)
            if (h.Feature != null)
            {
                Vector3 p = h.hexGO.transform.position;
                p.y += h.Relief.ObjectY;

                GameObject go = GameObject.Instantiate(h.Feature.Prefab, p, Quaternion.identity, h.hexGO.transform);
                go.name = "Feature";
            }
            
            // Remove the fog and make the underground visible
            if (h.IsVisible == false)
            {
	            GameObject fog = h.hexGO.transform.Find("Fog").gameObject;
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
            Transform feature = h.hexGO.transform.Find("Feature");
            if(feature != null)
	            Destroy(feature.gameObject);
            
            // Spawn the fog
            GameObject go = GameObject.Instantiate(FogPrefab, h.hexGO.transform.position, Quaternion.identity, h.hexGO.transform);
            go.name = "Fog";
            
            // Remember the visibility
            h.IsVisible = false;
        }
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
		    if((int)Hex.Distance(centerHex, hex) == distance)
		    {
			    results.Add(hex);
		    }
	    }
	    
	    return results.ToArray();
    }

    public void SpawnUnitAt( string type, GameObject prefab, Hex hex ) {

        if(this.units == null) {
            this.units = new List<Unit>();
        }

        GameObject go = Instantiate( prefab, hex.hexGO.transform.position, Quaternion.identity, hex.hexGO.transform );

        Unit unit = new Unit(type, hex, go, CurrentPeople());

        units.Add(unit);
        hex.AddUnit(unit);

        // Add the UnitView.OnMove function to the unit.OnMove event
        unit.OnMove += go.GetComponent<UnitBehaviour>().OnMove;
    }
    public bool SpawnCityAt(GameObject prefab, People people, Hex hex) {

	    if(this.cities == null) {
		    this.cities = new List<City>();
	    }

	    if (hex.city != null)
	    {
		    Debug.Log("Impossible to build a new city: there is already one here!");
		    return false;
	    }
	    
	    GameObject go = Instantiate(prefab, hex.hexGO.transform.position, Quaternion.identity,
		    hex.hexGO.transform);

	    City city = new City(hex, people, go);

	    people.Cities.Add(city);
	    hex.AddCity(city);
	    
	    if (OnCityCreated != null)
		    OnCityCreated(city);
	    
	    return true;
    }

    public bool SpawnPeopleAt(string name, bool isPlayer, Hex hex)
    {
	    People people = new People(name, isPlayer, this, hex);
	    peoples.Add(people);
	    territoryBorders.Add(people, new List<GameObject>());
	    
	    /*
	    // Scenario where we spawn a city
	    SpawnCityAt(CityPrefab, people, hex);
	    
	    people.TerritoryAdd(hex);
	    foreach (Hex h in hex.GetNeighbours())
	    {
		    people.TerritoryAdd(h);
	    }
	    DrawTerritory(people);
	    */
	    
	    // Scenario where we spawn scouts
	    SpawnUnitAt("Scout", ScoutPrefab, hex);
	    
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
				    GameObject go = Instantiate(TerritoryBorderPrefab, hex.hexGO.transform.position,
					    Quaternion.identity, hex.hexGO.transform);
				    go.name = "Border of " + people + " " + edge.HexA + " - " + edge.HexB;
				    territoryBorders[people].Add(go);
				    go.transform.rotation = edge.Angle(hex);
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
}
