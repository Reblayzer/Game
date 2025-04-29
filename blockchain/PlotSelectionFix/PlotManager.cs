using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

public class PlotManager : MonoBehaviour
{
  public static PlotManager Instance { get; private set; }

  [Header("Prefabs & References")]
  public GameObject gridManagerPrefab;
  public BuildingButtonSelector buildingSelector;
  public Transform cameraPivot;
  public Transform cameraRig;

  [Header("Layout")]
  public int plotRows = 3, plotCols = 3;
  public float plotSpacing = 0.5f;

  [Header("Types")]
  public int abandonedPlotCount = 1, voidPlotCount = 1;

  [Header("Shared UI")]
  public GameObject selectedCuboidUIPanel;
  public TMP_Text selectedCuboidInfoText;
  public Button upgradeButton;

  private List<GridManager> plots = new List<GridManager>();

  void Awake() => Instance = this;

  void Start() => GeneratePlots();

  private void GeneratePlots()
  {
    float worldStep = 7f + plotSpacing;
    int total = plotRows * plotCols;
    var indices = new List<int>();
    for (int i = 0; i < total; i++) indices.Add(i);
    indices.Shuffle();
    var abandoned = new HashSet<int>(indices.GetRange(0, Mathf.Min(abandonedPlotCount, total)));
    var voids = new HashSet<int>(indices.GetRange(abandonedPlotCount,
                        Mathf.Min(voidPlotCount, total - abandonedPlotCount)));

    plots.Clear();
    for (int r = 0; r < plotRows; r++)
      for (int c = 0; c < plotCols; c++)
      {
        int flat = r * plotCols + c;
        Vector3 pos = new Vector3(
            (c - (plotCols - 1) / 2f) * worldStep,
            0,
            (r - (plotRows - 1) / 2f) * worldStep
        );

        var go = Instantiate(gridManagerPrefab, pos, Quaternion.identity, transform);
        var gm = go.GetComponent<GridManager>();
        gm.plotRow = r;
        gm.plotCol = c;

        go.name = $"GridManager({r},{c})";
        gm.SetUIReferences(selectedCuboidUIPanel, selectedCuboidInfoText, upgradeButton);
        gm.SetButtonSelector(buildingSelector);

        if (voids.Contains(flat)) gm.plotType = PlotType.Void;
        else if (abandoned.Contains(flat)) gm.plotType = PlotType.Abandoned;
        else gm.plotType = PlotType.Normal;

        plots.Add(gm);
      }

    // Ownership: one Yours, three Opponent
    var normalPlots = plots.FindAll(p => p.plotType == PlotType.Normal);
    if (normalPlots.Count > 0)
    {
      int mine = Random.Range(0, normalPlots.Count);
      normalPlots[mine].ownership = Ownership.Yours;
      normalPlots.RemoveAt(mine);
      int oppC = Mathf.Min(3, normalPlots.Count);
      for (int i = 0; i < oppC; i++)
      {
        int idx = Random.Range(0, normalPlots.Count);
        normalPlots[idx].ownership = Ownership.Opponent;
        normalPlots.RemoveAt(idx);
      }
    }

    // Highlight & init camera on your plot
    foreach (var g in plots)
      StartCoroutine(HighlightWhenReady(g));

    var yours = plots.FindAll(p => p.ownership == Ownership.Yours);
    if (yours.Count > 0)
      StartCoroutine(InitializeAfterTilesReady(yours[0]));
    else
      Debug.LogWarning("No plot assigned to YOU!");
  }

  private IEnumerator HighlightWhenReady(GridManager gm)
  {
    yield return new WaitUntil(() => gm.IsInitialized);
    Color col = buildingSelector.normalTileColor;
    if (gm.plotType == PlotType.Void) col = buildingSelector.voidPlotColor;
    else if (gm.plotType == PlotType.Abandoned) col = buildingSelector.abandonedPlotColor;
    gm.HighlightPlot(col);
  }

  private IEnumerator InitializeAfterTilesReady(GridManager gm)
  {
    yield return new WaitUntil(() => gm.IsInitialized);
    cameraPivot.position = gm.transform.position;
    cameraRig.position = new Vector3(0, cameraRig.position.y, -30f);
    buildingSelector.SetActiveGridManager(gm);
    Debug.Log($"🎯 Initialised on plot {gm.plotRow},{gm.plotCol}");
  }
}
