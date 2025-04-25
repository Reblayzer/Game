using UnityEngine;

public class MapUIController : MonoBehaviour
{
    public void ToggleAllMarkers(bool show)
    {
        // includeInactive:true will pick up all your disabled canvases
        var all = FindObjectsOfType<MarkerCanvasController>(includeInactive: true);
        foreach (var c in all)
            c.gameObject.SetActive(show);
    }
}
