using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class NameplateBehaviour : MonoBehaviour, IPointerClickHandler
{

    public MapObject Target;
    public Camera TheCamera;
    public Text Name;
    
    public Vector3 ScreenPositionOffset = new Vector3(0, 30, 0);
    public Vector3 WorldPositionOffset = new Vector3(0, 1, 0);

    RectTransform rectTransform;
    
    // Start is called before the first frame update
    void Start()
    {
        if(TheCamera == null)
            TheCamera = Camera.main;

        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Target.GO == null)
        {
            // The object we're supposed to track as been removed: let's autodestruct
            Destroy(this.gameObject);
            return;
        }

        //Find out the screen position of our target gameobject and set ourselves to that, plus offset

        Vector3 screenPos = TheCamera.WorldToScreenPoint(Target.GO.transform.position + WorldPositionOffset);

        rectTransform.anchoredPosition = screenPos + ScreenPositionOffset;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Target.GetType() == typeof(City))
            GameObject.FindObjectOfType<InputController>().SelectedCity = (City)Target;
    }
}
