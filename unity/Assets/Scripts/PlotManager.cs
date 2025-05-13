using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
    public int mountainPlotCount = 1;

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

    private IEnumerator InitializeAfterTilesReady(GridManager gm)
    {
        yield return new WaitUntil(() => gm != null && gm.IsInitialized);
        cameraPivot.position = gm.transform.position;
        cameraRig.position = new Vector3(0, cameraRig.position.y, -30f);
        buildingSelector.SetActiveGridManager(gm);
        Debug.Log($"🎯 Initialized with plot: {gm.name}");
    }

    private void GeneratePlots()
    {
        float gridSize = 7f;
        float spacing = gridSize + plotSpacing;
        int total = plotRows * plotCols;

        // 1) Shuffle indices
        var allIndices = new List<int>();
        for (int i = 0; i < total; i++) allIndices.Add(i);
        allIndices.Shuffle();

        // 2) Pick Abandoned & Void
        int start = 0;
        var abandonedIdx = new HashSet<int>(
          allIndices.GetRange(start, Mathf.Min(abandonedPlotCount, total - start))
        );
        start += abandonedPlotCount;
        var voidIdx = new HashSet<int>(
          allIndices.GetRange(start, Mathf.Min(voidPlotCount, total - start))
        );
        start += voidPlotCount;
        var mountainIdx = new HashSet<int>(
          allIndices.GetRange(start, Mathf.Min(mountainPlotCount, total - start))
        );

        // 3) Spawn every plot
        plots.Clear();
        for (int r = 0; r < plotRows; r++)
            for (int c = 0; c < plotCols; c++)
            {
                int flat = r * plotCols + c;
                Vector3 pos = new Vector3((r - plotRows / 2f) * spacing, 0, (c - plotCols / 2f) * spacing);

                var go = Instantiate(gridManagerPrefab, pos, Quaternion.identity, transform);
                var grid = go.GetComponent<GridManager>();

                grid.plotRow = r;
                grid.plotCol = c;

                go.name = $"GridManager({r},{c})";
                grid.SetPlotID($"{r},{c}");
                grid.SetUIReferences(selectedCuboidUIPanel, selectedCuboidInfoText, upgradeButton);
                grid.SetButtonSelector(buildingSelector);

                if (mountainIdx.Contains(flat)) grid.plotType = PlotType.Mountain;
                else if (voidIdx.Contains(flat)) grid.plotType = PlotType.Void;
                else if (abandonedIdx.Contains(flat)) grid.plotType = PlotType.Abandoned;
                else grid.plotType = PlotType.Normal;

                plots.Add(grid);
            }

        // 4) From the Normal plots, pick one “Yours” and three “Opponent”
        var normals = plots.FindAll(p => p.plotType == PlotType.Normal);
        if (normals.Count > 0)
        {
            int mine = Random.Range(0, normals.Count);
            normals[mine].ownership = Ownership.Yours;
            normals.RemoveAt(mine);

            int oppC = Mathf.Min(3, normals.Count);
            for (int i = 0; i < oppC; i++)
            {
                int idx = Random.Range(0, normals.Count);
                normals[idx].ownership = Ownership.Opponent;
                normals.RemoveAt(idx);
            }
        }

        // 5) Highlight each plot once it’s ready
        foreach (var g in plots)
            StartCoroutine(HighlightWhenReady(g));

        // 6) Initialize camera & UI on “Yours”
        var yours = plots.FindAll(p => p.ownership == Ownership.Yours);
        if (yours.Count > 0)
            StartCoroutine(InitializeAfterTilesReady(yours[0]));
        else
            Debug.LogWarning("⚠️ No plot was assigned to You!");
    }

    private IEnumerator HighlightWhenReady(GridManager g)
    {
        yield return new WaitUntil(() => g.IsInitialized);

        Color col = buildingSelector.normalTileColor;
        if (g.plotType == PlotType.Void) col = buildingSelector.voidPlotColor;
        else if (g.plotType == PlotType.Abandoned) col = buildingSelector.abandonedPlotColor;

        g.HighlightPlot(col);
    }
}

// Fisher–Yates shuffle extension
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
