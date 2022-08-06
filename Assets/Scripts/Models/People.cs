using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// A people is the equivalent of an empire, or civilization, ruled by a player or by the computer,
/// existing over a territory, producing resources, making choices...
/// </summary>
public class People
{
    public readonly HexMap HexMap;
    public readonly string Name;
    public bool IsPlayer;
    public Color Color;

    public Yields YieldsTotal; // Total of yields produced by this people
    public Yields YieldsLastTurn;
    public float ExpansionScore; // Increase each turn until it reaches a treshold, triggering a border expansion
    public float Expansionism; // The increase of ExpansionScore each turn
    public int Openness; // The degree to which this people is open to interactions with other (100: fully open, -100: fully isolationist)
    public int Warmongering; // The degree of agressivity and willingness to wage war of this people (100: fully warmonger, -100: fully pacifist)
    public int Progressivism; // The degree to which this people is willing to try new politics and ideas (100: fully progressive, -100: fully conservative)
    public Dictionary<string, int> Interests;
    public Dictionary<string, int> Jobs; // Job repartitions (get the percentage with GetJobs())
    public Dictionary<string, string> JobNames;
    public Dictionary<string, int> Ethos;
    public Dictionary<string, int> Adaptation;
    public Dictionary<Choice, ChoiceOption> ChoicesMade;

    // Expansion
    public int ExpansionFoodStock;
    public int ExpansionMilitaryStock;
    public int ExpansionMilitaryCost;
    public int ExpansionCultureStock;
    public int ExpansionCultureCost;
    // Economy
    public List<Building> BuildingsAvailable;
    public BuildingProject BuildingProject;
    public BuildingProject BuildingProjectLast;
    public int ConstructionOverflow; // Excess of wealth from last building project
    public int BuildingsCount;
    public int WondersCount;
    // Research
    public int ResearchScienceStock;
    public HashSet<Research> ResearchCompleted;
    public Research ResearchCurrent;
    public int ResearchEraIndex;
    public int ResearchesCompletedInEra;
    // Culture
    public int ArtworkCultureStock;
    public int ArtworkCost;
    public List<Artwork> Artworks;
    public ArtMovement ArtMovement; // Currently adopted ArtMovement
    public List<ArtSubject> ArtSubjects;
    public List<string> ArtForms; // List of the three latest forms of art (painting, sculpture...) that can be picked by art movements
    // Military
    // public Tactic Tactic;
    public Dictionary<string, int> Strategies;

    public HashSet<Hex> ExploredHexes { get; protected set; }
    public HashSet<Hex> SearchedHexes { get; protected set; } // Hexes already searched by a scout, giving resources
    public HashSet<Hex> Territory { get; private set; }
    public Hex Capital;
    public List<City> Cities; // Not sure we keep the cities, but I put it there in the meantime
    public List<Unit> Units;

