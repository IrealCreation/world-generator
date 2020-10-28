using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector3 startPosition;
	private Vector3 oldPosition;
    private Vector3 cameraTargetOffset;

    private bool panorama;

    public float minZ = -5f;
    public float maxZ = 39.5f;
    public float minZoom = 30;
    public float maxZoom = 80;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = this.transform.position;
        oldPosition = this.transform.position;
        cameraTargetOffset = new Vector3();
        
        // Enable Panorama for the start menu
        panorama = true;
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

    void CheckIfCameraMoved() {

    	if(oldPosition != this.transform.position) {

    		oldPosition = this.transform.position;

    		// Update the position of hexes
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
        float minHeight = 5;
        float maxHeight = 30;

        Vector3 newPosition = Camera.main.transform.position;
        
        // Move camera towards hitPos
        Vector3 dir = hitPos - newPosition;

        // Stop zooming out at a certain distance.
        if (scrollAmount > 0 || newPosition.y < (maxHeight - 0.1f)) {
            cameraTargetOffset += dir * scrollAmount;
        }
        newPosition = Vector3.Lerp(newPosition, newPosition + cameraTargetOffset, Time.deltaTime * 5f);
        cameraTargetOffset -= newPosition - Camera.main.transform.position;
        
        if (newPosition.y < minHeight) {
            newPosition.y = minHeight;
        }
        if (newPosition.y > maxHeight) {
            newPosition.y = maxHeight;
        }
        Camera.main.transform.position = newPosition;

        // Change camera angle
        Camera.main.transform.rotation = Quaternion.Euler (
            Mathf.Lerp (30, 75, newPosition.y / maxHeight),
            Camera.main.transform.rotation.eulerAngles.y,
            Camera.main.transform.rotation.eulerAngles.z
        );
    }

    public void ZoomCamera_Old(float zoom) {

        if((zoom > 0.01f && Camera.main.fieldOfView < maxZoom) || (zoom < 0.01f && Camera.main.fieldOfView > minZoom)) {
            Camera.main.fieldOfView += zoom;
        }
    }

    public void ResetCamera()
    {
        panorama = false;
        Camera.main.transform.position = startPosition;
        
        //TODO: reset zoom
    }

    public Vector3 GetPosition()
    {
        return this.transform.position;
    }
}
