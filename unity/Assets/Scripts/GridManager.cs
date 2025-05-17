using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class CuboidType
{
    public string name;
    private string plotID;
    public GameObject prefab;
    public GameObject shockwavePrefab;
    public int length = 1;
    public int width = 1;
    public int height = 1;

    public void SetPlotID(string id)
    {
        plotID = id;
    }
}

public enum PlotType { Normal, Abandoned, Void, Mountain }
public enum Ownership { Unclaimed, Yours, Opponent }

public class GridManager : MonoBehaviour
{
    [HideInInspector] public Ownership ownership = Ownership.Unclaimed;
    [HideInInspector] public int plotRow;
    [HideInInspector] public int plotCol;

    [Header("Grid Settings")]
    public int gridSize = 7;
    public GameObject tilePrefab;
    public CuboidType[] cuboidTypes;

    [Header("Mountain")]
    [SerializeField] private GameObject mountainPrefab;

    [Header("Pit")]
    public GameObject pitPrefab;

    [Header("Abandoned")]
    public GameObject abandonedBuildingPrefab;

    [Header("Materials")]
    public Material ghostMaterialValid;
    public Material ghostMaterialInvalid;

    [Header("UI References")]
    public GameObject buildingButtonsPanel;
    [SerializeField] private GameObject selectedCuboidUIPanel;
    [SerializeField] private TMP_Text selectedCuboidInfoText;
    [SerializeField] private Button upgradeButton;

    public GameObject SelectedCuboidUIPanel => selectedCuboidUIPanel;
    public TMP_Text SelectedCuboidInfoText => selectedCuboidInfoText;
    public Button UpgradeButton => upgradeButton;

    [Header("Audio")]
    public AudioClip placementSound;

    [Header("Marker Canvas Prefabs")]
    public GameObject claimedMarkerCanvasPrefab;
    public GameObject unclaimedMarkerCanvasPrefab;
    public GameObject abandonedMarkerCanvasPrefab;
    public GameObject opponentMarkerCanvasPrefab;

    public event Action OnCuboidPlaced;

    private AudioSource audioSource;
    private bool[,] occupiedTiles;
    private Tile[,] tileGrid;
    private bool initialized = false;
    public bool IsInitialized => initialized;

    private bool isRotated = false;
    private GameObject ghostObject;
    private string lastGhostName = "";
    private bool hasSelectedCuboid = false;
    private BuildingButtonSelector buttonSelector;
    private bool isEditMode = false;
    public int selectedIndex = 0;
    private Color currentHighlightColor;
    private bool placementPhase = false;

    public bool InPlacementPhase => placementPhase;
    private int _placedMask;
    private int _tileMask;

    [Header("Ground Decoration")]
    [Tooltip("Pebbles go at local Y = 0.025")]
    public GameObject pebblesPrefab;
    [Tooltip("Small rock goes at local Y = 0")]
    public GameObject smallRockPrefab;
    [Tooltip("High rock goes at local Y = 0")]
    public GameObject highRockPrefab;

    // 0 = none, 1 = pebbles, 2 = small rock, 3 = high rock
    private int[,] decorationMap;

    public bool CanPlace =>
        buildingButtonsPanel != null &&
        buildingButtonsPanel.activeSelf &&
        hasSelectedCuboid &&
        plotType == PlotType.Normal &&
        placementPhase;

    public PlotType plotType = PlotType.Normal;
    public bool IsActive { get; private set; }

