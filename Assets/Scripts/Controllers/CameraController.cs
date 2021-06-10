using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector3 startPosition;
	private Vector3 oldPosition;
    private Vector3 cameraTargetOffset;
    private HexMap hexMap;

    private bool panorama;

    float minZ = -5;
    float maxZ; //numRows * 1.25
    public float minZoom;
    public float maxZoom;

    // Start is called before the first frame update
    void Start()
    {
        hexMap = GameObject.FindObjectOfType<HexMap>();
        SetLimits(hexMap.numRows);
        startPosition = this.transform.position;
        oldPosition = this.transform.position;
        cameraTargetOffset = new Vector3();
        
        // Enable Panorama for the start menu
        ResetCamera(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (panorama)
        {
            Vector3 translate = new Vector3(
                3,
                0,
                0
            );
            MoveCamera(translate * Time.deltaTime);
        }
        CheckIfCameraMoved();
    }

    public void SetLimits(int numRows)
    {
        maxZ = numRows * 1.25f;
    }

    void CheckIfCameraMoved() {

    	if(oldPosition != this.transform.position) {

    		oldPosition = this.transform.position;

    		// Update the position of hexes
            // TODO: optimize this by getting the behaviours from hexMap
    		HexBehaviour[] hexes = GameObject.FindObjectsOfType<HexBehaviour>();

    		foreach(HexBehaviour hex in hexes) {
    			hex.UpdatePosition();
    		}
    	}
    }

    public void MoveCamera(Vector3 movement) {

        //Check top and bottom limits
        if(Camera.main.transform.position.z + movement.z < minZ) {
            movement.z = minZ - Camera.main.transform.position.z;
        }
        else if(Camera.main.transform.position.z + movement.z > maxZ){
            movement.z = maxZ - Camera.main.transform.position.z;
        }

        Camera.main.transform.Translate(movement, Space.World);
    }

    public void ZoomCamera(float scrollAmount, Vector3 hitPos)
    {
        Vector3 newPosition = Camera.main.transform.position;
        
        // Move camera towards hitPos
        Vector3 dir = hitPos - newPosition;

        // Stop zooming out at a certain distance.
        if (scrollAmount > 0 || newPosition.y < (maxZoom - 0.1f)) {
            cameraTargetOffset += dir * scrollAmount;
        }
        newPosition = Vector3.Lerp(newPosition, newPosition + cameraTargetOffset, Time.deltaTime * 5f);
        cameraTargetOffset -= newPosition - Camera.main.transform.position;
        
        if (newPosition.y < minZoom) {
            newPosition.y = minZoom;
        }
        if (newPosition.y > maxZoom) {
            newPosition.y = maxZoom;
        }
        Camera.main.transform.position = newPosition;

        // Change camera angle
        Camera.main.transform.rotation = Quaternion.Euler (
            Mathf.Lerp (30, 85, newPosition.y / maxZoom),
            Camera.main.transform.rotation.eulerAngles.y,
            Camera.main.transform.rotation.eulerAngles.z
        );
    }

    public void ZoomCamera_Old(float zoom) {

        if((zoom > 0.01f && Camera.main.fieldOfView < maxZoom) || (zoom < 0.01f && Camera.main.fieldOfView > minZoom)) {
            Camera.main.fieldOfView += zoom;
        }
    }

    public void ResetCamera(bool panorama = false)
    {
        startPosition.z = (minZ + maxZ) / 2f;
        this.panorama = panorama;
        Camera.main.transform.position = startPosition;
        // TODO: calculate the ideal initial camera position
    }

    public Vector3 GetPosition()
    {
        return this.transform.position;
    }

    public float GetZoom()
    {
        return this.transform.position.y;
    }
}
