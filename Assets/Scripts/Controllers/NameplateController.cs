using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameplateController : MonoBehaviour
{
    public GameObject prefab;

    private Dictionary<MapObject, GameObject> mapObjectToNameplate;
    
    // Start is called before the first frame update
    void Start()
    {
        GameObject.FindObjectOfType<HexMap>().OnCityCreated += CreateNameplate;
        mapObjectToNameplate = new Dictionary<MapObject, GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateNameplate( MapObject mo )
    {
        GameObject nameGO = (GameObject) Instantiate(prefab, this.transform);
        nameGO.GetComponentInChildren<Image>().color = mo.NameplateColor;
        nameGO.GetComponent<NameplateBehaviour>().Target = mo;
        nameGO.GetComponentInChildren<Text>().text = mo.Name;
        mapObjectToNameplate.Add(mo, nameGO);
    }

    public void MapObjectDestroyed(MapObject mo)
    {
        Destroy(mapObjectToNameplate[mo]);
        mapObjectToNameplate.Remove(mo);
    }
}
