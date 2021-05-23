
using System.Collections.Generic;
using QPath;
using UnityEngine;

// Edge between two hexes
public class Edge : IQPathTile
{
    public readonly Hex HexA;
    public readonly Hex HexB;
    public readonly HexMap HexMap;
    
    private char orientation; //q (column), r (row), or s (sum)

    public River River;

    private Edge[] neighbours; //Cache the neighbours

    public Edge(HexMap hexMap, Hex a, Hex b, char orientation)
    {
        this.HexMap = hexMap;
        this.HexA = a;
        this.HexB = b;
        this.orientation = orientation;

        //Debug.Log("Instantiate Edge " + ToString() + " (orientation " + orientation + ")");
    }

    public Vector3[] Positions()
    {
        Vector3[] positions = new Vector3[2];
        if (orientation == 'q')
        {
            positions[0] = new Vector3(0, 0, Hex.RADIUS);
            positions[1] = new Vector3(Hex.WIDTH_MULTIPLIER, 0, Hex.RADIUS / 2);
        }
        else if (orientation == 'r')
        {
            positions[0] = new Vector3(Hex.WIDTH_MULTIPLIER, 0, Hex.RADIUS / 2);
            positions[1] = new Vector3(Hex.WIDTH_MULTIPLIER, 0, Hex.RADIUS / -2);
        }
        else if (orientation == 's')
        {
            positions[0] = new Vector3(Hex.WIDTH_MULTIPLIER, 0, Hex.RADIUS / -2);
            positions[1] = new Vector3(0, 0, -Hex.RADIUS);
        }

        //Debug.Log("Edge " + ToString() + " (orientation " + orientation + ") positions: " + positions[0] + " " + positions[1]);

        return positions;
    }

    public Vector3[] Positions(Vector3 offset)
    {
        Vector3[] positions = Positions();

        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] += offset;
        }

        return positions;
    }

    public Quaternion Angle(Hex refHex)
    {
        // The rotation of this edge, relatively to the top-left edge
        Vector3 vector3 = Vector3.zero;
        if (orientation == 'r')
        {
            if (refHex.Q == HexA.Q)
                vector3.y = 120;
            else
                vector3.y = 300;
        }
        if (orientation == 'q')
        {
            if (refHex.R == HexA.R)
                vector3.y = 60;
            else
                vector3.y = 240;
        }
        if (orientation == 's')
        {
            if (refHex.Q == HexA.Q)
                vector3.y = 180;
            else
                vector3.y = 0;
        }
        
        return Quaternion.Euler(0, vector3.y, 0);
    }
    
    public IQPathTile[] GetNeighbours()
    {
        if (this.neighbours == null)
        {
            List<Edge> neighbours = new List<Edge>();
            Edge e;

            if (orientation == 'q')
            {
                e = HexMap.GetEdgeAt(HexA.Q - 1, HexA.R + 1, HexA.Q, HexA.R);
                if(e != null)
                    neighbours.Add(e);
                
                e = HexMap.GetEdgeAt(HexA.Q - 1, HexA.R + 1, HexB.Q, HexB.R);
                if(e != null)
                    neighbours.Add(e);
                
                e = HexMap.GetEdgeAt(HexA.Q, HexA.R, HexB.Q + 1, HexB.R - 1);
                if(e != null)
                    neighbours.Add(e);
                
                e = HexMap.GetEdgeAt(HexB.Q, HexB.R, HexB.Q + 1, HexB.R - 1);
                if(e != null)
                    neighbours.Add(e);
            }
            else if (orientation == 'r')
            {
                e = HexMap.GetEdgeAt(HexA.Q, HexA.R, HexB.Q - 1, HexB.R + 1);
                if(e != null)
                    neighbours.Add(e);
                
                e = HexMap.GetEdgeAt(HexA.Q, HexA.R, HexB.Q, HexB.R - 1);
                if(e != null)
                    neighbours.Add(e);
                
                e = HexMap.GetEdgeAt(HexA.Q + 1, HexA.R - 1, HexB.Q, HexB.R);
                if(e != null)
                    neighbours.Add(e);
                
                e = HexMap.GetEdgeAt(HexA.Q, HexA.R + 1, HexB.Q, HexB.R);
                if(e != null)
                    neighbours.Add(e);
            }
            else
            {
                e = HexMap.GetEdgeAt(HexB.Q - 1, HexB.R, HexA.Q, HexA.R);
                if(e != null)
                    neighbours.Add(e);
                
                e = HexMap.GetEdgeAt(HexA.Q, HexA.R - 1, HexB.Q, HexB.R);
                if(e != null)
                    neighbours.Add(e);
                
                e = HexMap.GetEdgeAt(HexA.Q, HexA.R, HexB.Q, HexB.R + 1);
                if(e != null)
                    neighbours.Add(e);
                
                e = HexMap.GetEdgeAt(HexB.Q, HexB.R, HexA.Q + 1, HexA.R);
                if(e != null)
                    neighbours.Add(e);
            }

            this.neighbours = neighbours.ToArray();
        }
        
        return this.neighbours;
    }

    public float GetElevation()
    {
        return (4f + HexA.Elevation + HexB.Elevation) / 2f;
    }

    public void SetRiver(River r)
    {
        this.River = r;
    }

    public float CostToEnter()
    {
        return GetElevation();
    }
    public static float RiverCostEstimate(IQPathTile tile)
    {
        // Possibly totally useless (the actual method used is on River.CostToEnterTile)
        Edge e = (Edge) tile;
        return tile.CostToEnter();
    }
    public static bool RiverEndValidate(IQPathTile tile)
    {
        Edge e = (Edge) tile;
        
        return (e.HexA.IsSea || e.HexB.IsSea || e.River != null);
    }

    public float AggregateCostToEnter(float costSoFar, IQPathTile fromTile, IQPathUnit unit)
    {
        return unit.AggregateTurnsToEnterTile(fromTile, this, costSoFar);
    }

    public override string ToString()
    {
        return HexA + " / " + HexB;
    }
}