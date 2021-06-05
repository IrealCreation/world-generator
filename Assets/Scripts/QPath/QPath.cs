using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QPath {

	/* 
		Our tiles need to be able to return the following information : 
			1) List of neighbours
			2) The cost to enter this tile from another tile
		QPath doesn't care what our tiles are (hex, square...) : they just need to implement the IQPathTile interface.

	*/

	public static class QPath {

		// <T> is used to define a generic type that must be specified when calling the function, and we want it to implement IQPathTile
		public static T[] FindPath<T>( IQPathUnit unit, T startTile, T endTile, CostEstimateDelegate costEstimateFunc )
			where T : IQPathTile 
		{

			if(unit == null || startTile == null || endTile == null) {

				Debug.LogError("null values passed to QPath::FindPath");
				return null;

			}

			// Call on our path solver
			QPath_AStar<T> resolver = new QPath_AStar<T>( unit, startTile, endTile, costEstimateFunc );
			
			resolver.DoWork();

			return resolver.GetResult();
		}

	}

	public delegate float TileEnteringCostDelegate();

	public delegate float CostEstimateDelegate(IQPathTile a, IQPathTile b);

}
