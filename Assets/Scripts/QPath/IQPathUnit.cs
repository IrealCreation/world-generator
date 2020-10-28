using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QPath {

	public interface IQPathUnit {

		float AggregateTurnsToEnterTile(IQPathTile sourceTile, IQPathTile destinationTile, float turnsToDate);

	}

}
