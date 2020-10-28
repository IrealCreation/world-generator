using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnitView : MonoBehaviour
{

    //Script added to the units prefab to manage its physical appearance

    Vector3 newPosition; // The position we're moving to
    Vector3 currentVelocity;

    public float smoothTime = 0.5f;
    
    private Queue<Hex> movePath = new Queue<Hex>(); // Path to animate. First hex is the one we're standing in
    
    public bool animationIsPlaying = false;

    public void Start() {
        newPosition = this.transform.position;
    }

    public void Update() {
        if (animationIsPlaying)
        {
            this.transform.position = Vector3.SmoothDamp(this.transform.position, newPosition, ref currentVelocity, smoothTime);
            //Debug.Log(Vector3.Distance(this.transform.position, newPosition));
            if (Vector3.Distance(this.transform.position, newPosition) < 0.1f)
            {
                animationIsPlaying = NextMove();
            }
        }
    }

    public void OnMove(Hex newHex) {

        movePath.Enqueue(newHex);
        animationIsPlaying = true;
        
    }
    public void OnMove(Queue<Hex> hexes) {

        // We currently prefer to use OnMove(Hex newHex)
        // Animate the unit moving along a path of hexes

        // We have to use a while loop, because hexes seems to be passed by reference and clearing it in the Unit class clears it here.
        while (hexes.Count > 0)
        {
            movePath.Enqueue(hexes.Dequeue());
        }

        animationIsPlaying = true;

    }

    /// <summary>
    /// Prepare the animation for the next move in the movePath queue
    /// </summary>
    /// <returns>Returns true if another move is queued ; false if it's the end of the path</returns>
    private bool NextMove()
    {
        //Debug.Log("NextMove " + movePath.Count);
        if (movePath.Count == 1)
        {
            movePath.Clear();
        }
        if (movePath.Count == 0)
        {
            return false;
        }
        
        Hex oldHex = movePath.Dequeue();
        Hex newHex = movePath.Peek();
        
        newPosition = newHex.PositionFromCamera();
        
        //Debug.Log("NewPosition " + newPosition);

        // If the unit is at the edge of the map, we teleport it on the other side without animation.
        // We check if the move is largely greater than the distance of a normal movement
        if( Vector3.Distance(this.transform.position, newPosition) > 5 ) {
            this.transform.position = newPosition;
        }

        return true;
    }
}
