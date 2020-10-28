using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSelectionPanel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public Text Title;
    public Text Movement;
    public Text Infos;

    public GameObject CityBuildButton;

    public InputController inputController;
    
    // Update is called once per frame
    void Update()
    {
        if (inputController.SelectedUnit != null)
        {
            Unit unit = inputController.SelectedUnit;
            Title.text = unit.Name;
            Movement.text = string.Format("{0}/{1}", unit.MovePoints, unit.Movement);
            
            Hex[] hexPath = unit.GetHexPath();
            Infos.text = hexPath == null ? "No move planned" : hexPath.Length.ToString();

            if (unit.CanBuildCity)
            {
                CityBuildButton.SetActive(true);
                if (inputController.SelectedUnit.GetHex().CanBuildCity())
                    CityBuildButton.GetComponent<Button>().interactable = true;
                else
                    CityBuildButton.GetComponent<Button>().interactable = false;
            }
            else
            {
                CityBuildButton.SetActive(false);
            }
        }
    }
}
