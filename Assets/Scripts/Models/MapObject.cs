using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapObject
{
    public GameObject GO;
    protected Hex hex;
    
    public string Name;
    public string MapObjectType;
    public Color NameplateColor = Color.gray;
    
    public int Life;
    public int Strength;

    public Hex GetHex() {
        return hex;
    }
}