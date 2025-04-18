using UnityEngine;
using UnityEngine.EventSystems;

public class UIRaycastClickHandler : MonoBehaviour, IPointerClickHandler
{
    public BuildingButtonSelector selector;
    public int buttonIndex;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (selector != null)
        {
            selector.SelectByIndex(buttonIndex);
        }
    }
}
