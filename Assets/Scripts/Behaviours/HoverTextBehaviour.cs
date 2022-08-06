using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverTextBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string Text = "Hover Text";
    private bool active;

    public void Start()
    {
        active = false;
    }

    public void Update()
    {
        if (active)
        {
            GameController.Main.HoverTextPopup.transform.position = Input.mousePosition + new Vector3(0,10,0);
        }
    }

    public void OnDisable()
    {
        HideHoverText();
    }

    public void OnDestroy()
    {
        HideHoverText();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!active)
        {
            ShowHoverText();
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (active)
        {
            HideHoverText();
        }
    }

    public void ShowHoverText()
    {
        active = true;
        GameController.Main.HoverTextPopup.GetComponentInChildren<Text>().text = Text;
        GameController.Main.HoverTextPopup.SetActive(true);
    }

    public void HideHoverText()
    {
        if (active)
        {
            GameController.Main.HoverTextPopup.SetActive(false);
            active = false;
        }
    }
}