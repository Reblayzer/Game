using UnityEngine;
using TMPro;

public class Tooltip : MonoBehaviour
{
    public TextMeshProUGUI textObject;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        Hide();
    }

    public void Show(string message, Vector3 worldPosition)
    {
        if (textObject == null) return;

        textObject.text = message;

        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        rectTransform.position = screenPos;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