    public People(string name, Color color, bool isPlayer, HexMap hexMap, Hex startingHex)
    {
        Name = name;
        IsPlayer = isPlayer;
        HexMap = hexMap;
        Territory = new HashSet<Hex>();
        Cities = new List<City>();
        Units = new List<Unit>();
        ChoicesMade = new Dictionary<Choice, ChoiceOption>();
        Color = color;

        Capital = startingHex;
        SearchedHexes = new HashSet<Hex>();
        ExploredHexes = new HashSet<Hex>();
        Explore(startingHex, 3);
        
        // Instantiate basic stats
        YieldsLastTurn = new Yields();
        YieldsTotal = new Yields();
        ExpansionScore = 0;
        Expansionism = 1;
        Openness = 0;
        Progressivism = 0;
        
        // Instantiate the interests
        Interests = new Dictionary<string, int>();
        Interests["food"] = 20; // "Growth"
        Interests["wealth"] = 20; // "Industry"
        Interests["military"] = 20; // "Militarism"
        Interests["science"] = 20; // "Curiosity"
        Interests["culture"] = 20; // "Inspiration"
        Interests["religion"] = 0; // "Faith"
        
        // Instantiate the jobs
        Jobs = new Dictionary<string, int>();
        Jobs["food"] = 20;
        Jobs["wealth"] = 20;
        Jobs["military"] = 20;
        Jobs["science"] = 20;
        Jobs["culture"] = 20;
        Jobs["religion"] = 0;
        
        // Instantiate the job names
        JobNames = new Dictionary<string, string>();
        JobNames["food"] = "Cueilleurs";
        JobNames["wealth"] = "Artisans";
        JobNames["military"] = "Chasseurs";
        JobNames["science"] = "Sages";
        JobNames["culture"] = "Conteurs";
        JobNames["religion"] = "Chamans";
        
        // Instantiate the political ethos
        Ethos = new Dictionary<string, int>();
        Ethos["equality"] = 0;
        Ethos["elitism"] = 0;
        Ethos["liberty"] = 0;
        Ethos["authority"] = 0;
        
        // Instantiate the military strategies
        Strategies = new Dictionary<string, int>();
        Strategies["mobility"] = 0;
        Strategies["defence"] = 0;
        Strategies["discipline"] = 0;
        Strategies["offence"] = 0;
        
        // Instantiate the projects variable
        ExpansionFoodStock = 0;
        ExpansionMilitaryStock = 0;
        ExpansionMilitaryCost = 300;
        ExpansionCultureStock = 0;
        ExpansionCultureCost = 300;
        BuildingsAvailable = GameController.Main.StartingBuildings;
        BuildingsCount = 0;
        WondersCount = 0;
        ResearchScienceStock = 0;
        ResearchCompleted = new HashSet<Research>();
        ResearchEraIndex = 0;
        ResearchesCompletedInEra = 0;
        ArtworkCultureStock = 0;
        ArtworkCost = 600;
        Artworks = new List<Artwork>();
        ArtSubjects = new List<ArtSubject>();
        ArtForms = new List<string>() {"Peinture rupestre", "Roche gravée", "Chant primordial"}; // Original art forms available
        
        // Instantiate the biomes and relief adaptation
        Adaptation = new Dictionary<string, int>();
        foreach (Biome biome in HexMap.Biomes.Values)
        {
            Adaptation.Add(biome.Name, 20);
        }
        foreach (Relief relief in HexMap.Reliefs.Values)
        {
            Adaptation.Add(relief.Name, 0);
        }
    }

