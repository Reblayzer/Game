using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class PlotTriggerController : MonoBehaviour
{
    [HideInInspector] public GameObject markerCanvas;
    public Ownership ownership = Ownership.Unclaimed;

    void OnMouseDown()
    {
        // 1) Ignore clicks over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // 2) Ignore if the map is up
        if (MapUIController.I != null && MapUIController.I.IsMapOpen)
            return;

        // 3) Tell our central PlotSelector
        var gm = GetComponentInParent<GridManager>();
        if (gm != null && PlotSelector.Instance != null)
            PlotSelector.Instance.SelectPlot(gm);

        // 4) Marker‐canvas toggle
        if (markerCanvas == null) return;

        if (markerCanvas.activeSelf)
        {
            markerCanvas.SetActive(false);
            return;
        }

        // hide any others
        var others = Object.FindObjectsByType<PlotTriggerController>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
        foreach (var other in others)
            if (other != this && other.markerCanvas != null)
                other.markerCanvas.SetActive(false);

        markerCanvas.SetActive(true);
    }
}