using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class CuboidType
{
    public string name;
    private string plotID;
    public GameObject prefab;
    public int length = 1;
    public int width = 1;
    public int height = 1;

    public void SetPlotID(string id)
    {
        plotID = id;
    }
}

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridSize = 7;
    public GameObject tilePrefab;
    public CuboidType[] cuboidTypes;

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

    private AudioSource audioSource;
    private bool[,] occupiedTiles;
    private Tile[,] tileGrid;
    private bool isRotated = false;
    private GameObject ghostObject;
    private string lastGhostName = "";
    private bool hasSelectedCuboid = false;
    public int selectedIndex = 0;
    public bool IsActive { get; private set; }

    public bool CanPlace => buildingButtonsPanel != null
                             && buildingButtonsPanel.activeSelf
                             && hasSelectedCuboid;

    void Start()
    {
        // 🔶 Add a non-blocking trigger for plot selection
        GameObject triggerZone = new GameObject("PlotTrigger");
        triggerZone.transform.SetParent(transform);
        triggerZone.transform.localPosition = Vector3.zero;
        triggerZone.transform.localRotation = Quaternion.identity;
        triggerZone.transform.localScale = Vector3.one;

        BoxCollider trigger = triggerZone.AddComponent<BoxCollider>();
        trigger.size = new Vector3(gridSize, 0.1f, gridSize);
        trigger.center = new Vector3(0, 0.01f, 0); // Just above tiles
        trigger.isTrigger = true;

        triggerZone.layer = LayerMask.NameToLayer("Plot");

        // 🔷 Tile setup
        float offset = gridSize / 2f - 0.5f;
        occupiedTiles = new bool[gridSize, gridSize];
        tileGrid = new Tile[gridSize, gridSize];

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                Vector3 position = transform.position + new Vector3(x - offset, 0, z - offset);
                GameObject tileObj = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                tileObj.layer = LayerMask.NameToLayer("Tile");

                Tile tileScript = tileObj.AddComponent<Tile>();
                tileScript.Init(new Vector2Int(x, z), this);
                tileGrid[x, z] = tileScript;
            }
        }

        // 🔊 Audio
        audioSource = GetComponent<AudioSource>();

        // 🧩 UI init
        if (selectedCuboidUIPanel != null)
        {
            selectedCuboidUIPanel.SetActive(false);
        }

        if (buildingButtonsPanel != null)
        {
            buildingButtonsPanel.SetActive(true);
        }
    }

    public void SetActive(bool state)
    {
        IsActive = state;

        if (!state)
        {
            ClearHighlights();
            hasSelectedCuboid = false;

            if (selectedCuboidUIPanel != null)
                selectedCuboidUIPanel.SetActive(false);

            if (upgradeButton != null)
                upgradeButton.gameObject.SetActive(false);
        }
        else
        {
            // Optional: show building panel again
            if (buildingButtonsPanel != null)
                buildingButtonsPanel.SetActive(true);

            if (hasSelectedCuboid)
                Debug.Log("🟢 Active plot and cuboid selected: CanPlace should be true");
        }
    }

    void Update()
    {
        if (!IsActive)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            ClearHighlights();
            return;
        }

        HandleRotation();

        if (!CanPlace)
        {
            Debug.LogWarning("⛔ Can't place — CanPlace is false");
            ClearHighlights();
            return;
        }

        // Exclude Ghost and Ignore Raycast layers
        int raycastMask = ~LayerMask.GetMask("Ghost", "Ignore Raycast");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f, raycastMask);

        bool hitAnyTile = false;

        foreach (RaycastHit hit in hits)
        {
            GameObject hitObject = hit.collider.gameObject;
            string layerName = LayerMask.LayerToName(hitObject.layer);
            Debug.Log($"🧲 Ray hit: {hitObject.name} (Layer: {layerName})");

            Tile tile = hitObject.GetComponent<Tile>();
            if (tile != null && tile.GridManager == this)
            {
                hitAnyTile = true;

                bool isValid = HighlightTiles(tile.gridPosition.x, tile.gridPosition.y);

                if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log($"🖱️ Clicked on tile ({tile.gridPosition.x},{tile.gridPosition.y}) — valid: {isValid}");
                    TryPlaceCuboidAt(tile.gridPosition.x, tile.gridPosition.y);
                }

                ShowGhost(tile.gridPosition.x, tile.gridPosition.y, cuboidTypes[selectedIndex], isRotated, isValid);
                return;
            }
        }

        if (Input.GetMouseButtonDown(0) && !hitAnyTile)
        {
            Debug.LogWarning("❌ Clicked but no tile was hit — something blocked it.");

            RaycastHit[] allHits = Physics.RaycastAll(ray, 100f);
            foreach (var hit in allHits)
            {
                string name = hit.collider.name;
                string layer = LayerMask.LayerToName(hit.collider.gameObject.layer);
                Debug.Log($"📦 Click possibly blocked by: {name} (Layer: {layer})");
            }
        }

        ClearHighlights();
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
        Debug.Log("📦 TryPlaceCuboidAt: Start");

        if (!CanPlace)
        {
            Debug.LogWarning("❌ TryPlaceCuboidAt called but CanPlace is false.");
            return;
        }

        if (cuboidTypes == null || cuboidTypes.Length == 0)
        {
            Debug.LogError("❌ cuboidTypes array is null or empty.");
            return;
        }

        if (selectedIndex < 0 || selectedIndex >= cuboidTypes.Length)
        {
            Debug.LogError($"❌ Invalid selectedIndex: {selectedIndex}");
            return;
        }

        CuboidType current = cuboidTypes[selectedIndex];
        int length = isRotated ? current.width : current.length;
        int width = isRotated ? current.length : current.width;

        for (int x = startX; x < startX + length; x++)
        {
            for (int z = startZ; z < startZ + width; z++)
            {
                if (x < 0 || z < 0 || x >= gridSize || z >= gridSize)
                {
                    Debug.LogWarning($"❌ Out of bounds: ({x},{z})");
                    return;
                }

                if (occupiedTiles[x, z])
                {
                    Debug.LogWarning($"❌ Tile already occupied at: ({x},{z})");
                    return;
                }
            }
        }

        float offset = gridSize / 2f - 0.5f;
        Vector3 spawnPos = transform.position + new Vector3(
            startX + length / 2f - 0.5f - offset,
            current.height / 2f,
            startZ + width / 2f - 0.5f - offset
        );

        Quaternion rotation = isRotated ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
        GameObject placed = Instantiate(current.prefab, spawnPos, rotation);

        // ✅ Set the layer to avoid blocking raycasts later
        SetLayerRecursive(placed, LayerMask.NameToLayer("Placed"));

        SelectableCuboid selectable = placed.AddComponent<SelectableCuboid>();
        selectable.cuboidName = current.name;
        selectable.infoPanel = SelectedCuboidUIPanel;
        selectable.infoDisplay = SelectedCuboidInfoText;
        selectable.upgradeButton = UpgradeButton;

        for (int x = startX; x < startX + length; x++)
        {
            for (int z = startZ; z < startZ + width; z++)
            {
                occupiedTiles[x, z] = true;
            }
        }

        ClearHighlights();

        if (placementSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(placementSound);
        }

        Debug.Log($"✅ Placed {current.name} at ({startX},{startZ})");
    }

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
            Debug.Log($"✅ Ghost '{obj.name}' set to layer: {LayerMask.LayerToName(layer)}");
        }
    }

    public void SetSelectedCuboid(int index)
    {
        if (index >= 0 && index < cuboidTypes.Length)
        {
            selectedIndex = index;
            hasSelectedCuboid = true;
            ClearHighlights();
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
                if (x >= gridSize || z >= gridSize || occupiedTiles[x, z])
                {
                    validPlacement = false;
                    break;
                }
            }
        }

        Color highlightColor = validPlacement ? Color.green : Color.red;

        for (int x = startX; x < startX + length; x++)
        {
            for (int z = startZ; z < startZ + width; z++)
            {
                if (x < gridSize && z < gridSize)
                {
                    tileGrid[x, z].SetHighlight(highlightColor);
                }
            }
        }

        return validPlacement;
    }

    public void ClearHighlights()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                if (tileGrid[x, z] != null)
                {
                    tileGrid[x, z].ResetColor();
                }
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

            // Set to Ghost layer
            SetLayerRecursive(ghostObject, LayerMask.NameToLayer("Ghost"));

            ApplyGhostMaterial(ghostObject, isValid);
            Debug.Log($"👻 Spawned new ghost: {ghostName} at {ghostPos} (valid: {isValid})");
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
            Debug.Log($"🔇 Disabled collider on ghost child: {collider.gameObject.name}");
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
}
