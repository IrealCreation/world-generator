using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MenuController : MonoBehaviour
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

        if (canCloseMenu)
        {
            CloseMenuButton.gameObject.SetActive(true);
        }
        else
        {
            CloseMenuButton.gameObject.SetActive(false);
        }
    }

    public void CloseMenu()
    {
        InputController.OnMap = true;
        StartMenu.SetActive(false);
        BottomUI.SetActive(true);
    }
}