    public void StartTurn()
    {
        BuildingsCount = 0;
        WondersCount = 0;
        
        // Start turns of units (give back movement points...)
        if(Units != null) {
            foreach(Unit u in Units) {
                u.StartTurn();
            }
        }
        
        // *** Jobs ***
        foreach (KeyValuePair<string, int> kv in Interests)
        {
            // The job share is inferior the interest value: let's increase it
            if (Jobs[kv.Key] < kv.Value)
            {
                // Between 1 and 2 of evolution, depending on progressivism, but clamped to the interest value
                Jobs[kv.Key] = Mathf.Min(Jobs[kv.Key] + 1 + (Progressivism + 100) / 200, kv.Value);
            }
            // The job share is superior to the interest value: let's decrease it
            else if (Jobs[kv.Key] > kv.Value)
            {
                // Between 1 and 2 of evolution, depending on progressivism, clamped between 0 and the interest value
                Jobs[kv.Key] = Mathf.Max(Jobs[kv.Key] - 1 - (Progressivism + 100) / 200, kv.Value);
            }
        }
        

        // *** Yields ***
        YieldsLastTurn = new Yields(
            (int)(GetJobs("food") * 500), 
            (int)(GetJobs("wealth") * 500), 
            (int)(GetJobs("military") * 500), 
            (int)(GetJobs("science") * 500), 
            (int)(GetJobs("culture") * 500));
        
        // Yields from hexes
        // foreach (Hex hex in Territory)
        // {
        //     YieldsLastTurn += hex.GetYields();
        // }
        
        YieldsTotal += YieldsLastTurn;

        // *** Expansion ***
        ExpansionFoodStock += YieldsLastTurn.Food;
        ExpansionMilitaryStock += YieldsLastTurn.Military;
        ExpansionCultureStock += YieldsLastTurn.Culture;
        // Try for the military expansion
        if (ExpansionFoodStock + ExpansionMilitaryStock >= ExpansionMilitaryCost)
        {
            // Remove from the food stock what we can't take from the military stock
            ExpansionFoodStock -= ExpansionMilitaryCost - ExpansionMilitaryStock;
            // Reset the military stock
            ExpansionMilitaryStock = 0;
            // Increase by 10% this expansion cost
            ExpansionMilitaryCost = (int)(1.1f * ExpansionMilitaryCost);
            // Expand the territory
            TerritoryAdd(TargetExpansion());
            // Create the notification
            new Notification(this, "Conquête d'un nouveau territoire neutre.");
        }
        // Try for the cultural expansion
        else if (ExpansionFoodStock + ExpansionCultureStock >= ExpansionCultureCost)
        {
            // Remove from the food stock what we can't take from the culture stock
            ExpansionFoodStock -= ExpansionCultureCost - ExpansionCultureStock;
            // Reset the culture stock
            ExpansionCultureStock = 0;
            // Increase by 10% this expansion cost
            ExpansionCultureCost = (int)(1.1f * ExpansionCultureCost);
            // Expand the territory
            TerritoryAdd(TargetExpansion());
            // Create the notification
            new Notification(this, "Assimilation d'un nouveau territoire neutre.");
        }


        // *** Science ***
        ResearchScienceStock += YieldsLastTurn.Science;
        
        // If we don't have an ongoing research, let's pick one
        if (ResearchCurrent == null)
        {
            ResearchCurrent = SelectResearch();
        }

        // If we have enough science, complete the research
        if (ResearchScienceStock > ResearchCurrent.GetCost())
        {
            CompleteResearch();
        }
        
        
        // *** Culture ***
        ArtworkCultureStock += YieldsLastTurn.Culture;
        if (ArtworkCultureStock >= ArtworkCost)
        {
            ArtworkCultureStock -= ArtworkCost;
            ArtworkCost = (int) (ArtworkCost * 1.2f);
            Artwork artwork = CreateArtwork();
            Artworks.Add(artwork);
            new Notification(this, "Nouvelle oeuvre d'art : " + artwork.ToString());
        }

        // *** Economy ***
        
        // Check if the current building project is still valid
        if (BuildingProject != null && !BuildingProject.IsValid(this))
            BuildingProject = null;
        
        // Remove all the already build wonders from our buildings list
        BuildingsAvailable.RemoveAll(building => building.WonderBuilt == true);
        
        // If we don't have any building project, let's make one
        if (BuildingProject == null)
        {
            BuildingProject = SelectBuildingProject();
            if(BuildingProject != null)
                new Notification(this, "Début de construction : " + BuildingProject);
        }

        // If we have a building project, let's build it
        if (BuildingProject != null)
        {
            int lastOverflow = ConstructionOverflow;
            ConstructionOverflow = BuildingProject.Build(YieldsLastTurn.Wealth + lastOverflow);
            if (ConstructionOverflow > 0)
            {
                BuildingProjectLast = BuildingProject;
                BuildingProject = null;
                // Notification sent in BuildingProject.Finish()
            }
        }
        
        // Count the number of buildings and wonders
        foreach (Hex hex in Territory)
        {
            if (hex.Building != null)
            {
                if (hex.Building.IsWonder)
                    WondersCount++;
                else
                    BuildingsCount++;
            }
        }
    }

    public void EndTurn() {
        
        if(Units != null) {
            
            foreach(Unit u in Units) {

                u.EndTurn();
            }
        }
    }

    public float GetJobs(string jobName)
    {
        int total = 0;
        foreach (int jobValue in Jobs.Values)
        {
            total += jobValue;
        }
        return (float) Jobs[jobName] / total;
    }

    public bool HasExploredHex(Hex h)
    {
        return ExploredHexes.Contains(h);
    }

    public bool HasSearchedHex(Hex h)
    {
        return SearchedHexes.Contains(h);
    }

    public void Explore(Hex h, int range = 0)
    {
        if (range > 0)
            ExploredHexes.UnionWith(HexMap.GetHexesWithinRangeOf(h, range));
        else
            ExploredHexes.Add(h);
        HexMap.UpdateHexesVisuals();
    }

    public void SearchHex(Hex h)
    {
        SearchedHexes.Add(h);
        HexMap.UpdateHexVisuals(h);
        GainYields(h.Resource.Yields);
    }

