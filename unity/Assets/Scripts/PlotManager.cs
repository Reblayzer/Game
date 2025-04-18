using UnityEngine;

public class PlotManager : MonoBehaviour
{
    public GameObject gridManagerPrefab;
    public BuildingButtonSelector buildingSelector;
    public int plotRows = 3;
    public int plotCols = 3;
    public float plotSpacing = 0.5f; // adjustable space between plots
    public Vector2Int tilesPerPlot = new Vector2Int(7, 7); // each plot is 7x7 tiles

    private void Start()
    {
        GeneratePlots();
    }

    void GeneratePlots()
    {
        Vector2 centerOffset = new Vector2(plotCols / 2f, plotRows / 2f);

        for (int x = 0; x < plotCols; x++)
        {
            for (int y = 0; y < plotRows; y++)
            {
                Vector3 worldPosition = new Vector3(
                    (x - centerOffset.x) * (tilesPerPlot.x + plotSpacing),
                    0,
                    (y - centerOffset.y) * (tilesPerPlot.y + plotSpacing)
                );

                GameObject plot = Instantiate(gridManagerPrefab, worldPosition, Quaternion.identity, transform);

                GridManager gm = plot.GetComponent<GridManager>();
                gm.SetPlotID($"{x},{y}");

                // Set the center plot as the one the UI controls
                if (x == 1 && y == 1)
                {
                    buildingSelector.SetActiveGridManager(gm);
                }
            }
        }
    }
}
