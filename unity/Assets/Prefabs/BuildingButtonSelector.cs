using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    [Header("Panel Buttons Colors")]
    public Color normalButtonColor;
    public Color selectedButtonColor;

    [Header("Tile Colors")]
    public Color normalTileColor;
    public Color selectedTileColor;
    public Color ghostCanPlaceColor;
    public Color ghostCanNotPlaceColor;
    public Color hoverHighlightPlot;

    public TMP_Text plotLabel;
    private int currentIndex = -1;
    private GridManager activeGridManager;

    public bool IsInEditMode { get; private set; }
    public int CurrentIndex => currentIndex;

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
                plot.HighlightPlot(normalTileColor);
            }
        }

        activeGridManager.HighlightPlot(selectedTileColor);
        activeGridManager.SetActive(true);

        if (plotLabel != null)
            plotLabel.text = $"Selected Plot: {gm.gameObject.name}";

        if (currentIndex >= 0 && currentIndex < buildingButtons.Count)
            activeGridManager.SetSelectedCuboid(currentIndex);
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
}
