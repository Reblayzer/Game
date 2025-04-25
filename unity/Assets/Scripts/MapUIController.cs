using UnityEngine;
using UnityEngine.UI;

public class MapUIController : MonoBehaviour
{
    public Toggle mapToggle;
    public static MapUIController I { get; private set; }

    void Awake()
    {
        I = this;
    }

    public bool IsMapOpen => mapToggle != null && mapToggle.isOn;

    public void ToggleAllMarkers(bool show)
    {
        var all = FindObjectsOfType<MarkerCanvasController>(includeInactive: true);
        foreach (var c in all)
            c.gameObject.SetActive(show);
    }
}