    /// <summary>
    /// Pick a valid hex for territory expansion
    /// </summary>
    public Hex TargetExpansion()
    {
        Dictionary<Hex, int> candidates = new Dictionary<Hex, int>();
        HashSet<River> rivers = new HashSet<River>();
        int scoreTotal = 0;
        
        // List owned rivers
        foreach (Hex ownedHex in Territory)
        {
            if(ownedHex.HasRiver())
                rivers.UnionWith(ownedHex.GetRivers());
        }

        // Find unowned neighbours of owned hexes
        foreach (Hex ownedHex in Territory)
        {
            if(ownedHex.HasRiver())
                rivers.UnionWith(ownedHex.GetRivers());
            
            foreach (Hex newHex in ownedHex.GetNeighbours())
            {
                if (newHex.People == null && !candidates.ContainsKey(newHex) && newHex.Biome.ExpansionModifier > 0)
                {
                    int score = Adaptation[newHex.Biome.Name] + Adaptation[newHex.Relief.Name];
                    
                    // Effects of biomes, relief and features
                    score = Mathf.RoundToInt(score * newHex.Biome.ExpansionModifier);
                    score = Mathf.RoundToInt(score * newHex.Relief.ExpansionModifier);
                    if(newHex.Feature != null)
                        score = Mathf.RoundToInt(score * newHex.Feature.ExpansionModifier);
                    
                    // Points for adjacent owned hexes 
                    int count = -1; // Start at -1 because there is one assured adjacent hex
                    foreach (Hex neighbour in newHex.GetNeighbours())
                    {
                        if (Territory.Contains(neighbour))
                            count += 1;
                    }
                    if (count == 5) // Every surrounding tile is in territory: we should really add it
                        score += 100; 
                    score += Mathf.RoundToInt((Adaptation[newHex.Biome.Name] + Adaptation[newHex.Relief.Name]) * count * 0.2f);
                    
                    // Malus points for distance with capital
                    score -= Hex.Distance(newHex, Capital) * 2;
            
                    // Bonus points for shared river
                    if (newHex.HasRiver())
                    {
                        if (rivers.Intersect(newHex.GetRivers()).Any())
                        {
                            // The candidate has a common river with the current territory: bonus points!
                            score += 20;
                        }
                    }
            
                    // Bonus points for resource
                    if (newHex.Resource != null)
                        score += 20;

                    if (score < 1)
                        score = 1;

                    // We ² it so prefered tiles are even more prefered
                    score *= score;
                    candidates.Add(newHex, score);
                    scoreTotal += score;
                }
            }
        }

        if (candidates.Count == 0)
            return null;
        
        /*
        // Order candidates by score
        var topCandidates = (from entry in candidates orderby entry.Value descending select entry.Key).Take(5);
        foreach (Hex hex in topCandidates)
        {
            // Each top candidate has a 75% chance to be selected, starting from top
            if (Random.Range(0, 100) < 75)
                return hex;
        }
        // If by chance no hex has been selected, let's just pick the top one
        return topCandidates.First();
         */
        return Utils.SelectByPref(candidates, scoreTotal);
    }

    public void TerritoryAdd(Hex h)
    {
        if (h == null)
            return;
        Territory.Add(h);
        h.People = this;
        Adaptation[h.Biome.Name] += 1;
        HexMap.DrawTerritory(this);
    }

    public void GainYields(Yields yields)
    {
        YieldsTotal += yields;
        GameController.Main.ScreenUIController.UpdateYieldsStock(YieldsTotal);
    }

    public void MakeChoice(Choice choice, ChoiceOption option)
    {
        ChoicesMade.Add(choice, option);
        
        Interests["culture"] += option.Bonuses.ContainsKey("culture") ? option.Bonuses["culture"] : 0;
        Interests["science"] += option.Bonuses.ContainsKey("science") ? option.Bonuses["science"] : 0;
        Interests["military"] += option.Bonuses.ContainsKey("military") ? option.Bonuses["military"] : 0;
        Interests["wealth"] += option.Bonuses.ContainsKey("wealth") ? option.Bonuses["wealth"] : 0;
        Interests["food"] += option.Bonuses.ContainsKey("food") ? option.Bonuses["food"] : 0;

        Ethos["equality"] += option.Bonuses.ContainsKey("equality") ? option.Bonuses["equality"] : 0;
        Ethos["elitism"] += option.Bonuses.ContainsKey("elitism") ? option.Bonuses["elitism"] : 0;
        Ethos["liberty"] += option.Bonuses.ContainsKey("liberty") ? option.Bonuses["liberty"] : 0;
        Ethos["authority"] += option.Bonuses.ContainsKey("authority") ? option.Bonuses["authority"] : 0;

        Strategies["mobility"] += option.Bonuses.ContainsKey("mobility") ? option.Bonuses["mobility"] : 0;
        Strategies["defence"] += option.Bonuses.ContainsKey("defence") ? option.Bonuses["defence"] : 0;
        Strategies["discipline"] += option.Bonuses.ContainsKey("discipline") ? option.Bonuses["discipline"] : 0;
        Strategies["offence"] += option.Bonuses.ContainsKey("offence") ? option.Bonuses["offence"] : 0;

        Warmongering += option.Bonuses.ContainsKey("warmongering") ? option.Bonuses["warmongering"] : 0;
        Openness += option.Bonuses.ContainsKey("openness") ? option.Bonuses["openness"] : 0;
        Progressivism += option.Bonuses.ContainsKey("progressivism") ? option.Bonuses["progressivism"] : 0;
        
        // TODO: InvalidOperationException: Collection was modified; enumeration operation may not execute.
        foreach (string adaptation in Adaptation.Keys.ToList())
        {
            if (option.Bonuses.ContainsKey(adaptation))
                Adaptation[adaptation] += option.Bonuses[adaptation];
        }
        
        if(option.ArtSubject != null)
            ArtSubjects.Add(option.ArtSubject);
    }

