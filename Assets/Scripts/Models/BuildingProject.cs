/// <summary>
/// Ongoing project of building construction by a people on a hex
/// </summary>
public class BuildingProject
{
    public Building Building;
    public Hex Hex;
    public int Investment;

    public BuildingProject(Building building, Hex hex)
    {
        Building = building;
        Hex = hex;
    }

    /// <summary>
    /// Check if the building project is still valid
    /// </summary>
    /// <param name="people"></param>
    /// <returns></returns>
    public bool IsValid(People people)
    {
        if (Hex.People == people && Building.WonderBuilt == false)
            return true;
        return false;
    }

    public int PrefScore(People people, string interest)
    {
        // Let's calculate the preference score for this building (the interest yield counts double)
        int score = Building.Yields.Sum() + Building.Yields.Get(interest);
        // Remove half the yields of the existing building
        if (Hex.Building != null)
            score -= Hex.Building.Yields.Sum() / 2;
        // Malus points for distance with capital
        score -= Hex.Distance(Hex, people.Capital) * 10;
        // Bonus depending on the tile adaptation
        score += score * (people.Adaptation[Hex.Biome.Name] + people.Adaptation[Hex.Relief.Name]) / 40;
        // Malus for features
        if (Hex.Feature != null)
            score -= 100;
        
        return score;
    }

    /// <summary>
    /// Invest the resource into the building
    /// </summary>
    /// <param name="investment">Resources invested</param>
    /// <returns>0 if the building isn't finished, or else the resource overflow (min 1)</returns>
    public int Build(int investment)
    {
        Investment += investment;
        if (Investment > Building.Cost)
        {
            Finish();
            return 1 + Investment - Building.Cost;
        }

        return 0;
    }

    public void Finish()
    {
        Hex.Building = Building;
        if (Building.IsWonder)
        {
            Building.WonderBuilt = true;
            new Notification(Hex.People, "Merveille achevée : " + ToString());
        }
        else
        {
            new Notification(Hex.People, "Bâtiment construit : " + ToString());
        }

        // Show the choice
        if (Building.Choice != null)
        {
            GameController.Main.PresentChoice(Building.Choice, Hex.People);
        }
    }

    public override string ToString()
    {
        return Building.ToString() + "(" + Hex.ToString() + ")";
    }
}