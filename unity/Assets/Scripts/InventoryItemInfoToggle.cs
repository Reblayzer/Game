using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class InventoryItemInfoToggle : MonoBehaviour
{
  public GameObject infoPanel;

  private Toggle _toggle;

  void Awake()
  {
    _toggle = GetComponent<Toggle>();

    _toggle.onValueChanged.AddListener(isOn =>
    {
      if (infoPanel != null)
        infoPanel.SetActive(isOn);
    });

    if (infoPanel != null)
      infoPanel.SetActive(_toggle.isOn);
  }

  void OnDestroy()
  {
    _toggle.onValueChanged.RemoveAllListeners();
  }
}