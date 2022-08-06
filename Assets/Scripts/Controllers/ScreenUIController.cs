using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ScreenUIController : MonoBehaviour
{
    private HexMap hexMap;
    private bool canCloseMenu;
    public InputController InputController;
    public CameraController CameraController;
    public InputField SeedInput;
    public GameObject StartMenu;
    public GameObject BottomUI;
    public GameObject TileInformations;
    public GameObject NotificationsPanel;
    public GameObject NotificationPanelPrefab;
    public GameObject MapInformations;
    private Text yieldsStockFood;
    private Text yieldsStockWealth;
    private Text yieldsStockMilitary;
    private Text yieldsStockScience;
    private Text yieldsStockCulture;
    public Text CurrentSeedText;
    public Button CloseMenuButton;
    
    public GameObject UnitSelectionPanel;
    public GameObject CitySelectionPanel;
    public Button NextTurnButton;
    public Button ToggleYieldsButton;
    public Text TileNameText;
    public Text TileInfoText;
    /* People Informations */
    private People selectedPeople;
    public GameObject PeopleInformations;
    public Text PeopleNameText;
    public Text PeoplePopulation;
    public Text PeopleTerritory;
    public Text PeopleScienceEra;
    public Text PeopleScienceNumber;
    public Text PeopleScienceLast;
    public Text PeopleScienceCurrent;
    public Text PeopleWealthBuildings;
    public Text PeopleWealthWonders;
    public Text PeopleWealthLast;
    public Text PeopleWealthCurrent;
    public Text PeopleCultureMovement;
    public Text PeopleCultureNumber;
    public Text PeopleCultureLast;
    public Text PeopleCultureCurrent;
    public Text PeopleMilitaryStrategy;
    
    public GameObject ChoicePanel;
    public GameObject ChoiceOptionsDisplay;
    public GameObject ChoiceOptionPanelPrefab;
    
    private void Start()
    {
        hexMap = GameObject.FindObjectOfType<HexMap>();
        CameraController = Camera.main.GetComponent<CameraController>();
        canCloseMenu = false; // Cannot close the first starting menu

        // Cache some gameobjects for efficiency
        Transform yieldsStock = PeopleInformations.transform.Find("People Yields");
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
        
        GameController.Main.StartGame(seed);
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
        // ToggleYieldsButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// Switch the UI between "omniscient map view" (true) or "ingame player view" (false)
    /// </summary>
    public void ToggleMapInfo(bool active)
    {
        //MapInfoPanel.SetActive(active);
        NextTurnButton.gameObject.SetActive(!active);
        PeopleInformations.gameObject.SetActive(!active);
    }

    public void NextTurn()
    {
        GameController.Main.NextTurn();
    }

    public void SelectHex(Hex hex)
    {
        if (hex != null)
        {
            TileNameText.text = hex.ToString();
            TileInfoText.text = hex.Description();
        }
        else
        {
            TileNameText.text = "";
            TileInfoText.text = "";
        }
    }

    public void SelectPeople(People people)
    {
        selectedPeople = people;
        if (selectedPeople == null)
        {
            PeopleInformations.SetActive(false);
            TileInformations.SetActive(true);
            MapInformations.SetActive(true);
            return;
        }
        PeopleInformations.SetActive(true);
        TileInformations.SetActive(false);
        MapInformations.SetActive(false);
        UpdatePeopleInfos();
    }

    public void UpdatePeopleInfos()
    {
        if (selectedPeople == null)
            return;
        
        PeopleNameText.text = selectedPeople.Name;
        PeopleNameText.color = selectedPeople.Color;
        UpdateYieldsStock(selectedPeople.YieldsLastTurn);
        
        PeopleTerritory.text = "Nombre de territoires : " + selectedPeople.Territory.Count;

        if (selectedPeople.ResearchCompleted.Count > 0)
            PeopleScienceEra.text = "Ère actuelle : " + selectedPeople.ResearchCompleted.Last().ResearchEra.Name;
        else
            PeopleScienceEra.text = "Préhistoire";
        PeopleScienceNumber.text = "Compte de découvertes : " + selectedPeople.ResearchCompleted.Count;
        if (selectedPeople.ResearchCompleted.Count > 0)
            PeopleScienceLast.text = "Dernière découverte : " + selectedPeople.ResearchCompleted.Last().Name;
        else
            PeopleScienceLast.text = "Aucune découverte effectuée";
        if (selectedPeople.ResearchCurrent != null)
            PeopleScienceCurrent.text = "Prochaine découverte dans " + selectedPeople.ResearchTimeEstimate() + " tours" +
                                        "\n(" + selectedPeople.ResearchCurrent.GetInterests() + ")";
        else
            PeopleScienceCurrent.text = "Aucune recherche en cours";
        
        PeopleWealthBuildings.text = "Nombre de bâtiments : " + selectedPeople.BuildingsCount;
        PeopleWealthWonders.text = "Nombre de merveilles : " + selectedPeople.WondersCount;
        if (selectedPeople.BuildingProjectLast != null)
            PeopleWealthLast.text = "Dernière construction : " + selectedPeople.BuildingProjectLast.Building.Name;
        else
            PeopleWealthLast.text = "Aucune construction récente";
        if (selectedPeople.BuildingProject != null)
            PeopleWealthCurrent.text = "Construction en cours : " + selectedPeople.BuildingProject.Building +
                                       "\n(" + selectedPeople.BuildingTimeEstimate() + " tours)";
        else
            PeopleWealthCurrent.text = "Aucune construction en cours";

        if (selectedPeople.ArtMovement != null)
            PeopleCultureMovement.text = "Courant artistique : " + selectedPeople.ArtMovement.Name;
        else
            PeopleCultureMovement.text = "Aucun courant artistique actuel";
        PeopleCultureNumber.text = "Nombre d'oeuvres d'art : " + selectedPeople.Artworks.Count;
        if (selectedPeople.Artworks.Count > 0)
            PeopleCultureLast.text = "Dernière oeuvre :\n" + selectedPeople.Artworks.Last().Name;
        else
            PeopleCultureLast.text = "Aucune oeuvre d'art";
        PeopleCultureCurrent.text = "Prochaine oeuvre dans " + selectedPeople.ArtworkTimeEstimate() + " tours";
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

    public void ShowChoice(Choice choice, People people)
    {
        ChoicePanel.SetActive(true);
        ChoicePanel.transform.Find("Title").GetComponent<Text>().text = choice.Name;
        ChoicePanel.transform.Find("Description").GetComponent<Text>().text = choice.Description.Replace("$name", people.Name);
        Transform optionsContainer = ChoiceOptionsDisplay.transform;
        
        // Remove possible previous choices
        if (optionsContainer.childCount > 0)
        {
            foreach (Transform child in optionsContainer) {
                Destroy(child.gameObject);
            }
        }
        
        foreach (ChoiceOption option in choice.Options)
        {
            GameObject optionGO = Instantiate(ChoiceOptionPanelPrefab, optionsContainer);
            optionGO.GetComponent<ChoiceOptionPanelBehaviour>().SetChoiceOption(option, choice, people);
        }
    }

    public void HideChoice()
    {
        ChoicePanel.SetActive(false);
    }

    public void ShowNotification(Notification notification)
    {
        GameObject notifGO = Instantiate(NotificationPanelPrefab, NotificationsPanel.transform);
        notifGO.transform.SetSiblingIndex(0);
        notifGO.GetComponent<Text>().text = notification.People.Name + " - " + notification.Text;
    }

    public void RemoveNotifications()
    {
        // Remove possible previous choices
        if (NotificationsPanel.transform.childCount > 0)
        {
            foreach (Transform child in NotificationsPanel.transform) {
                Destroy(child.gameObject);
            }
        }
    }
}
