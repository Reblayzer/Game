using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SelectableCuboid : MonoBehaviour
{
    [Header("Cuboid Data")]
    public string cuboidName;
    public int level = 1;

    [Header("UI References")]
    public GameObject infoPanel;
    public TMP_Text infoDisplay;
    public Button upgradeButton;

    private void Start()
    {
        // Hide UI at start
        if (infoPanel != null)
            infoPanel.SetActive(false);

        // Optional Debug
        Debug.Log($"{name} initialized with level {level}");
    }

    private void OnMouseDown()
    {
        Debug.Log($"{cuboidName} selected");

        if (infoPanel == null || infoDisplay == null || upgradeButton == null)
        {
            Debug.LogError("❌ UI references are still missing on click!");
            return;
        }

        // Show panel
        infoPanel.SetActive(true);

        // Update text
        infoDisplay.text = $"{cuboidName}\nLevel: {level}";

        // Clear old listeners and add this one
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(OnUpgradeClicked);
    }

    private void OnUpgradeClicked()
    {
        level++;
        infoDisplay.text = $"{cuboidName}\nLevel: {level}";
        Debug.Log($"{cuboidName} upgraded to Level {level}");
    }
}
