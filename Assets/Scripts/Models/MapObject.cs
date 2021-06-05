using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapObject
{
    public GameObject GO;
    public Hex Hex { get; protected set; }
    
    public string Name;
    public string MapObjectType;
    public Color NameplateColor = Color.gray;
    
    public int Life;
    public int Strength;
}