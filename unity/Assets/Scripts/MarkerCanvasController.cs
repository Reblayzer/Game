using UnityEngine;
using UnityEngine.UI;

public class MarkerCanvasController : MonoBehaviour
{
    private Canvas _canvas;

    void Awake()
    {
        _canvas = GetComponent<Canvas>();
        var btn = transform.Find("MarkerAid")?.GetComponent<Button>();
    }

    public void ToggleVisibility()
    {
        _canvas.gameObject.SetActive(!_canvas.gameObject.activeSelf);
    }
}