    public void MakeChoiceAI(Choice choice)
    {
        //TODO: let the AI make the choice
    }

    public int BuildingTimeEstimate()
    {
        if (BuildingProject == null || YieldsLastTurn.Wealth == 0)
            return 0;
        return (int) Mathf.Ceil((BuildingProject.Building.Cost - BuildingProject.Investment) / YieldsLastTurn.Wealth);
    }

    public int ResearchTimeEstimate()
    {
        if (ResearchCurrent == null || YieldsLastTurn.Science == 0)
            return 0;
        return (int) Mathf.Ceil((ResearchCurrent.GetCost() - ResearchScienceStock) / YieldsLastTurn.Science);
    }

    public int ArtworkTimeEstimate()
    {
        if (YieldsLastTurn.Culture == 0)
            return 0;
        return (int) Mathf.Ceil((ArtworkCost - ArtworkCultureStock) / YieldsLastTurn.Culture);
    }

    public Research SelectResearch()
    {
        // Find the available researches
        ResearchEra era = GameController.Main.GetResearchEra(ResearchEraIndex);
        List<Research> researchAvailable = new List<Research>();
        foreach (Research research in era.Researches)
        {
            // Ignore researches already completed
            if(ResearchCompleted.Contains(research))
                continue;
            
            if (research.Interests.Count == 1)
            {
                // For single-interest researches: we just need to not have it already researched
                researchAvailable.Add(research);
            }
            else if(ResearchCompleted.Count > 0)
            {
                // For multiple-interests researches: we need to have to have all interests researched in this level
                Dictionary<string, bool> interests = new Dictionary<string, bool>();
                foreach (string interest in research.Interests)
                {
                    interests.Add(interest, false);
                }

                foreach (Research prevResearch in ResearchCompleted)
                {
                    if (prevResearch.ResearchEra.Level == ResearchEraIndex && prevResearch.Interests.Count == 1 &&
                        interests.ContainsKey(prevResearch.Interests[0]))
                    {
                        interests[prevResearch.Interests[0]] = true;
                    }
                }
                
                if(!interests.ContainsValue(false))
                    researchAvailable.Add(research);
            }
        }
        
        // Calculate the preference score for the available researches
        Dictionary<Research, int> researchScore = new Dictionary<Research, int>();
        foreach (Research research in researchAvailable)
        {
            int score = 0;
            foreach (string interest in research.Interests)
            {
                score += Interests[interest];
            }

            score /= research.Interests.Count;
            researchScore.Add(research, score);
        }
        
        // Select a research by its preference
        return Utils.SelectByPref(researchScore);
    }

    public void CompleteResearch()
    {
        // Add the current research to the completed pool
        ResearchCompleted.Add(ResearchCurrent);
        
        // Present the choice
        if (ResearchCurrent.Choice != null)
        {
            GameController.Main.PresentChoice(ResearchCurrent.Choice, this);
        }

        // Add the ArtForm
        if(ResearchCurrent.ArtForm != null)
        {
            ArtForms.RemoveAt(ArtForms.Count - 1);
            ArtForms.Insert(0, ResearchCurrent.ArtForm);
        }
        
        // Update the job names
        if (ResearchCurrent.JobNames != null)
            foreach (KeyValuePair<string, string> jobName in ResearchCurrent.JobNames)
                JobNames[jobName.Key] = jobName.Value;

        // Create the notification
        new Notification(this, "Recherche complétée : " + ResearchCurrent.Name);

        // Reset the research state
        ResearchScienceStock -= ResearchCurrent.GetCost();
        ResearchCurrent = null;
        
        // Update the research count and the current era
        ResearchesCompletedInEra++;
        if (ResearchesCompletedInEra > 5)
        {
            ResearchEraIndex++;
            ResearchesCompletedInEra = 0;
            new Notification(this, "Début d'une nouvelle ère : " + GameController.Main.GetResearchEra(ResearchEraIndex).Name);
        }
    }

