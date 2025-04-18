using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectableCuboid : MonoBehaviour
{
    public string cuboidName;
    public GameObject infoPanel;
    public TMP_Text infoDisplay;
    public Button upgradeButton;

    private int level = 1;

    void OnMouseDown()
    {
        // Fallback: Get UI refs from the active grid
        if (infoPanel == null || infoDisplay == null || upgradeButton == null)
        {
            GridManager activeGM = FindFirstObjectByType<BuildingButtonSelector>()?.GetActiveGridManager();
            if (activeGM != null)
            {
                infoPanel = activeGM.SelectedCuboidUIPanel;
                infoDisplay = activeGM.SelectedCuboidInfoText;
                upgradeButton = activeGM.UpgradeButton;
            }
        }

        if (infoPanel == null || infoDisplay == null || upgradeButton == null)
        {
            Debug.LogError("❌ Missing UI references on cuboid selection.");
            return;
        }

        infoPanel.SetActive(true);
        upgradeButton.gameObject.SetActive(true);

        infoDisplay.text = $"{cuboidName} (Level {level})";

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(() =>
        {
            level++;
            infoDisplay.text = $"{cuboidName} (Level {level})";
        });

        Debug.Log($"✅ {cuboidName} selected and upgrade UI ready.");
    }
}
