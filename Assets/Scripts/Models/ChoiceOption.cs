using System.Collections.Generic;

public class ChoiceOption
{
    public string Description;
    public Dictionary<string, int> Bonuses;
    public ArtSubject ArtSubject; // Art subject unlocked

    public ChoiceOption(string description, Dictionary<string, int> bonuses, ArtSubject artSubject = null)
    {
        Description = description;
        Bonuses = bonuses;
        ArtSubject = artSubject;
    }

    public string GetEffects()
    {
        string effects = "";
        foreach (KeyValuePair<string, int> bonus in Bonuses)
        {
            if (effects != "")
                effects += ", ";
            if(bonus.Value == 0)
                continue;
            effects += bonus.Key + " ";
            if (bonus.Value > 0)
                effects += "+";
            effects += bonus.Value;
        }

        return effects;
    }
}