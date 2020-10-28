using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameplateController : MonoBehaviour
{

    public GameObject prefab;
    
    // Start is called before the first frame update
    void Start()
    {
        GameObject.FindObjectOfType<HexMap>().OnCityCreated += CreateNameplate;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateNameplate( MapObject mo )
    {
        GameObject nameGO = (GameObject) Instantiate(prefab, this.transform);
        nameGO.GetComponent<Nameplate>().Target = mo;
        nameGO.GetComponent<Nameplate>().Name.text = mo.Name;
    }
}
