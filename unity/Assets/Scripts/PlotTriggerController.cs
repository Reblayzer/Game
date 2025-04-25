using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class PlotTriggerController : MonoBehaviour
{
    [HideInInspector] public GameObject markerCanvas;
    private static PlotTriggerController s_current;

    void OnMouseDown()
    {
        if (EventSystem.current != null
            && EventSystem.current.IsPointerOverGameObject())
            return;

        if (MapUIController.I != null && MapUIController.I.IsMapOpen)
            return;

        if (s_current == this)
        {
            markerCanvas.SetActive(false);
            s_current = null;
            return;
        }

        if (s_current != null)
            s_current.markerCanvas.SetActive(false);

        markerCanvas.SetActive(true);
        s_current = this;
    }
}
