using System.Collections.Generic;

public class ResearchEra
{
    public string Name;
    public int Level;
    public List<Research> Researches;

    public ResearchEra(string name, int level)
    {
        Name = name;
        Level = level;
        Researches = new List<Research>();
    }

    public void Add(Research research)
    {
        Researches.Add(research);
    }
}