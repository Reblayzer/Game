using UnityEngine;
using UnityEngine.UI;

public class MarkerCanvasController : MonoBehaviour
{
    Canvas _canvas;
    Button _btn;

    void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _btn = transform.Find("MarkerAid")?.GetComponent<Button>();
        if (_btn != null)
            _btn.onClick.AddListener(OnMarkerClicked);
    }

    void OnMarkerClicked()
    {
        if (PlotSelector.Instance.buildToggle.isOn)
            return;

        if (MapUIController.I != null && MapUIController.I.IsMapOpen)
            return;

        _canvas.gameObject.SetActive(!_canvas.gameObject.activeSelf);
    }
}