    public Artwork CreateArtwork()
    {
        // Movement / Form
        ArtMovement movement = null;
        string form = null;
        if (ArtMovement == null)
        {
            // No ArtMovement is currently adopted, so let's just give a raw ArtForm
            form = ArtForms[Random.Range(0, ArtForms.Count)];
        }
        else
        {
            movement = ArtMovement;
        }

        // Subject
        List<ArtSubject> subjects = ArtSubjects.ToList(); // Copy the art subjects list
        foreach (KeyValuePair<string, string> kv in JobNames) // Add the job names to the list
        {
            subjects.Add(new ArtSubject(kv.Value, kv.Key));
        }
        ArtSubject subject = subjects[Random.Range(0, subjects.Count)];

        // Focus
        string focus = null;
        List<string> possibleFocuses = new List<string>() {"geography"};
        // TODO: add buildings as a focus if we have a building, politics as a focus if we have a government
        switch (possibleFocuses[Random.Range(0, possibleFocuses.Count)])
        {
            case "geography":
                focus = Territory.ElementAt(Random.Range(0, Territory.Count)).Biome.Name;
                break;
            default:
                throw new Exception("Unexpected default case in switch statement");
        }
        
        // Descriptive
        string descriptive = null;
        switch (focus)
        {
            case "Coast":
                descriptive = "face à la mer";
                break;
            case "Lake":
                descriptive = "au bord du lac";
                break;
            case "Tropical":
                descriptive = "dans la jungle tropicale";
                break;
            case "Savanna":
                descriptive = "au coeur de la savanne";
                break;
            case "Desert":
                descriptive = "surmontant les dunes";
                break;
            case "Steppe":
                descriptive = "au milieu des steppes";
                break;
            case "Temperate":
                descriptive = "dans les vertes prairies";
                break;
            case "Taiga":
                descriptive = "au sein de la taiga";
                break;
            case "Tundra":
                descriptive = "sous la neige";
                break;
            default:
                throw new Exception("Unexpected default case is switch statement");
        }

        return new Artwork(this, movement, form, subject, descriptive, focus);
    }

    public BuildingProject SelectBuildingProject()
    {
        // Select the Interest of this building
        string buildingInterest = null;
        
        int rand = Random.Range(1, Interests.Values.Sum());
        foreach (KeyValuePair<string, int> interest in Interests)
        {
            rand -= interest.Value;
            if (rand <= 0)
            {
                buildingInterest = interest.Key;
                break;
            }
        }

        // Create the list of possible projects (one project by hex in the territory)
        Dictionary<BuildingProject, int> possibleProjects = new Dictionary<BuildingProject, int>();
        
        foreach (Building building in BuildingsAvailable)
        {
            // Skip if this building has nothing to do with the current interest
            if (building.Yields.Get(buildingInterest) == 0)
                continue;
                
            foreach (Hex hex in Territory)
            {
                // We can't build it here if :
                // - there is a wonder on this hex,
                // - the required biome/relief is wrong,
                // - the cost of the existing building is greater than this project
                if (building.CanBeBuiltOn(hex) ||
                    (hex.Building != null && !hex.Building.IsWonder && hex.Building.Cost <= building.Cost))
                {
                    
                    BuildingProject project = new BuildingProject(building, hex);
                    int score = project.PrefScore(this, buildingInterest);
                    //Debug.Log(project + " score: " + score);

                    if (score > 0)
                    {
                        // Register this project
                        possibleProjects.Add(project, score);
                    }
                }
            }
        }
        
        // If no project has been found, nothing will be built this turn
        if (possibleProjects.Count < 1)
            return null;
        
        // Now, select one of the projects by its preference score
        return Utils.SelectByPref(possibleProjects);
    }

    public override string ToString()
    {
        return Name;
    }
}