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

        // Center camera pivot on the middle plot
        Vector2Int centerPlot = new Vector2Int(plotRows / 2, plotCols / 2);
        GridManager centerManager = plots[centerPlot.x * plotCols + centerPlot.y];

        cameraPivot.position = centerManager.transform.position;
        cameraRig.position = new Vector3(0, cameraRig.position.y, -30f);

        buildingSelector.SetActiveGridManager(centerManager);

        Debug.Log($"🎯 Initialized with center plot: {centerManager.name}");
    }

    private void GeneratePlots()
    {
        float gridSize = 7f; // Adjust if your GridManager prefab size is different
        float spacing = gridSize + plotSpacing;

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

                plots.Add(grid);
            }
        }
    }
}
