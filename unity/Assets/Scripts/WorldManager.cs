using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class WorldManager : MonoBehaviour
{
    [Header("UI References")]
    public Button mapButton;
    public Button plotButton;
    public Button miningToolsButton;
    public Button miningDrillsButton;
    public TMP_Dropdown plotDropdown;
    public TMP_Text balanceValueText;
    public Transform plotParent;
    public GameObject plotPrefab;

    [Header("Windows")]
    public GameObject mapWindow;
    public GameObject miningToolsWindow;
    public GameObject miningDrillsWindow;

    [Header("Grid Settings")]
    public int gridSize = 5;
    public int basePlotCost = 200;
    public float priceIncreaseFactor = 1.5f;
    public int abandonedPlotCount = 5;

    private WorldData worldData;

    private HashSet<string> abandonedPlots = new();
    private List<GameObject> generatedPlotButtons = new();
    public GameObject plotWindowPrefab;
    public Transform plotWindowParent;
    private Dictionary<string, GameObject> plotWindows = new();

    void Start()
    {
        plotParent.gameObject.SetActive(true);
        HideAllWindows();

        mapButton.onClick.AddListener(() => ToggleWindow(mapWindow));
        miningToolsButton.onClick.AddListener(() => ToggleWindow(miningToolsWindow));
        miningDrillsButton.onClick.AddListener(() => ToggleWindow(miningDrillsWindow));

        worldData = SessionManager.Instance.currentUser.worlds[SessionManager.Instance.currentWorld];

        GenerateAbandonedPlots();
        GeneratePlots();
        UpdatePlotDropdown();
        plotDropdown.onValueChanged.AddListener(OnPlotSelected);

        UpdateBalanceText();
    }

    void GenerateAbandonedPlots()
    {
        while (abandonedPlots.Count < abandonedPlotCount)
        {
            int rand = Random.Range(1, gridSize * gridSize + 1);
            abandonedPlots.Add($"Plot{rand}");
        }
    }

    void GeneratePlots()
    {
        for (int i = 1; i <= gridSize * gridSize; i++)
        {
            string plotName = $"Plot{i}";
            GameObject plotObj = Instantiate(plotPrefab);
            plotObj.name = plotName;
            plotObj.transform.SetParent(plotParent, false);

            RectTransform rect = plotObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.localScale = Vector3.one;
                rect.anchoredPosition3D = Vector3.zero;
                rect.localRotation = Quaternion.identity;
                rect.sizeDelta = new Vector2(90, 90);
            }

            TMP_Text plotLabel = plotObj.transform.Find("Plot-Label").GetComponent<TMP_Text>();
            TMP_Text costLabel = plotObj.transform.Find("Cost-Label").GetComponent<TMP_Text>();
            TMP_Text costValue = plotObj.transform.Find("Cost-Value").GetComponent<TMP_Text>();
            Button plotButton = plotObj.GetComponent<Button>();

            plotLabel.text = plotName;

            if (abandonedPlots.Contains(plotName))
            {
                costLabel.text = "Abandoned";
                costValue.text = "";
                plotButton.interactable = false;
                plotButton.GetComponent<Image>().color = Color.gray;
                continue;
            }

            if (!worldData.plotCosts.ContainsKey(plotName))
                worldData.plotCosts[plotName] = basePlotCost;

            int cost = worldData.plotCosts[plotName];
            bool isOwned = worldData.ownedPlots.Contains(plotName);

            if (isOwned)
            {
                costLabel.text = "Owned";
                costValue.text = "";
                plotButton.interactable = false;
                plotButton.GetComponent<Image>().color = Color.green;
            }
            else
            {
                costLabel.text = "Cost:";
                costValue.text = cost.ToString();
                plotButton.onClick.AddListener(() => PurchasePlot(plotName, plotButton, costLabel, costValue));
            }

            generatedPlotButtons.Add(plotObj);
        }
    }

    void PurchasePlot(string plotName, Button button, TMP_Text costLabel, TMP_Text costValue)
    {
        int cost = worldData.plotCosts[plotName];
        var treasury = SessionManager.Instance;

        if (worldData.balance < cost)
        {
            Debug.LogWarning($"❌ Not enough balance to buy {plotName}.");
            return;
        }

        worldData.balance -= cost;
        worldData.ownedPlots.Add(plotName);
        if (!plotWindows.ContainsKey(plotName))
        {
            GameObject window = Instantiate(plotWindowPrefab, plotWindowParent);
            window.name = $"{plotName}-Window";
            window.SetActive(false);

            TMP_Text label = window.transform.Find("Plot-Label").GetComponent<TMP_Text>();
            label.text = plotName;

            plotWindows[plotName] = window;
        }

        treasury.treasuryOwnedPlots.Remove(plotName);
        treasury.treasuryBalance += cost;

        costLabel.text = "Owned";
        costValue.text = "";
        button.interactable = false;
        button.GetComponent<Image>().color = Color.green;

        List<string> plotKeys = new(worldData.plotCosts.Keys);

        foreach (string other in plotKeys)
        {
            if (!worldData.ownedPlots.Contains(other))
            {
                int newCost = Mathf.CeilToInt(worldData.plotCosts[other] * priceIncreaseFactor);
                worldData.plotCosts[other] = newCost;
            }
        }

        foreach (GameObject plotObj in generatedPlotButtons)
        {
            string name = plotObj.name;
            if (!worldData.ownedPlots.Contains(name) && worldData.plotCosts.ContainsKey(name))
            {
                TMP_Text costValueText = plotObj.transform.Find("Cost-Value").GetComponent<TMP_Text>();
                costValueText.text = worldData.plotCosts[name].ToString();
            }
        }

        UpdatePlotDropdown();
        UpdateBalanceText();

        // ✅ Make sure dropdown stays reset — no plot auto-selected
        plotDropdown.onValueChanged.RemoveListener(OnPlotSelected);
        plotDropdown.value = -1;
        plotDropdown.captionText.text = "Plots";
        plotDropdown.RefreshShownValue();
        plotDropdown.onValueChanged.AddListener(OnPlotSelected);
    }

    void UpdatePlotDropdown()
    {
        plotDropdown.onValueChanged.RemoveListener(OnPlotSelected);
        plotDropdown.ClearOptions();

        List<TMP_Dropdown.OptionData> options = new();
        options.Add(new TMP_Dropdown.OptionData("Plots")); // 👈 Placeholder first

        foreach (var plot in worldData.ownedPlots)
        {
            options.Add(new TMP_Dropdown.OptionData(plot));
        }

        plotDropdown.options = options;
        plotDropdown.value = 0; // Always start on "Plots"
        plotDropdown.captionText.text = "Plots";
        plotDropdown.RefreshShownValue();

        plotDropdown.gameObject.SetActive(worldData.ownedPlots.Count > 0);
        plotButton.gameObject.SetActive(worldData.ownedPlots.Count == 0);

        plotDropdown.onValueChanged.AddListener(OnPlotSelected);
    }

    void OnPlotSelected(int index)
    {
        if (index <= 0 || index >= plotDropdown.options.Count) return; // index 0 = "Plots"

        string selectedPlot = plotDropdown.options[index].text;

        HideAllWindows();

        if (plotWindows.TryGetValue(selectedPlot, out GameObject plotWin))
        {
            plotWin.SetActive(true);
            Debug.Log($"📍 Showing window for: {selectedPlot}");
        }

        plotDropdown.value = 0; // Reset to "Plots"
        plotDropdown.captionText.text = "Plots";
        plotDropdown.RefreshShownValue();
    }

    void HideAllWindows()
    {
        mapWindow.SetActive(false);
        miningToolsWindow.SetActive(false);
        miningDrillsWindow.SetActive(false);

        foreach (var window in plotWindows.Values)
            window.SetActive(false);
    }

    void ToggleWindow(GameObject window)
    {
        HideAllWindows();

        // Also clear dropdown state
        plotDropdown.onValueChanged.RemoveListener(OnPlotSelected);
        plotDropdown.value = -1;
        plotDropdown.captionText.text = "Plots";
        plotDropdown.RefreshShownValue();
        plotDropdown.onValueChanged.AddListener(OnPlotSelected);

        window.SetActive(true);
    }

    void UpdateBalanceText()
    {
        balanceValueText.text = worldData.balance.ToString();
    }

    public void GoBack()
    {
        SceneManager.LoadScene("WorldSelection");
    }
}
