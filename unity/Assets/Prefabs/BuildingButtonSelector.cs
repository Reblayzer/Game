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

    public TMP_Text plotLabel;
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

        if (plotLabel != null)
        {
            const string plotName = "MMMMMMMMMMMMMMM";
            int row = gm.plotRow;
            int col = gm.plotCol;
            plotLabel.text = $"{plotName} {row:00} | {col:00}";
        }
        UpdatePanelButtonsVisibility();

        if (currentIndex >= 0 && currentIndex < buildingButtons.Count)
            activeGridManager.SetSelectedCuboid(currentIndex);
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
                buildingButtons[i].backgroundImage.color = (i == index) ? selectedButtonColor : normalButtonColor; // ✅ Fixed
        }

        if (currentIndex == index)
        {
            ClearSelection(); // Clicking again deselects
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
                button.backgroundImage.color = normalButtonColor; // ✅ Fixed
        }

        currentIndex = -1;

        if (activeGridManager != null)
            activeGridManager.ClearCuboidSelection(); // This will hide the ghost
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

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Plot")))
        {
            GridManager hovered = hit.collider.GetComponentInParent<GridManager>();
            GridManager selected = GetActiveGridManager();

            if (hovered != null)
            {
                if (currentlyHoveredPlot != null && currentlyHoveredPlot != hovered)
                    RestorePlotBaseColor(currentlyHoveredPlot); // Always restore old

                if (hovered != selected)
                {
                    currentlyHoveredPlot = hovered;
                    currentlyHoveredPlot.HighlightPlot(hoverHighlightPlot);
                }
                else
                {
                    currentlyHoveredPlot = null; // Stop tracking if hovering selected
                }

                return;
            }
        }

        // If we're not hovering any plot
        if (currentlyHoveredPlot != null)
        {
            RestorePlotBaseColor(currentlyHoveredPlot);
            currentlyHoveredPlot = null;
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
