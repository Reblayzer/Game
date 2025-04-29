using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public enum PlotType { Normal, Abandoned, Void }
public enum Ownership { Unclaimed, Yours, Opponent }

[System.Serializable]
public class CuboidType
{
  public string name;
  public GameObject prefab;
  public GameObject shockwavePrefab;
  public int length = 1, width = 1, height = 1;
}

public class GridManager : MonoBehaviour
{
  [HideInInspector] public int plotRow;
  [HideInInspector] public int plotCol;
  [HideInInspector] public Ownership ownership = Ownership.Unclaimed;

  [Header("Grid Settings")]
  public int gridSize = 7;
  public GameObject tilePrefab;
  public CuboidType[] cuboidTypes;

  [Header("UI / Audio")]
  public GameObject buildingButtonsPanel;
  [SerializeField] private GameObject selectedCuboidUIPanel;
  [SerializeField] private TMP_Text selectedCuboidInfoText;
  [SerializeField] private Button upgradeButton;
  public AudioClip placementSound;

  [Header("Ghost Materials")]
  public Material ghostMaterialValid;
  public Material ghostMaterialInvalid;

  [Header("Marker Canvases")]
  public GameObject claimedMarkerCanvasPrefab;
  public GameObject unclaimedMarkerCanvasPrefab;
  public GameObject abandonedMarkerCanvasPrefab;
  public GameObject opponentMarkerCanvasPrefab;

  [HideInInspector] public PlotType plotType = PlotType.Normal;

  // internals
  private AudioSource audioSource;
  private bool[,] occupiedTiles;
  private Tile[,] tileGrid;
  private bool initialized;
  public bool IsInitialized => initialized;

  private bool isRotated, hasSelectedCuboid;
  private GameObject ghostObject;
  private string lastGhostName = "";
  private BuildingButtonSelector buttonSelector;
  private bool isEditMode;
  public bool IsActive { get; private set; }
  public int selectedIndex { get; private set; }

  public bool CanPlace =>
      buildingButtonsPanel != null &&
      buildingButtonsPanel.activeSelf &&
      hasSelectedCuboid &&
      plotType == PlotType.Normal;

  void Start()
  {
    // 1) Build trigger zone
    var triggerZone = new GameObject("PlotTrigger");
    triggerZone.transform.SetParent(transform);
    triggerZone.transform.localPosition = Vector3.zero;
    triggerZone.transform.localRotation = Quaternion.identity;
    triggerZone.transform.localScale = Vector3.one;
    var box = triggerZone.AddComponent<BoxCollider>();
    box.size = new Vector3(gridSize, 0.1f, gridSize);
    box.center = new Vector3(0, 0.01f, 0);
    box.isTrigger = true;
    triggerZone.layer = LayerMask.NameToLayer("Plot");

    // 2) Hook up ownership & marker
    var ptc = triggerZone.AddComponent<PlotTriggerController>();
    ptc.ownership = ownership;

    GameObject canvasPrefab = null;
    if (plotType == PlotType.Abandoned) canvasPrefab = abandonedMarkerCanvasPrefab;
    else if (plotType == PlotType.Normal)
      canvasPrefab = ownership == Ownership.Yours
          ? claimedMarkerCanvasPrefab
          : (ownership == Ownership.Opponent
              ? opponentMarkerCanvasPrefab
              : unclaimedMarkerCanvasPrefab);

    if (canvasPrefab != null)
    {
      var canv = Instantiate(canvasPrefab, triggerZone.transform);
      canv.SetActive(false);
      canv.transform.localPosition = Vector3.up * 2f;
      canv.transform.localRotation = Quaternion.identity;
      canv.AddComponent<BillboardCanvas>();
      ptc.markerCanvas = canv;
    }

    // 3) Generate tile grid
    float offset = gridSize / 2f - .5f;
    occupiedTiles = new bool[gridSize, gridSize];
    tileGrid = new Tile[gridSize, gridSize];

    for (int x = 0; x < gridSize; x++)
      for (int z = 0; z < gridSize; z++)
      {
        Vector3 pos = transform.position
                    + new Vector3(x - offset, 0, z - offset);
        var tileObj = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
        tileObj.layer = LayerMask.NameToLayer("Tile");
        var tile = tileObj.AddComponent<Tile>();
        tile.Init(new Vector2Int(x, z), this);
        tileGrid[x, z] = tile;
      }

    audioSource = GetComponent<AudioSource>();
    selectedCuboidUIPanel?.SetActive(false);
    buildingButtonsPanel?.SetActive(true);
    initialized = true;
  }

