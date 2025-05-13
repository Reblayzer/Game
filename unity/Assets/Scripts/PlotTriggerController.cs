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
        // ignore clicks over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // if you clicked *through* to your placed building (Placed layer), skip the plot logic:
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
        foreach (var h in hits)
        {
            if (h.collider.gameObject.layer == LayerMask.NameToLayer("Placed"))
                return; // let the building handle the click
        }

        // otherwise, this is a ground/plot click — continue as before:
        if (_grid != null)
            PlotSelector.Instance.SelectPlot(_grid);

        if (MapUIController.I != null && MapUIController.I.IsMapOpen)
            return;

        if (_buildToggle != null && _buildToggle.isOn)
        {
            // placement-mode click logic...
            if (_grid.IsInEditMode() && _grid.InPlacementPhase)
            {
                int tileMask = LayerMask.GetMask("Tile");
                if (Physics.Raycast(ray, out var hit, 100f, tileMask))
                {
                    var tile = hit.collider.GetComponent<Tile>();
                    if (tile != null && tile.GridManager == _grid)
                        _grid.TryPlaceCuboidAt(tile.gridPosition.x, tile.gridPosition.y);
                }
            }
        }
    }
}