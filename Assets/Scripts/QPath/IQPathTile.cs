using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace QPath {

	public interface IQPathTile {

		IQPathTile[] GetNeighbours();

		float CostToEnter();

		float AggregateCostToEnter( float costSoFar, IQPathTile fromTile, IQPathUnit unit );

	}

}
