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

        // 2) Always tell our PlotSelector to switch plots
        var gm = GetComponentInParent<GridManager>();
        if (gm != null && PlotSelector.Instance != null)
            PlotSelector.Instance.SelectPlot(gm);

        // 3) If the map is open, skip the little marker‐canvas toggle,
        //    but we've already selected the plot above.
        if (MapUIController.I != null && MapUIController.I.IsMapOpen)
            return;

        // 4) Now do your existing marker toggle:
        if (markerCanvas == null) return;

        // clicking same plot again hides its marker
        if (markerCanvas.activeSelf)
        {
            markerCanvas.SetActive(false);
            return;
        }

        // hide any other
        var all = Object.FindObjectsByType<PlotTriggerController>(
            FindObjectsInactive.Include, FindObjectsSortMode.None
        );
        foreach (var other in all)
            if (other != this && other.markerCanvas != null)
                other.markerCanvas.SetActive(false);

        // show ours
        markerCanvas.SetActive(true);
    }
}