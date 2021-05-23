using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer {

	// Player pointer
	
	public GameObject GO;

    public Hex MyHex { // Hex I am standing in
    	get;
    	protected set;
    }

    public void SetHex( Hex hex ) {
    	MyHex = hex;
    }

}
