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
    public Color selectedColor = Color.yellow;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.blue; // This is the new selected plot color

    public TMP_Text plotLabel;

    private int currentIndex = -1;
    private GridManager activeGridManager;

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

        // Set all OTHER plots to green
        foreach (GridManager plot in FindObjectsByType<GridManager>(FindObjectsSortMode.None))
        {
            if (plot != activeGridManager)
            {
                plot.SetActive(false);
                plot.HighlightPlot(Color.green);
            }
        }

        // Now handle the selected one
        activeGridManager.HighlightPlot(highlightColor);
        activeGridManager.SetActive(true);

        // Update UI label
        if (plotLabel != null)
        {
            plotLabel.text = $"Selected Plot: {gm.gameObject.name}";
        }

        // Apply cuboid selection
        if (currentIndex >= 0 && currentIndex < buildingButtons.Count)
        {
            activeGridManager.SetSelectedCuboid(currentIndex);
        }
    }

    public void SelectByIndex(int index)
    {
        for (int i = 0; i < buildingButtons.Count; i++)
        {
            if (buildingButtons[i]?.backgroundImage != null)
                buildingButtons[i].backgroundImage.color = (i == index) ? selectedColor : normalColor;
        }

        currentIndex = index;

        if (activeGridManager != null)
        {
            activeGridManager.SetSelectedCuboid(index);
        }
        else
        {
            Debug.LogWarning("No active GridManager assigned.");
        }
    }

    public void ClearSelection()
    {
        foreach (var button in buildingButtons)
        {
            if (button?.backgroundImage != null)
                button.backgroundImage.color = normalColor;
        }

        currentIndex = -1;

        if (activeGridManager != null)
        {
            activeGridManager.ClearCuboidSelection();
        }
    }
}