    void Start()
    {
        // 0) Cache your layer masks
        _placedMask = 1 << LayerMask.NameToLayer("Placed");
        _tileMask = 1 << LayerMask.NameToLayer("Tile");

        // 1) PlotTrigger collider
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
        var ptc = triggerZone.AddComponent<PlotTriggerController>();
        ptc.ownership = ownership;

        // 2) Special‐case: mountain in the center
        if (plotType == PlotType.Mountain && mountainPrefab != null)
        {
            var rock = Instantiate(
                mountainPrefab,
                transform.position,
                Quaternion.identity,
                transform
            );
            rock.name = "MountainTop";
        }

        // 3) Special‐case: pit for Void plots
        if (plotType == PlotType.Void && pitPrefab != null)
        {
            var pit = Instantiate(
                pitPrefab,
                transform.position,
                Quaternion.identity,
                transform
            );
            pit.name = "Pit";
        }

        // 3.5) Special‐case: abandoned‐plot building
        if (plotType == PlotType.Abandoned && abandonedBuildingPrefab != null)
        {
            var bld = Instantiate(
                abandonedBuildingPrefab,
                transform.position,
                Quaternion.identity,
                transform
            );
            bld.name = "AbandonedBuilding";
        }

        // 4) World‐marker canvas (Normal & Abandoned only)
        GameObject canvasPrefab = null;
        if (plotType == PlotType.Abandoned)
        {
            canvasPrefab = abandonedMarkerCanvasPrefab;
        }
        else if (plotType == PlotType.Normal)
        {
            switch (ownership)
            {
                case Ownership.Yours: canvasPrefab = claimedMarkerCanvasPrefab; break;
                case Ownership.Opponent: canvasPrefab = opponentMarkerCanvasPrefab; break;
                default: canvasPrefab = unclaimedMarkerCanvasPrefab; break;
            }
        }
        if (canvasPrefab != null)
        {
            var canv = Instantiate(canvasPrefab, triggerZone.transform);
            canv.SetActive(false);
            canv.transform.localPosition = new Vector3(0, 2f, 0);
            canv.AddComponent<BillboardCanvas>();
            ptc.markerCanvas = canv;
        }

        // === Precompute decorations ===
        ComputeDecorationMap();

        // 5) Build tile grid—skip the 5×5 interior on Void plots
        float offset = gridSize / 2f - 0.5f;
        occupiedTiles = new bool[gridSize, gridSize];
        tileGrid = new Tile[gridSize, gridSize];

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                if (plotType == PlotType.Void
                    && x > 0 && x < gridSize - 1
                    && z > 0 && z < gridSize - 1)
                    continue;

                Vector3 pos = transform.position
                              + new Vector3(x - offset, 0f, z - offset);

                GameObject tileObj = Instantiate(
                    tilePrefab,
                    pos,
                    Quaternion.identity,
                    transform
                );
                tileObj.layer = LayerMask.NameToLayer("Tile");

                var tileScript = tileObj.AddComponent<Tile>();
                tileScript.Init(new Vector2Int(x, z), this);
                tileGrid[x, z] = tileScript;

                int deco = decorationMap[x, z];
                switch (deco)
                {
                    case 1:
                        var pb = Instantiate(pebblesPrefab, tileObj.transform);
                        pb.transform.localPosition = new Vector3(0f, 0.025f, 0f);
                        pb.transform.localRotation = Quaternion.identity;
                        pb.transform.localScale = pebblesPrefab.transform.localScale;
                        break;
                    case 2:
                        var sr = Instantiate(smallRockPrefab, tileObj.transform);
                        sr.transform.localPosition = Vector3.zero;
                        sr.transform.localRotation = Quaternion.identity;
                        sr.transform.localScale = smallRockPrefab.transform.localScale;
                        break;
                    case 3:
                        var hr = Instantiate(highRockPrefab, tileObj.transform);
                        hr.transform.localPosition = Vector3.zero;
                        hr.transform.localRotation = Quaternion.identity;
                        hr.transform.localScale = highRockPrefab.transform.localScale;
                        break;
                }
            }
        }

        // 6) Grab your AudioSource and hide any UI overlays
        audioSource = GetComponent<AudioSource>();
        if (selectedCuboidUIPanel != null) selectedCuboidUIPanel.SetActive(false);
        if (buildingButtonsPanel != null) buildingButtonsPanel.SetActive(true);

        initialized = true;
    }

    // Fills decorationMap[x,z] with:
    //   0 = none
    //   1 = pebbles
    //   2 = small rock
    //   3 = high rock
    // Whenever we place a pebble (1) at [x,z], we force *one* of its valid tile-neighbors
    // (N/E/S/W) to also become a pebble—guaranteeing no lone pebbles.
    private void ComputeDecorationMap()
    {
        decorationMap = new int[gridSize, gridSize];
        var rng = new System.Random();

        // Helper: is (x,z) a real tile?
        bool IsValidTile(int x, int z)
        {
            if (x < 0 || z < 0 || x >= gridSize || z >= gridSize)
                return false;
            if (plotType == PlotType.Void
                && x > 0 && x < gridSize - 1
                && z > 0 && z < gridSize - 1)
                return false;
            return true;
        }

        // Four cardinal directions
        Vector2Int[] dirs = {
        new Vector2Int( 1,  0),
        new Vector2Int(-1,  0),
        new Vector2Int( 0,  1),
        new Vector2Int( 0, -1)
    };

        // Only place pebbles on Normal or Abandoned plots
        if (plotType == PlotType.Normal || plotType == PlotType.Abandoned)
        {
            // Decide how many chunks: exactly 2 if this is "your" plot,
            // otherwise random 1–4.
            int minChunks = 1, maxChunks = 4;
            int desiredChunks = (ownership == Ownership.Yours)
                                 ? 2
                                 : rng.Next(minChunks, maxChunks + 1);

            int placedChunks = 0, tries = 0;
            while (placedChunks < desiredChunks && tries < desiredChunks * 10)
            {
                tries++;
                // pick a random valid, empty cell
                int x = rng.Next(gridSize), z = rng.Next(gridSize);
                if (!IsValidTile(x, z) || decorationMap[x, z] != 0)
                    continue;

                // find its empty, valid neighbors
                var candidates = new List<Vector2Int>();
                foreach (var d in dirs)
                {
                    int nx = x + d.x, nz = z + d.y;
                    if (IsValidTile(nx, nz) && decorationMap[nx, nz] == 0)
                        candidates.Add(new Vector2Int(nx, nz));
                }
                if (candidates.Count == 0)
                    continue;

                // place the chunk of two pebbles
                var pick = candidates[rng.Next(candidates.Count)];
                decorationMap[x, z] = 1;
                decorationMap[pick.x, pick.y] = 1;
                placedChunks++;
            }

            // Guarantee at least one chunk if somehow none got placed
            if (placedChunks == 0)
            {
                for (int x = 0; x < gridSize && placedChunks == 0; x++)
                {
                    for (int z = 0; z < gridSize; z++)
                    {
                        if (!IsValidTile(x, z) || decorationMap[x, z] != 0)
                            continue;

                        var candidates = new List<Vector2Int>();
                        foreach (var d in dirs)
                        {
                            int nx = x + d.x, nz = z + d.y;
                            if (IsValidTile(nx, nz) && decorationMap[nx, nz] == 0)
                                candidates.Add(new Vector2Int(nx, nz));
                        }
                        if (candidates.Count == 0)
                            continue;

                        var pick = candidates[rng.Next(candidates.Count)];
                        decorationMap[x, z] = 1;
                        decorationMap[pick.x, pick.y] = 1;
                        placedChunks = 1;
                        break;
                    }
                }
            }
        }

        // Now fill leftover tiles with rocks at 10% chance
        const double rockSpawnChance = 0.1;
        double smallRockRatio = 0.35 / (0.35 + 0.3); // ~0.538
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                if (!IsValidTile(x, z) || decorationMap[x, z] != 0)
                    continue;
                if (rng.NextDouble() > rockSpawnChance)
                    continue;

                if (rng.NextDouble() < smallRockRatio)
                    decorationMap[x, z] = 2;
                else
                    decorationMap[x, z] = 3;
            }
        }
    }

    public void SetActive(bool state)
    {
        IsActive = state;

        if (!state)
        {
            ClearHighlights();
            hasSelectedCuboid = false;
            if (selectedCuboidUIPanel != null) selectedCuboidUIPanel.SetActive(false);
            if (upgradeButton != null) upgradeButton.gameObject.SetActive(false);
        }
        else
        {
            if (buildingButtonsPanel != null) buildingButtonsPanel.SetActive(true);
            if (buttonSelector != null && buttonSelector.CurrentIndex >= 0)
                SetSelectedCuboid(buttonSelector.CurrentIndex);
        }
    }

    private void HandleRotation()
    {
        if (Input.GetKeyDown(KeyCode.R) && CanPlace)
        {
            isRotated = !isRotated;
        }
    }

    public void TryPlaceCuboidAt(int startX, int startZ)
    {
        // 0) safety checks
        if (cuboidTypes == null || cuboidTypes.Length == 0) return;
        if (selectedIndex < 0 || selectedIndex >= cuboidTypes.Length) return;
        if (!CanPlace) return;

        // 1) dimensions
        CuboidType current = cuboidTypes[selectedIndex];
        int length = isRotated ? current.width : current.length;
        int width = isRotated ? current.length : current.width;

        // 2) grid occupancy
        for (int x = startX; x < startX + length; x++)
            for (int z = startZ; z < startZ + width; z++)
                if (x < 0 || z < 0 || x >= gridSize || z >= gridSize || occupiedTiles[x, z])
                    return;

        // 3) world‐space box for OverlapBox
        float offset = gridSize / 2f - 0.5f;
        Vector3 center = transform.position + new Vector3(
            startX + length * 0.5f - 0.5f - offset,
            current.height * 0.5f,
            startZ + width * 0.5f - 0.5f - offset
        );
        Vector3 halfExtents = new Vector3(length * 0.5f, current.height * 0.5f, width * 0.5f);
        Quaternion rot = isRotated ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;

        // 4) overlap test against already‐placed buildings
        if (Physics.OverlapBox(center, halfExtents, rot, _placedMask).Length > 0)
            return;

        // 5) instantiate exactly once
        GameObject placed = Instantiate(current.prefab, center, rot);
        SetLayerRecursive(placed, LayerMask.NameToLayer("Placed"));
        SetLayerRecursive(placed, LayerMask.NameToLayer("MiningDrill"));

        // 6) hook up selectable
        var sel = placed.AddComponent<SelectableCuboid>();
        sel.cuboidName = current.name;
        sel.infoPanel = SelectedCuboidUIPanel;
        sel.infoDisplay = SelectedCuboidInfoText;
        sel.upgradeButton = UpgradeButton;
        sel.Init(this);

        // 7) mark grid cells
        for (int x = startX; x < startX + length; x++)
            for (int z = startZ; z < startZ + width; z++)
                occupiedTiles[x, z] = true;

        // 8) cleanup
        ClearHighlights();
        if (placementSound != null && audioSource != null)
        {
            audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(placementSound);
        }
        if (current.shockwavePrefab != null)
        {
            Vector3 vfxPos = center - new Vector3(0, current.height * 0.5f - 0.2f, 0);
            var vfx = Instantiate(current.shockwavePrefab, vfxPos, Quaternion.identity);
            Destroy(vfx, 2f);
        }
        OnCuboidPlaced?.Invoke();
    }

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }

    public void SetSelectedCuboid(int index)
    {
        if (placementPhase)
        {
            EndPlacementPhase();
            ClearHighlights();
        }

        if (index >= 0 && index < cuboidTypes.Length)
        {
            selectedIndex = index;
            hasSelectedCuboid = true;
            ClearHighlights();

            if (IsActive && isEditMode && placementPhase)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                int tileMask = LayerMask.GetMask("Tile");

                if (Physics.Raycast(ray, out hit, 100f, tileMask))
                {
                    var tile = hit.collider.GetComponent<Tile>();
                    if (tile != null && tile.GridManager == this)
                    {
                        bool isValid = HighlightTiles(tile.gridPosition.x, tile.gridPosition.y);
                        ShowGhost(tile.gridPosition.x, tile.gridPosition.y,
                        cuboidTypes[selectedIndex], isRotated, isValid);
                    }
                }
            }
        }
    }

    public void ClearCuboidSelection()
    {
        hasSelectedCuboid = false;
        ClearHighlights();
    }

    public bool HighlightTiles(int startX, int startZ)
    {
        ClearHighlights();

        CuboidType current = cuboidTypes[selectedIndex];
        int length = isRotated ? current.width : current.length;
        int width = isRotated ? current.length : current.width;

        bool validPlacement = true;
        for (int x = startX; x < startX + length; x++)
        {
            for (int z = startZ; z < startZ + width; z++)
            {
                if (x < 0 || z < 0 || x >= gridSize || z >= gridSize || occupiedTiles[x, z])
                {
                    validPlacement = false;
                    break;
                }
            }
            if (!validPlacement) break;
        }

        if (validPlacement)
        {
            float offset = gridSize / 2f - 0.5f;
            Vector3 center = transform.position + new Vector3(
                startX + length * 0.5f - 0.5f - offset,
                current.height * 0.5f,
                startZ + width * 0.5f - 0.5f - offset
            );
            Vector3 halfExtents = new Vector3(length * 0.5f, current.height * 0.5f, width * 0.5f);
            Quaternion rot = isRotated ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;

            if (Physics.OverlapBox(center, halfExtents, rot, _placedMask).Length > 0)
                validPlacement = false;
        }

        Color ghostColor = validPlacement
            ? buttonSelector.ghostCanPlaceColor
            : buttonSelector.ghostCanNotPlaceColor;

        for (int x = startX; x < startX + length; x++)
            for (int z = startZ; z < startZ + width; z++)
                if (x >= 0 && z >= 0 && x < gridSize && z < gridSize)
                    tileGrid[x, z]?.SetTemporaryHighlight(ghostColor);

        return validPlacement;
    }

    public void ClearHighlights()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                tileGrid[x, z]?.ClearHighlight();
            }
        }

        if (ghostObject != null)
        {
            Destroy(ghostObject);
            ghostObject = null;
            lastGhostName = "";
        }
    }

    private void ShowGhost(int startX, int startZ, CuboidType current, bool rotated, bool isValid)
    {
        int length = rotated ? current.width : current.length;
        int width = rotated ? current.length : current.width;
        float offset = gridSize / 2f - 0.5f;

        Vector3 ghostPos = transform.position + new Vector3(
            startX + length / 2f - 0.5f - offset,
            current.height / 2f,
            startZ + width / 2f - 0.5f - offset
        );

        Quaternion rotation = rotated ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
        string ghostName = "Ghost_" + current.name;

        if (ghostObject == null || lastGhostName != ghostName)
        {
            if (ghostObject != null)
                Destroy(ghostObject);

            ghostObject = Instantiate(current.prefab, ghostPos, rotation);
            ghostObject.name = ghostName;
            lastGhostName = ghostName;

            SetLayerRecursive(ghostObject, LayerMask.NameToLayer("Ghost"));
            ApplyGhostMaterial(ghostObject, isValid);
        }
        else
        {
            ghostObject.transform.position = ghostPos;
            ghostObject.transform.rotation = rotation;
            ApplyGhostMaterial(ghostObject, isValid);
        }
    }

    private void ApplyGhostMaterial(GameObject obj, bool isValid)
    {
        Material matToUse = isValid ? ghostMaterialValid : ghostMaterialInvalid;

        foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
        {
            renderer.material = matToUse;
        }

        foreach (var collider in obj.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }
    }

    public void SetPlotID(string id)
    {
        Debug.Log($"Assigned Plot ID: {id}");
    }

    public void SetUIReferences(GameObject panel, TMP_Text infoText, Button upgradeBtn)
    {
        selectedCuboidUIPanel = panel;
        selectedCuboidInfoText = infoText;
        upgradeButton = upgradeBtn;
    }

    public void HighlightPlot(Color color)
    {
        currentHighlightColor = color;

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                tileGrid[x, z]?.SetPersistentColor(color);
            }
        }
    }

    public void ResetTileColors()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                tileGrid[x, z]?.ClearHighlight();
            }
        }
    }

    public void SetButtonSelector(BuildingButtonSelector selector)
    {
        buttonSelector = selector;
    }

    public void SetEditMode(bool state)
    {
        isEditMode = state;

        if (!state)
        {
            ClearSelectionAndUI();
        }
    }

    private void ClearSelectionAndUI()
    {
        hasSelectedCuboid = false;

        if (selectedCuboidUIPanel != null)
            selectedCuboidUIPanel.SetActive(false);

        if (upgradeButton != null)
            upgradeButton.gameObject.SetActive(false);

        ClearHighlights();
    }

    public bool IsInEditMode()
    {
        return isEditMode;
    }

    public void HideCuboidInfo()
    {
        selectedCuboidUIPanel?.SetActive(false);
        upgradeButton?.gameObject.SetActive(false);
        SelectableCuboid.currentlySelectedCuboid = null;
    }

    void Update()
    {
        // Only run ghost‐preview if we’re initialized, active, in edit mode, and have picked a cuboid
        if (!initialized || !IsActive || !isEditMode || !hasSelectedCuboid || !placementPhase)
            return;

        // Raycast against your tile layer
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int tileMask = LayerMask.GetMask("Tile");
        if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, tileMask))
            return;

        // If we hit one of our Tile objects, show preview there
        var tile = hit.collider.GetComponent<Tile>();
        if (tile != null && tile.GridManager == this)
        {
            // highlight + ghost
            bool valid = HighlightTiles(tile.gridPosition.x, tile.gridPosition.y);
            ShowGhost(
                tile.gridPosition.x,
                tile.gridPosition.y,
                cuboidTypes[selectedIndex],
                isRotated,
                valid
            );
        }
    }

    public void StartPlacementPhase()
    {
        placementPhase = true;
    }

    public void EndPlacementPhase()
    {
        placementPhase = false;
    }
}