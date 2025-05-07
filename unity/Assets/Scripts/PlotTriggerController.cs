using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class PlotTriggerController : MonoBehaviour
{
    [HideInInspector] public GameObject markerCanvas;
    public Ownership ownership = Ownership.Unclaimed;
    private Toggle _buildToggle;
    private GridManager _grid;

    void Awake()
    {
        _buildToggle = PlotSelector.Instance?.buildToggle;
        _grid = GetComponentInParent<GridManager>();
    }

    void Update()
    {
        // only in build→placement mode
        if (_buildToggle != null
          && _buildToggle.isOn
          && _grid.IsInEditMode()
          && _grid.InPlacementPhase
          && Input.GetMouseButtonDown(0))
        {
            // raycast *only* against your Tile layer
            int tileMask = 1 << LayerMask.NameToLayer("Tile");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, tileMask))
            {
                var tile = hit.collider.GetComponent<Tile>();
                if (tile != null && tile.GridManager == _grid)
                    _grid.TryPlaceCuboidAt(tile.gridPosition.x, tile.gridPosition.y);
            }
        }
    }

    void OnMouseDown()
    {
        // 1) ignore UI clicks
        if (EventSystem.current != null
            && EventSystem.current.IsPointerOverGameObject())
            return;

        // 2) always re-select plot
        if (_grid != null)
            PlotSelector.Instance.SelectPlot(_grid);

        // 3) if map is open, bail
        if (MapUIController.I != null && MapUIController.I.IsMapOpen)
            return;

        if (_buildToggle != null
            && _buildToggle.isOn
            && _grid.IsInEditMode()
            && _grid.InPlacementPhase)
        {
            // raycast down to figure out which tile
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100f,
                                LayerMask.GetMask("Tile")))
            {
                var tile = hit.collider.GetComponent<Tile>();
                if (tile != null && tile.GridManager == _grid)
                {
                    _grid.TryPlaceCuboidAt(
                        tile.gridPosition.x,
                        tile.gridPosition.y
                    );
                }
            }
            return;
        }

        // 5) otherwise, do your normal marker-toggle logic:
        if (markerCanvas == null) return;
        if (markerCanvas.activeSelf)
        {
            markerCanvas.SetActive(false);
            return;
        }

        // hide all other markers
        var all = Object.FindObjectsByType<PlotTriggerController>(
            FindObjectsInactive.Include, FindObjectsSortMode.None
        );
        foreach (var other in all)
            if (other != this && other.markerCanvas != null)
                other.markerCanvas.SetActive(false);

        markerCanvas.SetActive(true);
    }
}