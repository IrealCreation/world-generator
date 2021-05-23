using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UIController : MonoBehaviour
{
    HexMap hexMap;
    private bool canCloseMenu;
    public InputController InputController;
    CameraController CameraController;
    public InputField SeedInput;
    public GameObject StartMenu;
    public GameObject BottomUI;
    public Text CurrentSeedText;
    public Button CloseMenuButton;
    
    public GameObject UnitSelectionPanel;
    public GameObject CitySelectionPanel;
    public GameObject MapInfoPanel;
    public Button NextTurnButton;
    public Text TileNameText;
    public Text TileInfoText;
    private void Start()
    {
        hexMap = GameObject.FindObjectOfType<HexMap>();
        CameraController = Camera.main.GetComponent<CameraController>();
        canCloseMenu = false; // Cannot close the first starting menu
        
        //Initial camera with the panorama
        CameraController.SetLimits(hexMap.numRows);
        CameraController.ResetCamera(true);
        
        OpenMenu();
    }

    public void GenerateWorld(bool random = false)
    {
        int seed;
        if (random)
            seed = Random.Range(1, 999999);
        else
            seed = int.Parse(SeedInput.text);
        
        hexMap.ResetMap(seed);
        CurrentSeedText.text = "Current seed: " + seed;
        
        CameraController.SetLimits(hexMap.numRows);
        CameraController.ResetCamera();
        //Debug.Log("MenuController::NewMap()");
        
        CloseMenu();
        canCloseMenu = true;
    }

    public void RandomizeSeed()
    {
        SeedInput.text = Random.Range(1, 999999).ToString();
    }

    public void OpenMenu()
    {
        RandomizeSeed();
        InputController.OnMap = false;
        StartMenu.SetActive(true);
        BottomUI.SetActive(false);
        
        CloseMenuButton.gameObject.SetActive(canCloseMenu);
    }

    public void CloseMenu()
    {
        InputController.OnMap = true;
        StartMenu.SetActive(false);
        BottomUI.SetActive(true);
    }

    public void ToggleMapInfo(bool active)
    {
        MapInfoPanel.SetActive(active);
        NextTurnButton.gameObject.SetActive(!active);
    }

    public void NextTurn()
    {
        hexMap.NextTurn();
    }

    public void SelectHex(Hex hex)
    {
        if (hex != null)
        {
            TileNameText.text = "Coordinates: " + hex.ToString();
            TileInfoText.text = hex.Description();
        }
        else
        {
            TileNameText.text = "";
            TileInfoText.text = "";
        }
    }

    public void SelectUnit(Unit unit)
    {
        UnitSelectionPanel.SetActive(unit != null);
        // The display of informations is handled by UnitSelectionPanel
    }

    public void SelectCity(City city)
    {
        CitySelectionPanel.SetActive(city != null);
    }
}
