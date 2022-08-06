
using System.Collections.Generic;
using UnityEngine;

public class ArtMovement
{
    public string Name;
    public People Creator;
    public int Turn;
    public List<Artwork> Artworks;

    public ArtMovement(string artForm, string focus, People creator)
    {
        Creator = creator;
        Turn = GameController.Main.Turn;
        Artworks = new List<Artwork>();

        Name = artForm + " ";
        List<string> variants;
        switch (focus)
        {
            case "food":
                variants = new List<string>() {"naturaliste", "kitsch", "populaire", "folklorique"};
                Name += variants[Random.Range(0, variants.Count - 1)];
                break;
            case "wealth":
                variants = new List<string>() {"baroque", "maniériste", "opulent", "design"};
                Name += variants[Random.Range(0, variants.Count - 1)];
                break;
            case "military":
                variants = new List<string>() {"épique", "brutaliste", "héroïque"};
                Name += variants[Random.Range(0, variants.Count - 1)];
                break;
            case "science":
                variants = new List<string>() {"académique", "humaniste", "savant"};
                Name += variants[Random.Range(0, variants.Count - 1)];
                break;
            case "culture":
                variants = new List<string>() {"lyrique", "impressioniste", "esthétique", "fantastique"};
                Name += variants[Random.Range(0, variants.Count - 1)];
                break;
            case "equality":
                variants = new List<string>() {"réaliste", "satirique", "minimaliste"};
                Name += variants[Random.Range(0, variants.Count - 1)];
                break;
            case "elitism":
                variants = new List<string>() {"symboliste", "éclectique", "précieux"};
                Name += variants[Random.Range(0, variants.Count - 1)];
                break;
            case "liberty":
                variants = new List<string>() {"romantique", "nouveau", "libertin"};
                Name += variants[Random.Range(0, variants.Count - 1)];
                break;
            case "authority":
                variants = new List<string>() {"classique", "futuriste", "panégyrique"};
                Name += variants[Random.Range(0, variants.Count - 1)];
                break;
        }
    }
}