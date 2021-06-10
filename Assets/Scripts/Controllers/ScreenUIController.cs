using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ScreenUIController : MonoBehaviour
{
    HexMap hexMap;
    private bool canCloseMenu;
    public InputController InputController;
    CameraController CameraController;
    public InputField SeedInput;
    public GameObject StartMenu;
    public GameObject BottomUI;
    public GameObject PeopleTopBar;
    private Text yieldsStockFood;
    private Text yieldsStockWealth;
    private Text yieldsStockMilitary;
    private Text yieldsStockScience;
    private Text yieldsStockCulture;
    public Text CurrentSeedText;
    public Button CloseMenuButton;
    
    public GameObject UnitSelectionPanel;
    public GameObject CitySelectionPanel;
    public GameObject MapInfoPanel;
    public Button NextTurnButton;
    public Button ToggleYieldsButton;
    public Text TileNameText;
    public Text TileInfoText;
    private void Start()
    {
        hexMap = GameObject.FindObjectOfType<HexMap>();
        CameraController = Camera.main.GetComponent<CameraController>();
        canCloseMenu = false; // Cannot close the first starting menu

        // Cache some gameobjects for efficiency
        Transform yieldsStock = PeopleTopBar.transform.Find("Stock");
        yieldsStockFood = yieldsStock.Find("Food").GetComponentInChildren<Text>(true);
        yieldsStockWealth = yieldsStock.Find("Wealth").GetComponentInChildren<Text>(true);
        yieldsStockMilitary = yieldsStock.Find("Military").GetComponentInChildren<Text>(true);
        yieldsStockScience = yieldsStock.Find("Science").GetComponentInChildren<Text>(true);
        yieldsStockCulture = yieldsStock.Find("Culture").GetComponentInChildren<Text>(true);
        
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
        
        CloseMenu();
        canCloseMenu = true;
        CameraController.ResetCamera(false);
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
        ToggleYieldsButton.gameObject.SetActive(false);
        
        CloseMenuButton.gameObject.SetActive(canCloseMenu);
    }

    public void CloseMenu()
    {
        InputController.OnMap = true;
        StartMenu.SetActive(false);
        BottomUI.SetActive(true);
        ToggleYieldsButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// Switch the UI between "divine map view" (true) or "ingame player view" (false)
    /// </summary>
    public void ToggleMapInfo(bool active)
    {
        MapInfoPanel.SetActive(active);
        NextTurnButton.gameObject.SetActive(!active);
        PeopleTopBar.gameObject.SetActive(!active);
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

    public void UpdateYieldsStock(Yields yields)
    {
        yieldsStockFood.text = yields.Food.ToString();
        yieldsStockWealth.text = yields.Wealth.ToString();
        yieldsStockMilitary.text = yields.Military.ToString();
        yieldsStockScience.text = yields.Science.ToString();
        yieldsStockCulture.text = yields.Culture.ToString();
    }
}
