using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class NameplateBehaviour : MapUIBehaviour, IPointerClickHandler
{
    public Text Name;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (mapObjectTarget.GetType() == typeof(City))
            GameObject.FindObjectOfType<InputController>().SelectedCity = (City)mapObjectTarget;
    }
}
