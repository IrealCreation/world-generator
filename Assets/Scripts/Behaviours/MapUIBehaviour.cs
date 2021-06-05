
using UnityEngine;

public class MapUIBehaviour : MonoBehaviour
{
    protected MapObject mapObjectTarget;
    protected Hex hexTarget;
    protected Camera mainCamera;
    protected CameraController cameraController;
    public MapUIController MapUIController;
    
    public Vector3 ScreenPositionOffset = new Vector3(0, 30, 0);
    public Vector3 WorldPositionOffset = new Vector3(0, 1, 0);
    public float MinZoomScale = 1f;
    public float MaxZoomScale = 1f;

    protected RectTransform rectTransform;
    protected new Renderer renderer; // Renderer of the target. We hide a deprecated property, hence the "new"
    
    // Start is called before the first frame update
    void Start()
    {
        if(mainCamera == null)
            mainCamera = Camera.main;
        cameraController = mainCamera.GetComponent<CameraController>();

        rectTransform = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (mapObjectTarget != null && mapObjectTarget.GO == null)
        {
            Debug.Log("Destruction of MapUIBehaviour");
            // The object we're supposed to track as been removed: let's autodestruct
            Destroy(this.gameObject);
            return;
        }

        //Find out the screen position of our target gameobject and set ourselves to that, plus offset
        Vector3 targetPosition;
        if (mapObjectTarget != null)
            targetPosition = mapObjectTarget.GO.transform.position;
        else
            targetPosition = hexTarget.GO.transform.position;
        
        float distance = Vector3.Distance(targetPosition, mainCamera.transform.position);

        // Scale the size of the UI depending on the camera distance
        float scale = Mathf.Lerp(MaxZoomScale, MinZoomScale,
            (distance - cameraController.minZoom) /
            (cameraController.maxZoom - cameraController.minZoom));
        rectTransform.localScale = new Vector3(scale, scale, scale);

        Vector3 screenPos = mainCamera.WorldToScreenPoint(targetPosition + WorldPositionOffset);
        
        // Update ScreenPositionOffset depending on size of the UI element to center it (not ideal, but dynamic)
        Vector3 offset = ScreenPositionOffset;
        offset.x -= rectTransform.rect.width / 2 * scale;
        offset.y -= rectTransform.rect.height / 2 * scale;

        rectTransform.anchoredPosition = screenPos + offset;
    }

    public void SetMapObjectTarget(MapObject mo)
    {
        mapObjectTarget = mo;
        renderer = mo.GO.GetComponent<Renderer>();
    }

    public void SetHexTarget(Hex hex)
    {
        hexTarget = hex;
        renderer = hex.GO.GetComponentInChildren<Renderer>();
    }

    public bool TargetIsVisible()
    {
        if (renderer == null)
        {
            Debug.Log("MapUIBehaviour without renderer");
            return false;
        }

        return renderer.isVisible;
    }
}