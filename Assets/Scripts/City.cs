using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City : MapObject {

	public City(Hex hex, GameObject go) {
		MyHex = hex;
		GO = go;
		Name = "Les Gros Ormes";
		MapObjectType = "City";
		Life = 100;
		Strength = 10;
	}

	private CityJob cityJob;

	private float overflowedProduction;

	public void DoTurn()
	{
		float production = 10f;
		
		if (cityJob != null)
		{
			cityJob.AddProduction(production);
		}
	}

}
