using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapUIController : MonoBehaviour
{
    public GameObject NameplatePrefab;
    public GameObject YieldsPrefab;
    public GameObject YieldFoodPrefab;
    public GameObject YieldWealthPrefab;
    public GameObject YieldMilitaryPrefab;
    public GameObject YieldCulturePrefab;
    public GameObject YieldSciencePrefab;

    public bool DisplayYields = false;

    private Dictionary<MapObject, MapUIBehaviour> mapObjectToBehaviours;
    private Dictionary<Hex, MapUIBehaviour> hexToBehaviours;
    private Dictionary<Hex, Yields> hexYields;
    
    // Start is called before the first frame update
    void Start()
    {
        GameObject.FindObjectOfType<HexMap>().OnCityCreated += CreateNameplate;
        mapObjectToBehaviours = new Dictionary<MapObject, MapUIBehaviour>();
        hexToBehaviours = new Dictionary<Hex, MapUIBehaviour>();
        hexYields = new Dictionary<Hex, Yields>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (KeyValuePair<MapObject, MapUIBehaviour> kv in mapObjectToBehaviours)
        {
            if(DisplayYields && kv.Key.Hex.IsVisible == true && kv.Value.TargetIsVisible())
                kv.Value.gameObject.SetActive(true);
            else
                kv.Value.gameObject.SetActive(false);
        }
        foreach (KeyValuePair<Hex, MapUIBehaviour> kv in hexToBehaviours)
        {
            if(DisplayYields && kv.Key.IsVisible == true && kv.Value.TargetIsVisible())
                kv.Value.gameObject.SetActive(true);
            else
                kv.Value.gameObject.SetActive(false);
        }
    }

    public void CreateNameplate( MapObject mo )
    {
        GameObject nameGO = (GameObject) Instantiate(NameplatePrefab, this.transform);
        MapUIBehaviour behaviour = nameGO.GetComponent<MapUIBehaviour>();
        nameGO.GetComponentInChildren<Image>().color = mo.NameplateColor;
        nameGO.GetComponentInChildren<Text>().text = mo.Name;
        behaviour.SetMapObjectTarget(mo);
        behaviour.MapUIController = this;
        mapObjectToBehaviours.Add(mo, behaviour);
    }

    public void ShowYields(Hex hex)
    {
        GameObject go;
        Yields yields = hex.GetYields();
        
        if (!hexYields.ContainsKey(hex))
        {
            go = (GameObject) Instantiate(YieldsPrefab, this.transform);
            go.name = hex + " yields";
            MapUIBehaviour behaviour = go.GetComponent<MapUIBehaviour>();
            behaviour.SetHexTarget(hex);
            behaviour.MapUIController = this;
            hexToBehaviours.Add(hex, behaviour);
        }
        else
        {
            go = hexToBehaviours[hex].gameObject;
            if (yields == hexYields[hex])
                return; // The yields didn't change, let's exit
        }
        hexYields[hex] = new Yields(yields);

        int yield = yields.Food;
        if(yield < 1)
            go.transform.Find("Food").gameObject.SetActive(false);
        else
        {
            while (yield > 0)
            {
                go.transform.Find("Food").gameObject.SetActive(true);
                Instantiate(YieldFoodPrefab, go.transform.Find("Food").transform);
                yield--;
            }
        }
        
        yield = yields.Wealth;
        if(yield < 1)
            go.transform.Find("Wealth").gameObject.SetActive(false);
        else
        {
            while (yield > 0)
            {
                go.transform.Find("Wealth").gameObject.SetActive(true);
                Instantiate(YieldWealthPrefab, go.transform.Find("Wealth").transform);
                yield--;
            }
        }
        
        yield = yields.Military;
        if(yield < 1)
            go.transform.Find("Military").gameObject.SetActive(false);
        else
        {
            while (yield > 0)
            {
                go.transform.Find("Military").gameObject.SetActive(true);
                Instantiate(YieldMilitaryPrefab, go.transform.Find("Military").transform);
                yield--;
            }
        }
        
        yield = yields.Science;
        if(yield < 1)
            go.transform.Find("Science").gameObject.SetActive(false);
        else
        {
            while (yield > 0)
            {
                go.transform.Find("Science").gameObject.SetActive(true);
                Instantiate(YieldSciencePrefab, go.transform.Find("Science").transform);
                yield--;
            }
        }
        
        yield = yields.Culture;
        if(yield < 1)
            go.transform.Find("Culture").gameObject.SetActive(false);
        else
        {
            while (yield > 0)
            {
                go.transform.Find("Culture").gameObject.SetActive(true);
                Instantiate(YieldCulturePrefab, go.transform.Find("Culture").transform);
                yield--;
            }
        }
    }

    public void ToggleYields()
    {
        DisplayYields = !DisplayYields;
    }

    public void MapObjectDestroyed(MapObject mo)
    {
        Destroy(mapObjectToBehaviours[mo]);
        mapObjectToBehaviours.Remove(mo);
    }

    public void Restart()
    {
        // The game restarts: burn everything!
        foreach (KeyValuePair<MapObject, MapUIBehaviour> kv in mapObjectToBehaviours)
        {
            Destroy(kv.Value.gameObject);
            Destroy(kv.Value);
        }
        foreach (KeyValuePair<Hex, MapUIBehaviour> kv in hexToBehaviours)
        {
            Destroy(kv.Value.gameObject);
            Destroy(kv.Value);
        }
        mapObjectToBehaviours = new Dictionary<MapObject, MapUIBehaviour>();
        hexToBehaviours = new Dictionary<Hex, MapUIBehaviour>();
        hexYields = new Dictionary<Hex, Yields>();
    }
}