  public void SetActive(bool state)
  {
    IsActive = state;
    if (!state)
    {
      ClearHighlights();
      hasSelectedCuboid = false;
      selectedCuboidUIPanel?.SetActive(false);
      upgradeButton?.gameObject.SetActive(false);
    }
    else
    {
      buildingButtonsPanel?.SetActive(true);
      if (buttonSelector != null && selectedIndex >= 0)
        SetSelectedCuboid(selectedIndex);
    }
  }

  public void SetEditMode(bool state)
  {
    isEditMode = state;
    if (!state) ClearSelectionAndUI();
  }

  public void SetButtonSelector(BuildingButtonSelector sel)
      => buttonSelector = sel;

  public void SetSelectedCuboid(int index)
  {
    if (index < 0 || index >= cuboidTypes.Length) return;
    selectedIndex = index;
    hasSelectedCuboid = true;
    ClearHighlights();
    // (…ghost preview, existing logic…)
  }

  public void ClearCuboidSelection()
  {
    hasSelectedCuboid = false;
    ClearHighlights();
  }

  void Update()
  {
    if (!IsActive) return;
    if (EventSystem.current.IsPointerOverGameObject())
    {
      ClearHighlights();
      return;
    }

    if (!isEditMode) { /* …info‐click logic… */ ClearHighlights(); return; }
    if (!CanPlace) { ClearHighlights(); return; }

    HandleRotation();

    // (…raycast, ShowGhost, TryPlaceCuboidAt…)
  }

  private void HandleRotation()
  {
    if (Input.GetKeyDown(KeyCode.R) && CanPlace)
      isRotated = !isRotated;
  }

  public void TryPlaceCuboidAt(int sx, int sz)
  {
    if (!CanPlace) return;
    var cur = cuboidTypes[selectedIndex];
    int len = isRotated ? cur.width : cur.length;
    int wid = isRotated ? cur.length : cur.width;

    // bounds & occupancy check…
    float offset = gridSize / 2f - .5f;
    Vector3 spawn = transform.position + new Vector3(
        sx + len / 2f - .5f - offset,
        cur.height / 2f,
        sz + wid / 2f - .5f - offset
    );
    Quaternion rot = isRotated ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
    var placed = Instantiate(cur.prefab, spawn, rot);
    SetLayerRecursive(placed, LayerMask.NameToLayer("Placed"));

    var sel = placed.AddComponent<SelectableCuboid>();
    sel.cuboidName = cur.name;
    sel.infoPanel = selectedCuboidUIPanel;
    sel.infoDisplay = selectedCuboidInfoText;
    sel.upgradeButton = upgradeButton;
    sel.Init(this);

    // mark occupied…
    ClearHighlights();
    if (placementSound != null)
    {
      audioSource.pitch = Random.Range(.95f, 1.05f);
      audioSource.PlayOneShot(placementSound);
    }
    if (cur.shockwavePrefab != null)
    {
      var fx = Instantiate(cur.shockwavePrefab, spawn - Vector3.up * (cur.height / 2f - .2f), Quaternion.identity);
      Destroy(fx, 2f);
    }
  }

  private void SetLayerRecursive(GameObject go, int layer)
  {
    go.layer = layer;
    foreach (var c in go.GetComponentsInChildren<Transform>())
      c.gameObject.layer = layer;
  }

  public void HighlightPlot(Color col)
  {
    for (int x = 0; x < gridSize; x++)
      for (int z = 0; z < gridSize; z++)
        tileGrid[x, z]?.SetPersistentColor(col);
  }

  private void ClearSelectionAndUI()
  {
    hasSelectedCuboid = false;
    selectedCuboidUIPanel?.SetActive(false);
    upgradeButton?.gameObject.SetActive(false);
    ClearHighlights();
  }

  public void ResetTileColors() => ClearHighlights();
  public void ClearHighlights()
  {
    foreach (var t in tileGrid) t?.ClearHighlight();
    if (ghostObject != null) { Destroy(ghostObject); ghostObject = null; }
  }
}
