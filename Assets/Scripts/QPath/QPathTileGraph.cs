using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QPath {

	public class IQPathTileGraph {
		//The graph job is to keep a list of all neighbours leaving a tile

		private Dictionary<IQPathTile, IQPathTile[]> neighbours;

		class Edge {
			// An edge is the connexion between two nodes (in our case, tiles)

			public IQPathTile a;
			public IQPathTile b;

			public delegate int EdgeCostDelegate();
			public EdgeCostDelegate EdgeCost;
		}

		public IQPathTileGraph( IQPathWorld world ) {

			/*Edge e;

			e.EdgeCost();*/

		}

	}

}
