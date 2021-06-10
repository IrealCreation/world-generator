using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSelectionPanel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        inputController = GameObject.FindObjectOfType<InputController>();
        hexMap = GameObject.FindObjectOfType<HexMap>();
    }

    public Text Title;
    public Text Movement;
    public Text Infos;

    public GameObject CityBuildButton;
    public GameObject ExploreButton;

    private InputController inputController;
    private HexMap hexMap;
    
    // Update is called once per frame
    void Update()
    {
        if (inputController.SelectedUnit != null)
        {
            Unit unit = inputController.SelectedUnit;
            Title.text = unit.Name;
            Movement.text = string.Format("{0}/{1}", unit.MovePoints, unit.Movement);
            
            //Hex[] hexPath = unit.GetHexPath();
            //Infos.text = hexPath == null ? "No move planned" : hexPath.Length.ToString();

            if (unit.CanBuildCity)
            {
                CityBuildButton.SetActive(true);
                if (inputController.SelectedUnit.Hex.CanBuildCity())
                    CityBuildButton.GetComponent<Button>().interactable = true;
                else
                    CityBuildButton.GetComponent<Button>().interactable = false;
            }
            else
            {
                CityBuildButton.SetActive(false);
            }
            
            if (unit.CanSearch)
            {
                ExploreButton.SetActive(true);
                if (inputController.SelectedUnit.Hex.CanBeSearched(unit.People) && inputController.SelectedUnit.MovePoints > 0)
                    ExploreButton.GetComponent<Button>().interactable = true;
                else
                    ExploreButton.GetComponent<Button>().interactable = false;
            }
            else
            {
                ExploreButton.SetActive(false);
            }
        }
    }

    public void Search()
    {
        inputController.SelectedUnit.MovePoints--;
        hexMap.SearchHex(inputController.SelectedUnit.Hex);
    }
}
