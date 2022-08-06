
using System.Collections.Generic;

/// <summary>
/// Subject for an artwork, which can be added to the pool of available subject by a people's choices.
/// When selected in an artwork, influence the people towards its focus.
/// </summary>
public class ArtSubject
{
    public string Name;
    public string Focus; // Interest or ethos

    public ArtSubject(string name, string focus)
    {
        Name = name;
        Focus = focus;
    }
}