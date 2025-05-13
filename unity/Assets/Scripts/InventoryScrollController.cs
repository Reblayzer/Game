using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryScrollController : MonoBehaviour
{
  [SerializeField] private RectTransform content;

  void OnEnable()
  {
    WarehouseData.Instance.OnInventoryChanged += UpdateAll;
    // also populate immediately in case there’s already data
    UpdateAll(WarehouseData.Instance.GetAllStored());
  }

  void OnDisable()
  {
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
