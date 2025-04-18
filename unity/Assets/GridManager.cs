using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class CuboidType
{
    public string name;
    public GameObject prefab;
    public int length = 1;
    public int width = 1;
    public int height = 1;
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
    public GameObject selectedCuboidUIPanel;
    public TMP_Text selectedCuboidInfoText;
    public Button upgradeButton;

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

    public bool CanPlace => buildingButtonsPanel != null
                             && buildingButtonsPanel.activeSelf
                             && hasSelectedCuboid;

    void Start()
    {
        float offset = gridSize / 2f - 0.5f;

        occupiedTiles = new bool[gridSize, gridSize];
        tileGrid = new Tile[gridSize, gridSize];

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                Vector3 position = new Vector3(x - offset, 0, z - offset);
                GameObject tileObj = Instantiate(tilePrefab, position, Quaternion.identity, transform);

                Tile tileScript = tileObj.AddComponent<Tile>();
                tileScript.Init(new Vector2Int(x, z), this);
                tileGrid[x, z] = tileScript;
            }
        }

        audioSource = GetComponent<AudioSource>();

        // Hide UI panel at start
        if (selectedCuboidUIPanel != null)
        {
            selectedCuboidUIPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            ClearHighlights();
            return;
        }

        HandleRotation();

        if (!CanPlace)
        {
            ClearHighlights();
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Tile tile = hit.collider.GetComponent<Tile>();
            if (tile != null)
            {
                bool isValid = HighlightTiles(tile.gridPosition.x, tile.gridPosition.y);
                ShowGhost(tile.gridPosition.x, tile.gridPosition.y, cuboidTypes[selectedIndex], isRotated, isValid);
            }
        }
        else
        {
            ClearHighlights();
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
        if (!CanPlace)
            return;

        CuboidType current = cuboidTypes[selectedIndex];
        int length = isRotated ? current.width : current.length;
        int width = isRotated ? current.length : current.width;

        for (int x = startX; x < startX + length; x++)
        {
            for (int z = startZ; z < startZ + width; z++)
            {
                if (x >= gridSize || z >= gridSize || occupiedTiles[x, z])
                {
                    Debug.Log("Can't place cuboid: space is occupied or out of bounds.");
                    return;
                }
            }
        }

        float offset = gridSize / 2f - 0.5f;
        Vector3 spawnPos = new Vector3(
            startX + length / 2f - 0.5f - offset,
            current.height / 2f,
            startZ + width / 2f - 0.5f - offset
        );

        Quaternion rotation = isRotated ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
        GameObject placed = Instantiate(current.prefab, spawnPos, rotation);

        SelectableCuboid selectable = placed.AddComponent<SelectableCuboid>();
        selectable.cuboidName = current.name;
        selectable.infoPanel = selectedCuboidUIPanel;
        selectable.infoDisplay = selectedCuboidInfoText;
        selectable.upgradeButton = upgradeButton;

        for (int x = startX; x < startX + length; x++)
        {
            for (int z = startZ; z < startZ + width; z++)
            {
                occupiedTiles[x, z] = true;
            }
        }

        if (placementSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(placementSound);
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
                tileGrid[x, z].ResetColor();
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

        Vector3 ghostPos = new Vector3(
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
}
