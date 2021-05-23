using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City : MapObject
{
	public People People;
	
	private CityJob cityJob;
	private float overflowedProduction;

	public City(Hex hex, People people, GameObject go) {
		base.hex = hex;
		People = people;
		NameplateColor = People.Color;
		GO = go;
		Name = "Les Gros Ormes";
		MapObjectType = "City";
		Life = 100;
		Strength = 10;
	}

	public void DoTurn()
	{
		float production = 10f;
		
		if (cityJob != null)
		{
			cityJob.AddProduction(production);
		}
	}

}
