using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class InventoryToggleController : MonoBehaviour
{
    public GameObject inventoryScrollView;

    public GameObject plotInfoPanel;

    Toggle _toggle;

    void Awake()
    {
        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(OnToggleChanged);

        OnToggleChanged(_toggle.isOn);
    }

    void OnDestroy()
    {
        _toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    void OnToggleChanged(bool isOn)
    {
        if (inventoryScrollView != null)
            inventoryScrollView.SetActive(isOn);

        if (plotInfoPanel != null)
            plotInfoPanel.SetActive(!isOn);
    }
}
