using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputController : MonoBehaviour
{
    public float scrollSpeed = 20f;
    float keyboardMoveSpeed = 12;

    public bool OnMap; // Is the player watching the map and has access to map interractions (scroll zoom...)
    
    private HexMap hexMap;
    private Hex hexUnderMouse;
    private Hex hexLastUnderMouse;
    private LineRenderer lineRenderer;
    
    private ScreenUIController screenUIController;
    private CameraController cameraController;

    Vector3 lastMousePosition; //Position of the mouse on the screen (from Input.mousePosition)
    Vector3 lastHitPos; //Position of the mouse relating to the game ground plane (from MouseRayHitPos())

    Unit _selectedUnit;
    public Unit SelectedUnit
    {
        get { return _selectedUnit; }
        set
        {
            _selectedUnit = value;
            screenUIController.SelectUnit(_selectedUnit);
            DrawPath(null);
            if(_selectedUnit != null && _selectedUnit.CanSearch)
                hexMap.HighlightSearchableHexes(_selectedUnit.People);
            else 
                hexMap.UnhighlightSearchableHexes();
        }
    }

    City _selectedCity;
    public City SelectedCity
    {
        get { return _selectedCity; }
        set
        {
            // We temporarily set ourselves to null, to avoid infinite looping through ResetUpdateFunc
            _selectedCity = null;
            
            if (value != null)
            {
                ResetUpdateFunc();
                Update_CurrentFunc = Update_CityView;
                
                //TODO: move the camera to the city
                Vector3 cityPos = value.Hex.Position();
                //Debug.Log("City position: " + cityPos);
                //Debug.Log("Camera position: " + Camera.main.GetComponent<CameraController>().GetPosition());
                //Camera.main.GetComponent<CameraController>().MoveCamera(cityPos);
            }
            else
            {
                ResetUpdateFunc();
            }
            
            _selectedCity = value;
            screenUIController.SelectCity(_selectedCity);
            SelectedUnit = null;
        }
    }

    private Hex[] hexPath;

    delegate void UpdateFunc();
    UpdateFunc Update_CurrentFunc; //Variable to store a pointer to the function we currently need to call during Update()

    public LayerMask LayerIDHexTiles;

    void Start() {
        ResetUpdateFunc();

        hexMap = GameObject.FindObjectOfType<HexMap>();
        lineRenderer = transform.GetComponentInChildren<LineRenderer>();
        lineRenderer.gameObject.SetActive(false);
        cameraController = Camera.main.GetComponent<CameraController>();
        screenUIController = GameObject.FindObjectOfType<ScreenUIController>();
        
        // Disable the map control until the map is generated
        OnMap = false;
    }

    // Update is called once per frame
    void Update() {
        
        if(OnMap) {
            hexUnderMouse = MouseToHex();
            //Debug.Log("hexUnderMouse : " + hexUnderMouse);
            
            if (Input.GetKey(KeyCode.Escape))
            {
                // Escape key just leaves any mode we can be in
                ResetUpdateFunc();
                SelectedUnit = null;
            }
            
            if (Input.GetKey(KeyCode.KeypadEnter) || Input.GetKey(KeyCode.Return))
            {
                screenUIController.OpenMenu();
            }
            if (Input.GetKey(KeyCode.R))
            {
                screenUIController.GenerateWorld(true);
            }
            if( Input.GetKey(KeyCode.S) )
            {
                // Spawn scouts here
                hexMap.SpawnPeopleAt("Testeurs", true, hexUnderMouse);
                screenUIController.ToggleMapInfo(false);
            }
            if(Input.GetKeyDown(KeyCode.Space)) 
            {
                hexMap.NextTurn();
            }
            if(Input.GetKeyDown(KeyCode.Escape)) 
            {
                Application.Quit();
            }
            
            //Keyboard camera control
            Vector3 translate = new Vector3(
                Input.GetAxis("Horizontal"),
                0,
                Input.GetAxis("Vertical")
            );

            cameraController.MoveCamera(translate * keyboardMoveSpeed * Time.deltaTime);
            
            // Display informations of the currently hovered hex
            screenUIController.SelectHex(hexUnderMouse);

            //We call the currently needed Update subfunction
            Update_CurrentFunc();

            lastMousePosition = Input.mousePosition;
            hexLastUnderMouse = hexUnderMouse;

            Update_ScrollZoom();
        }

    }

    // Reset the Update subfunction to Update_DetectMode and cleanup stuff
    void ResetUpdateFunc() {

        Update_CurrentFunc = Update_DetectMode;

        OnMap = true;
        hexPath = null;

        if (SelectedCity != null)
            SelectedCity = null;

    }

    // Update subfunction to detect the appropriate mouse behaviour
    void Update_DetectMode() {
        
        // Check if we are over an UI element
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if( Input.GetMouseButtonDown(0) ) {
            // Left mouse button just went down

        }
        else if( Input.GetMouseButtonUp(0) ) {
            // Left mouse button just went up : it's a click 

            // Are we clicking on a hex with units?
            Unit[] units = hexUnderMouse.Units();
            
            // TODO : implements cycling through units on the tile

            if (units != null && units.Length > 0)
            {
                SelectedUnit = units[0];
                DrawPath(SelectedUnit.GetHexPath());
            }
            else
            {
                SelectedUnit = null;
            }

        }
        else if( Input.GetMouseButton(0) && Input.mousePosition != lastMousePosition ) {
            // Left mouse button is being pressed and the mouse is moving : that's a camera drag!
            lastHitPos = MouseRayHitPos();

            Update_CurrentFunc = Update_CameraDrag;
            Update_CurrentFunc();
        }
        else if( Input.GetMouseButtonDown(1) && SelectedUnit != null)
        {
            // We start selecting a path with this unit with right click pressed

            // Debug.Log("Start unit movement selection mode");
            Update_CurrentFunc = Update_UnitMovement;
        }
    }

    void Update_UnitMovement() {
        // TODO: would be cool if we could continue to move the camera with the left mouse button 

        if (Input.GetMouseButtonUp(0) || SelectedUnit == null)
        {
            // Something else has been clicked or the unit disappeared ; let's exit the mode
            Debug.Log("Exit unit movement selection mode");
            
            ResetUpdateFunc();
            SelectedUnit = null;
            return;
        }
        
        if(Input.GetMouseButtonUp(1)) 
        {
            // Right mouse button up: end of the movement selection
            // Debug.Log("Complete unit movement selection mode");

            // Send the pathfinding to the unit movement queue
            SelectedUnit.SetHexPath(hexPath);

            ResetUpdateFunc();
        }
        
        // We have a selected unit. Let's pathfind to the hex under mouse, and draw the path
        // But first, is this a different hex from before (or we don't already have a path)?
        if (hexPath == null || hexUnderMouse != hexLastUnderMouse)
        {
            // Do a pathfinding search
            hexPath = QPath.QPath.FindPath<Hex>(SelectedUnit, SelectedUnit.Hex, hexUnderMouse, Hex.CostEstimate);
            
            // Draw the path
            DrawPath(hexPath);
        }
    }

    // Update subfunction to move the camera around on the map
    void Update_CameraDrag() {
            
        Vector3 hitPos = MouseRayHitPos();

        if( Input.GetMouseButtonUp(0)) {
            // Mouse button just went up, stop drag
            ResetUpdateFunc();
        }

        Vector3 diff = lastHitPos - hitPos;
        Camera.main.GetComponent<CameraController>().MoveCamera(diff);
        lastHitPos = MouseRayHitPos();
    }

    void Update_ScrollZoom() {

        //Camera zoom / dezoom on scroll
        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        cameraController.ZoomCamera(scrollAmount, MouseRayHitPos());
        //CameraController.ZoomCamera_Old(scrollAmount * scrollSpeed);
    }

    void Update_CityView()
    {
        
    }

    Hex MouseToHex()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(mouseRay, out hitInfo, Mathf.Infinity, LayerIDHexTiles))
        {
            //Something got hit!
            //Debug.Log("MouseToHex() : Something got hit!");
            
            //Let's get the collider parent, the game object, by asking for its rigidbody
            GameObject hexGO = hitInfo.rigidbody.gameObject;
            
            //Let's convert this game object to the more conceptual Hex
            return hexMap.GetHexFromGameObject(hexGO);
        }

        return null;
    }

    public Vector3 MouseRayHitPos() {

    	// "Ray" that emanates from the mouse
    	Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        float rayLength = (mouseRay.origin.y / mouseRay.direction.y);

        return mouseRay.origin - (mouseRay.direction * rayLength);
    }

    void DrawPath(Hex[] hexPath)
    {
        if ((hexPath == null || hexPath.Length == 0) && lineRenderer.enabled == false)
        {
            //Nothing to erase, move along
            return;
        }
        if (hexPath == null || hexPath.Length == 0)
        {
            lineRenderer.gameObject.SetActive(false);
            return;
        }
        lineRenderer.gameObject.SetActive(true);
        
        Vector3[] positions = new Vector3[hexPath.Length];
        for (int i = 0; i < hexPath.Length; i++)
        {
            GameObject hexGO = hexMap.GetGameObjectFromHex(hexPath[i]);
            positions[i] = hexGO.transform.position + (Vector3.up * 0.1f);
        }

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }
}
