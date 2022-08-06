
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChoiceOptionPanelBehaviour : MonoBehaviour, IPointerClickHandler
{
    private ChoiceOption option;
    private Choice choice;
    private People people;

    public void SetChoiceOption(ChoiceOption option, Choice choice, People people)
    {
        this.option = option;
        this.choice = choice;
        this.people = people;
        transform.Find("Description").GetComponent<Text>().text = option.Description;
        gameObject.GetComponent<HoverTextBehaviour>().Text = option.GetEffects();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameController.Main.MakeChoice(option, choice, people);
    }
}