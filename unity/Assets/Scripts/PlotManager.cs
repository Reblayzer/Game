using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlotManager : MonoBehaviour
{
    public static PlotManager Instance { get; private set; }

    [Header("Prefabs & References")]
    public GameObject gridManagerPrefab;
    public BuildingButtonSelector buildingSelector;
    public Transform cameraPivot;
    public Transform cameraRig;

    [Header("Grid Layout Settings")]
    public int plotRows = 3;
    public int plotCols = 3;
    public float plotSpacing = 0.5f;

    [Header("Plot Type Distribution")]
    public int abandonedPlotCount = 1;
    public int voidPlotCount = 1;


    [Header("Shared UI References")]
    public GameObject selectedCuboidUIPanel;
    public TMP_Text selectedCuboidInfoText;
    public Button upgradeButton;

    private List<GridManager> plots = new List<GridManager>();

    void Start()
    {
        Instance = this;
        GeneratePlots();
    }

    private IEnumerator InitializeAfterTilesReady(GridManager selectedPlot)
    {
        yield return new WaitUntil(() => selectedPlot != null && selectedPlot.IsInitialized);

        cameraPivot.position = selectedPlot.transform.position;
        cameraRig.position = new Vector3(0, cameraRig.position.y, -30f);

        buildingSelector.SetActiveGridManager(selectedPlot);

        Debug.Log($"🎯 Initialized with random normal plot: {selectedPlot.name}");
    }

    private void GeneratePlots()
    {
        float gridSize = 7f;
        float spacing = gridSize + plotSpacing;
        int totalPlots = plotRows * plotCols;

        // Step 1: Generate list of all indices and shuffle
        List<int> allIndices = new List<int>();
        for (int i = 0; i < totalPlots; i++) allIndices.Add(i);
        allIndices.Shuffle();

        // Step 2: Choose indices for Abandoned and Void
        HashSet<int> abandonedIndices = new HashSet<int>(allIndices.GetRange(0, Mathf.Min(abandonedPlotCount, totalPlots)));
        HashSet<int> voidIndices = new HashSet<int>(allIndices.GetRange(abandonedPlotCount, Mathf.Min(voidPlotCount, totalPlots - abandonedPlotCount)));

        // Step 3: Create plots and assign types
        plots.Clear();
        for (int row = 0; row < plotRows; row++)
        {
            for (int col = 0; col < plotCols; col++)
            {
                int flatIndex = row * plotCols + col;
                Vector3 position = new Vector3(
                    (row - plotRows / 2) * spacing,
                    0,
                    (col - plotCols / 2) * spacing
                );

                GameObject gridGO = Instantiate(gridManagerPrefab, position, Quaternion.identity, transform);
                GridManager grid = gridGO.GetComponent<GridManager>();

                gridGO.name = $"GridManager({row},{col})";
                grid.SetPlotID($"{row},{col}");
                grid.SetUIReferences(selectedCuboidUIPanel, selectedCuboidInfoText, upgradeButton);
                grid.SetButtonSelector(buildingSelector);

                // Assign plot type and highlight accordingly
                if (voidIndices.Contains(flatIndex))
                {
                    grid.plotType = PlotType.Void;
                    StartCoroutine(HighlightWhenReady(grid, buildingSelector.voidPlotColor));
                }
                else if (abandonedIndices.Contains(flatIndex))
                {
                    grid.plotType = PlotType.Abandoned;
                    StartCoroutine(HighlightWhenReady(grid, buildingSelector.abandonedPlotColor));
                }
                else
                {
                    grid.plotType = PlotType.Normal;
                    StartCoroutine(HighlightWhenReady(grid, buildingSelector.normalTileColor));
                }

                plots.Add(grid);
            }
        }

        // Step 4: Pick a random normal plot and set it as the active one
        List<GridManager> normalPlots = plots.FindAll(p => p.plotType == PlotType.Normal);
        if (normalPlots.Count > 0)
        {
            GridManager randomNormal = normalPlots[Random.Range(0, normalPlots.Count)];
            StartCoroutine(InitializeAfterTilesReady(randomNormal));
        }
        else
        {
            Debug.LogWarning("⚠️ No Normal plots available to select!");
        }
    }

    private IEnumerator HighlightWhenReady(GridManager grid, Color fallbackColor)
    {
        yield return new WaitUntil(() => grid.IsInitialized);

        // 🧠 Ensure we're using plotType-specific color AFTER initialization
        Color highlightColor = fallbackColor;

        if (grid.plotType == PlotType.Abandoned)
            highlightColor = buildingSelector.abandonedPlotColor;
        else if (grid.plotType == PlotType.Void)
            highlightColor = buildingSelector.voidPlotColor;
        else if (grid == buildingSelector.GetActiveGridManager())
            highlightColor = buildingSelector.selectedTileColor;
        else
            highlightColor = buildingSelector.normalTileColor;

        grid.HighlightPlot(highlightColor);
    }
}

public static class ListExtensions
{
    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
}
