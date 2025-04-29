using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class BlueprintToggle : MonoBehaviour
{
    [Tooltip("The panel that shows this blueprint’s details (e.g. WarehouseInfo)")]
    public GameObject infoPanel;

    Toggle _toggle;

    void Awake()
    {
        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(SetInfoPanelActive);
        SetInfoPanelActive(_toggle.isOn);
    }

    void OnDestroy()
    {
        _toggle.onValueChanged.RemoveListener(SetInfoPanelActive);
    }

    private void SetInfoPanelActive(bool isOn)
    {
        if (infoPanel != null)
            infoPanel.SetActive(isOn);
    }
}
