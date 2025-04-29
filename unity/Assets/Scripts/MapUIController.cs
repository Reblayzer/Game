using UnityEngine;
using UnityEngine.UI;

public class MapUIController : MonoBehaviour
{
    [Header("UI")]
    public Toggle mapToggle;
    public PlotSelector plotSelector;

    public static MapUIController I { get; private set; }

    void Awake()
    {
        if (I == null) I = this;

        if (mapToggle != null)
            mapToggle.onValueChanged.AddListener(OnMapToggled);
    }

    public bool IsMapOpen => mapToggle != null && mapToggle.isOn;

    private void OnMapToggled(bool isOpen)
    {
        ToggleAllMarkers(isOpen);
        plotSelector?.UpdateBuildingsButton();
    }

    public void ToggleAllMarkers(bool show)
    {
        var all = Object.FindObjectsByType<MarkerCanvasController>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var c in all)
            c.gameObject.SetActive(show);
    }
}
