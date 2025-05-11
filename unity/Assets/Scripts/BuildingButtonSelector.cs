using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class BuildingButtonSelector : MonoBehaviour
{
    [System.Serializable]
    public class BuildingButton
    {
        public GameObject rootObject;
        public Image backgroundImage;
    }

    public List<BuildingButton> buildingButtons;

    [Header("Special Buttons")]
    public BuildingButton bridgeButton;


    [Header("Panel Buttons Colors")]
    public Color normalButtonColor;
    public Color selectedButtonColor;

    [Header("Tile Colors")]
    public Color abandonedPlotColor;
    public Color voidPlotColor;
    public Color normalTileColor;
    public Color selectedTileColor;
    public Color ghostCanPlaceColor;
    public Color ghostCanNotPlaceColor;
    public Color hoverHighlightPlot;

    [Header("the “owned” plot label")]
    public TMP_Text ownedPlotLabel;

    [Header("the “buy” plot label")]
    public TMP_Text buyPlotLabel;
    [Header("the “build” plot label")]
    public TMP_Text buildPlotLabel;
    private int currentIndex = -1;
    private GridManager activeGridManager;
    private GridManager currentlyHoveredPlot;

    public bool IsInEditMode { get; private set; }
    public int CurrentIndex => currentIndex;

    void Update()
    {
        HandlePlotHover();
    }

    public void SetActiveGridManager(GridManager gm)
    {
        StartCoroutine(SwitchPlotCoroutine(gm));
    }

    public GridManager GetActiveGridManager()
    {
        return activeGridManager;
    }

    private IEnumerator SwitchPlotCoroutine(GridManager gm)
    {
        yield return new WaitUntil(() => gm != null && gm.IsInitialized);
        activeGridManager = gm;

        foreach (GridManager plot in FindObjectsByType<GridManager>(FindObjectsSortMode.None))
        {
            if (plot != activeGridManager)
            {
                plot.SetActive(false);
                RestorePlotBaseColor(plot);
            }
        }

        activeGridManager.HighlightPlot(selectedTileColor);
        activeGridManager.SetActive(true);

        bool isUnclaimed = gm.ownership == Ownership.Unclaimed;

        ownedPlotLabel.gameObject.SetActive(!isUnclaimed);
        buyPlotLabel.gameObject.SetActive(isUnclaimed);

        TMP_Text label = isUnclaimed ? buyPlotLabel : ownedPlotLabel;
        if (label != null)
        {
            const string plotName = "MMMMMMMMMMMMMMM";
            int row = gm.plotRow;
            int col = gm.plotCol;
            label.text = $"{plotName} {row:00} | {col:00}";
        }

        if (buildPlotLabel != null)
        {
            buildPlotLabel.text = label.text;
        }

        UpdatePanelButtonsVisibility();

        if (currentIndex >= 0 && currentIndex < buildingButtons.Count)
            activeGridManager.SetSelectedCuboid(currentIndex);

        PlotSelector.Instance?.UpdateBuildingsButton();
    }

    private void UpdatePanelButtonsVisibility()
    {
        bool isVoid = activeGridManager != null && activeGridManager.plotType == PlotType.Void;

        foreach (var button in buildingButtons)
        {
            if (button?.rootObject != null)
                button.rootObject.SetActive(!isVoid);
        }

        if (bridgeButton?.rootObject != null)
            bridgeButton.rootObject.SetActive(isVoid);
    }

    public void SelectByIndex(int index)
    {
        for (int i = 0; i < buildingButtons.Count; i++)
        {
            if (buildingButtons[i]?.backgroundImage != null)
                buildingButtons[i].backgroundImage.color = (i == index) ? selectedButtonColor : normalButtonColor;
        }

        if (currentIndex == index)
        {
            ClearSelection();
            return;
        }

        currentIndex = index;

        if (activeGridManager != null)
            activeGridManager.SetSelectedCuboid(index);
    }

    public void ClearSelection()
    {
        foreach (var button in buildingButtons)
        {
            if (button?.backgroundImage != null)
                button.backgroundImage.color = normalButtonColor;
        }

        currentIndex = -1;

        if (activeGridManager != null)
            activeGridManager.ClearCuboidSelection();
    }

    public void ToggleEditMode(bool state)
    {
        IsInEditMode = state;

        if (!state)
            ClearSelection();

        if (activeGridManager != null)
            activeGridManager.SetEditMode(state);
    }

    private void HandlePlotHover()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int mask = LayerMask.GetMask("Plot");

        bool didHit = Physics.Raycast(ray, out var hit, 100f, mask);

        var selected = activeGridManager;
        foreach (var other in Object.FindObjectsByType<GridManager>(
                        FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (other == selected) continue;

            Color baseCol = other.plotType == PlotType.Abandoned ? abandonedPlotColor
                         : other.plotType == PlotType.Void ? voidPlotColor
                         : normalTileColor;
            other.HighlightPlot(baseCol);
        }

        if (didHit)
        {
            var hovered = hit.collider.GetComponentInParent<GridManager>();
            if (hovered != null && hovered != selected)
            {
                hovered.HighlightPlot(hoverHighlightPlot);
            }
            return;
        }
    }

    private void RestorePlotBaseColor(GridManager plot)
    {
        if (plot == GetActiveGridManager())
        {
            plot.HighlightPlot(selectedTileColor);
            return;
        }

        switch (plot.plotType)
        {
            case PlotType.Abandoned:
                plot.HighlightPlot(abandonedPlotColor);
                break;
            case PlotType.Void:
                plot.HighlightPlot(voidPlotColor);
                break;
            default:
                plot.HighlightPlot(normalTileColor);
                break;
        }
    }
}
