
using System.Collections.Generic;

/// <summary>
/// Intellectual research that can be completed by a people
/// </summary>
public class Research
{
    public string Name;
    public ResearchEra ResearchEra; // Starting at 0
    public List<string> Interests; // "food", "wealth", "military", "science", "culture"

    // Bonuses brought once this research is completed
    public Yields BaseYields; // Increase of base yields
    public List<Building> Buildings; // Buildings unlocked
    public Choice Choice; // Choice presented afterwards
    public string ArtForm; // Art form that can be picked by art movements 
    public Dictionary<string, string> JobNames; // Change in job names

    public Research(string name, ResearchEra era, List<string> interests, Yields baseYields = null, List<Building> buildings = null, 
        Choice choice = null, string artForm = null, Dictionary<string, string> jobNames = null)
    {
        Name = name;
        ResearchEra = era;
        Interests = interests;
        BaseYields = baseYields;
        Buildings = buildings;
        Choice = choice;
        ArtForm = artForm;
        JobNames = jobNames;
    }
    
    public Research(string name, ResearchEra era, string interest, Yields baseYields = null, List<Building> buildings = null, 
        Choice choice = null, string artForm = null, Dictionary<string, string> jobNames = null)
        : this(name, era, new List<string>() {interest}, baseYields, buildings, choice, artForm, jobNames) {}

    public int GetCost()
    {
        return 400 + ResearchEra.Level * 200;
    }

    public string GetInterests()
    {
        string result = "";
        foreach (string interest in Interests)
        {
            if (result != "")
                result += " - ";
            result += interest;
        }
        return result;
    }
}