using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlotManager : MonoBehaviour
{
    [Header("Prefabs & References")]
    public GameObject gridManagerPrefab;
    public BuildingButtonSelector buildingSelector;
    public Transform cameraPivot;
    public Transform cameraRig;

    [Header("Grid Layout Settings")]
    public int plotRows = 3;
    public int plotCols = 3;
    public float plotSpacing = 0.5f;

    [Header("Shared UI References")]
    public GameObject selectedCuboidUIPanel;
    public TMP_Text selectedCuboidInfoText;
    public Button upgradeButton;

    private List<GridManager> plots = new List<GridManager>();

    void Start()
    {
        GeneratePlots();
        StartCoroutine(InitializeAfterTilesReady());
    }

    private IEnumerator InitializeAfterTilesReady()
    {
        yield return null; // Wait one frame to ensure all GridManagers have run Start()

        Vector2Int centerPlot = new Vector2Int(plotRows / 2, plotCols / 2);
        GridManager centerManager = plots[centerPlot.x * plotCols + centerPlot.y];

        cameraPivot.position = centerManager.transform.position;
        cameraRig.position = new Vector3(0, cameraRig.position.y, -30f);

        buildingSelector.SetActiveGridManager(centerManager);

        Debug.Log($"🎯 Initialized with center plot: {centerManager.name}");
    }

    private void GeneratePlots()
    {
        float gridSize = 7f;
        float spacing = gridSize + plotSpacing;
        Vector2Int center = new Vector2Int(plotRows / 2, plotCols / 2);

        for (int row = 0; row < plotRows; row++)
        {
            for (int col = 0; col < plotCols; col++)
            {
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

                // 🔷 NEW: Set BuildingButtonSelector reference
                grid.SetButtonSelector(buildingSelector);

                plots.Add(grid);

                // Only highlight green if it's not the center plot
                if (!(row == center.x && col == center.y))
                {
                    StartCoroutine(HighlightWhenReady(grid, Color.green));
                }
            }
        }
    }

    private IEnumerator HighlightWhenReady(GridManager grid, Color color)
    {
        yield return new WaitUntil(() => grid.IsInitialized);
        grid.HighlightPlot(color);
    }
}
