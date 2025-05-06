/* using UnityEngine;
using UnityEngine.EventSystems;

public class GhostCuboidController : MonoBehaviour
{
  [Tooltip("A semi-transparent material you assign in the Inspector")]
  public Material ghostMaterial;

  private GameObject _ghostInstance;
  private GameObject _blueprintPrefab;
  private GridManager _activeGrid;
  private Camera _camera;

  void Awake()
  {
    _camera = Camera.main;
  }

  void Update()
  {
    // if we’re not previewing, nothing to do
    if (_ghostInstance == null || _activeGrid == null) return;

    // raycast into the scene
    Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
    if (!Physics.Raycast(ray, out var hit, 100f, LayerMask.GetMask("Grid")))
      return;

    // snap that point to your grid’s cell center
    Vector3 snapped = _activeGrid.SnapToGrid(hit.point);
    _ghostInstance.transform.position = snapped;

    // on left‐click (and not over UI) → place & finish
    if (Input.GetMouseButtonDown(0) &&
        EventSystem.current != null &&
        !EventSystem.current.IsPointerOverGameObject())
    {
      // your existing placement call:
      _activeGrid.PlaceCuboid(snapped, _blueprintPrefab);

      // destroy ghost & reset
      Destroy(_ghostInstance);
      _ghostInstance = null;
      _blueprintPrefab = null;
      _activeGrid = null;
    }
  }


  public void StartPreview(GameObject blueprintPrefab, GridManager grid)
  {
    // if one was already running, kill it
    if (_ghostInstance != null) Destroy(_ghostInstance);

    _blueprintPrefab = blueprintPrefab;
    _activeGrid = grid;

    // spawn a copy
    _ghostInstance = Instantiate(blueprintPrefab, Vector3.zero, Quaternion.identity);

    // apply your ghost material to all renderers
    foreach (var r in _ghostInstance.GetComponentsInChildren<Renderer>())
      r.material = ghostMaterial;
  }

  public void CancelPreview()
  {
    if (_ghostInstance != null) Destroy(_ghostInstance);
    _ghostInstance = null;
    _activeGrid = null;
    _blueprintPrefab = null;
  }
}
 */