using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryScrollController : MonoBehaviour
{
  [SerializeField] private RectTransform content;

  void Awake()
  {
    // subscribe once and for all, even if we hide the panel
    WarehouseData.Instance.OnInventoryChanged += UpdateAll;
    // immediately fill it with whatever is already in the warehouse
    UpdateAll(WarehouseData.Instance.GetAllStored());
  }

  void OnDestroy()
  {
    // if you really want to clean up:
    WarehouseData.Instance.OnInventoryChanged -= UpdateAll;
  }

  private void UpdateAll(int[] totals)
  {
    for (int i = 0; i < content.childCount; i++)
    {
      var slot = content.GetChild(i);
      var label = slot.Find("AmountLabel").GetComponent<TMP_Text>();
      label.text = (i < totals.Length ? totals[i] : 0).ToString("N0");
    }
  }
}
